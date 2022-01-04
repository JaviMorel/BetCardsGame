using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string VersionName = "0.1";

    [SerializeField] private GameObject UsernameMenu = null;
    [SerializeField] private GameObject ConnectPanel = null;

    [SerializeField] private InputField UsernameInput = null;
    [SerializeField] private InputField CreateGameInput = null;
    [SerializeField] private InputField JoinGameInput = null;

    [SerializeField] private GameObject SetUsernameButton = null;

    [SerializeField] private const int MaxPlayersInRoom = 8;

    [SerializeField] private GameObject ExplainCanvas = null;
    [SerializeField] private GameObject ExplainPag1 = null;
    [SerializeField] private GameObject ExplainPag2 = null;

    [SerializeField] private GameObject ErrorHost = null;
    private bool ErrorHostActivated = false;
    [SerializeField] private GameObject ErrorJoin1 = null;
    private bool ErrorJoin1Activated = false;
    private int ExplainPag = 0;
    private void Awake()
    {
        
        PhotonNetwork.ConnectUsingSettings(VersionName);
    }

    private void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        Debug.Log("Connected");
    }

    // Start is called before the first frame update
    void Start()
    {
        ConnectPanel.SetActive(false);
        UsernameMenu.SetActive(true);
        ExplainPag = 0;
        ExplainCanvas.SetActive(false);
        SetUsernameButton.SetActive(false);
        ErrorHost.SetActive(false);
        ErrorJoin1.SetActive(false);
        ErrorHostActivated = false;
        ErrorJoin1Activated = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeUsernameInput()
    {
        if(UsernameInput.text.Length >= 3)
        {
            SetUsernameButton.SetActive(true);
        }
        else
        {
            SetUsernameButton.SetActive(false);
        }
    }

    public void SetUsername()
    {
        ConnectPanel.SetActive(true);
        UsernameMenu.SetActive(false);
        PhotonNetwork.playerName = UsernameInput.text;
    }


    public void CreateGame()
    {
        if (CreateGameInput.text == "")
        {
            if (!ErrorHostActivated)
            {
                ErrorHostActivated = true;
                StartCoroutine(ActiveErrorHost());
            }
            
            return;
        }
        PhotonNetwork.CreateRoom(CreateGameInput.text, new RoomOptions() { MaxPlayers = MaxPlayersInRoom },null);
        
    }

    private IEnumerator ActiveErrorHost()
    {
        ErrorHost.SetActive(true);
        yield return new WaitForSeconds(3);
        ErrorHost.SetActive(false);
        ErrorHostActivated = false;
    }

    private IEnumerator ActiveErrorJoin1()
    {
        ErrorJoin1.SetActive(true);
        yield return new WaitForSeconds(3);
        ErrorJoin1.SetActive(false);
        ErrorJoin1Activated = false;
    }


    public void JoinGame()
    {
        if (JoinGameInput.text == "")
        {
            if (!ErrorJoin1Activated)
            {
                ErrorJoin1Activated = true;
                StartCoroutine(ActiveErrorJoin1());
            }

            return;
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = MaxPlayersInRoom;
        PhotonNetwork.JoinOrCreateRoom(JoinGameInput.text, roomOptions, TypedLobby.Default);

    }

    private void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game");
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

    public void ExitApplication()
    {
        Application.Quit();
    }
}
