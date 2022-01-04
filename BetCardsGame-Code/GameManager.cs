using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;


public class GameManager : Photon.MonoBehaviour
{
    private PhotonView _photonView = null;

    public BetsManager betsManager = null;

    public GameObject StartCanvas = null;
    public GameObject HostCanvas = null;
    public GameObject ClientCanvas = null;

    public Text NumPlayersText = null;

    //public Text PingText = null;
    public GameObject DisconnectMenu = null;
    private bool PauseActive = false;

    //Feed
    public GameObject PlayerFeed;
    public GameObject FeedGrid;

    //Player names 
    public GameObject PlayerNameText;
    public GameObject PlayerNamesGrid;
    
    public Image BetsGameMode = null;

    public List<string> PlayersNamesPlaying = new List<string>();

    public bool GameStarted = false;

    [SerializeField] private GameObject ExplainCanvas = null;
    [SerializeField] private GameObject ExplainPag1 = null;
    [SerializeField] private GameObject ExplainPag2 = null;
    private int ExplainPag = 0;


    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();

        OpenStartMenu();

        UpdateNumPlayersText();

        //Add host name
        PlayersNamesPlaying.Add(PhotonNetwork.playerName);

        UpdatePlayerNamesGrid();
    }

    void Start()
    {
        ExplainPag = 0;
        ExplainCanvas.SetActive(false);
    }

    private void Update()
    {
        if (GameStarted)
        {
            ActiveDisconnectMenu();
        }

    }
    

    private void OpenStartMenu()
    {
        StartCanvas.SetActive(true);
        if (PhotonNetwork.isMasterClient)  //EL HOST
        {
            HostCanvas.SetActive(true);
            ClientCanvas.SetActive(false);
        }
        else //NO ERES EL HOST
        {
            HostCanvas.SetActive(false);
            ClientCanvas.SetActive(true);
        }
    }

    private void ActiveDisconnectMenu()
    {
        if(PauseActive && Input.GetKeyDown(KeyCode.Escape))
        {
            DisconnectMenu.SetActive(false);
            PauseActive = false;
        } else if (!PauseActive && Input.GetKeyDown(KeyCode.Escape))
        {
            DisconnectMenu.SetActive(true);
            PauseActive = true;
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Menu");
    }

    private void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        GameObject obj = Instantiate(PlayerFeed, new Vector2(0, 0), Quaternion.identity);
        obj.transform.SetParent(FeedGrid.transform, false);
        obj.GetComponent<Text>().text = player.NickName + " joined the game";
        obj.GetComponent<Text>().color = Color.green;
        
        if (PhotonNetwork.isMasterClient)
        {
            PlayersNamesPlaying.Add(player.NickName);

            string[] playerListArray = new string[PlayersNamesPlaying.Count];
            for(int i = 0; i < PlayersNamesPlaying.Count; ++i)
            {
                playerListArray[i] = PlayersNamesPlaying[i];
            }

            _photonView.RPC(nameof(RPC_OnReceivePlayersNamesList), PhotonTargets.All, new object[] { playerListArray });
        }

        UpdateNumPlayersText();
    }

    private void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        GameObject obj = Instantiate(PlayerFeed, new Vector2(0, 0), Quaternion.identity);
        obj.transform.SetParent(FeedGrid.transform, false);
        obj.GetComponent<Text>().text = player.NickName + " left the game";
        obj.GetComponent<Text>().color = Color.red;

        
        if (PhotonNetwork.isMasterClient)
        {
            PlayersNamesPlaying.Remove(player.NickName);

            string[] playerListArray = new string[PlayersNamesPlaying.Count];
            for (int i = 0; i < PlayersNamesPlaying.Count; ++i)
            {
                playerListArray[i] = PlayersNamesPlaying[i];
            }
            _photonView.RPC(nameof(RPC_OnReceivePlayersNamesList), PhotonTargets.All, new object[] { playerListArray });

            if (GameStarted)
            {
                betsManager.SetPlayerLoseDisconnect(player.NickName);
            }
        }

        UpdateNumPlayersText();

        
    }

    [PunRPC]
    private void RPC_OnReceivePlayersNamesList(string[] playerList)
    {
        PlayersNamesPlaying.Clear();

        for (int i = 0; i < playerList.Length; ++i)
        {
            PlayersNamesPlaying.Add(playerList[i]);
        }

        UpdatePlayerNamesGrid();
    }


    //Cuando se va el host, se cambia el master
    private void OnMasterClientSwitched(PhotonPlayer newMaster)
    {
        Debug.Log("The new masterclient is: " + newMaster.NickName);

        if (!GameStarted)
        {
            OpenStartMenu();
        }
        else
        {

        }
    }

    private void UpdateNumPlayersText()
    {
        NumPlayersText.text = "Players: " + PhotonNetwork.playerList.Length;
    }
    private void UpdatePlayerNamesGrid()
    {
        Text[] playerNames_list = PlayerNamesGrid.GetComponentsInChildren<Text>();
        for (int i = 0; i < playerNames_list.Length; ++i)
        {
            Destroy(playerNames_list[i].gameObject);
        }

        //Creo los nombres
        for (int i = 0; i< PlayersNamesPlaying.Count; ++i)
        {
            GameObject obj = Instantiate(PlayerNameText, new Vector2(0, 0), Quaternion.identity);
            obj.transform.SetParent(PlayerNamesGrid.transform, false);
            obj.GetComponent<Text>().text = PlayersNamesPlaying[i] + "";
        }

    }


    public void SetGameStarted()
    {
        if (PhotonNetwork.playerList.Length < 2)
            return;

        PhotonNetwork.room.IsVisible = false;
        PhotonNetwork.room.IsOpen = false;
        _photonView.RPC(nameof(RPC_OnReceiveGameStarted), PhotonTargets.All, new object[] { true });
    }

    [PunRPC]
    private void RPC_OnReceiveGameStarted(bool bGameStarted)
    {
        GameStarted = bGameStarted;
        StartGame();
    }


    public void StartGame()
    {
        if (betsManager == null)
        {
            print("ERROR, BETSMANAGER DOESNT FOUND");
            return;
        }

        betsManager.gameManager = this;

        betsManager.PlayersNamesPlaying = new List<string>();
        for (int i = 0; i < PlayersNamesPlaying.Count; ++i)
        {
            betsManager.PlayersNamesPlaying.Add(PlayersNamesPlaying[i]);
        }

        betsManager.NumPlayers = betsManager.PlayersNamesPlaying.Count;

        betsManager.SetPlayerNamesInGame();
        betsManager.OpenBetsHUD();
        
        StartCanvas.SetActive(false);  
    }

    public void BackToMenu()
    {
        GameStarted = false;
        OpenStartMenu();
    }

    private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }

    public void OpenExplainPage()
    {
        if (ExplainPag == 0)
        {
            ExplainPag1.SetActive(true);
            ExplainPag2.SetActive(false);
            ExplainCanvas.SetActive(true);
        }
        else
        {
            ExplainPag2.SetActive(true);
            ExplainPag1.SetActive(false);
            ExplainCanvas.SetActive(true);
        }
    }

    public void CloseExplainPage()
    {
        ExplainCanvas.SetActive(false);
    }

    public void NextPage()
    {
        ExplainPag = 1;
        ExplainPag2.SetActive(true);
        ExplainPag1.SetActive(false);
        ExplainCanvas.SetActive(true);
    }

    public void BackPage()
    {
        ExplainPag = 0;
        ExplainPag1.SetActive(true);
        ExplainPag2.SetActive(false);
        ExplainCanvas.SetActive(true);
    }

}

