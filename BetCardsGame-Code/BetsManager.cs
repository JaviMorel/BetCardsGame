using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum GameState
{
    NOSTARTED,
    PREINITIAL,
    TURN,
    WAITING,
    CONTINUE,
    FINISHED
};


[RequireComponent(typeof(PhotonView))]
public class BetsManager : Photon.MonoBehaviour
{
    private PhotonView _photonView;
    public GameManager gameManager;

    private GameState gameState = GameState.NOSTARTED;

    private Player LocalPlayer = null;
    private int LocalPlayerNum = -1;
    private int LocalBet = -1;
    private int CurrentBetNum = 0;
    private bool CanChooseCard = false;
    private Card CardChosen = null;
    private List<Card> FirstCardsOfAllPlayers = new List<Card>();
    private int CurrentCards = 5;

    private List<Card> CardsChosenByPlayers = new List<Card>();

    private const int MaxNumLives = 5;
    private const int MaxNumCardsIni = 5;
    
    private int PlayerTurn = 0;
    private int NumCards = 5;
    private bool[] PlayerFinish; //Terminar de recibir las cartas
    private bool[] PlayerFinishWaitSeconds; //Terminar de recibir las cartas
    private bool[] PlayerFinishUpdateLives; //Terminar de recibir las cartas

    private int SumBets = 0;
    private int InitialPlayerTurn = 0;

    public int NumPlayers = 0;
    public List<string> PlayersNamesPlaying = new List<string>();

    private List<int> PlayersWithLives = new List<int>();  //Jugadores aun con vidas

    //---------------CARDS------------------
    public List<Card> Cards = new List<Card>();
    public Sprite BackImageCard = null;
    //----------------------------------------


    public List<Player> Players = new List<Player>();

    //--------HUD--------

    public GameObject BetsHUD = null;
    public GameObject BetsHostHUD = null;
    public GameObject BetsClientHUD = null;

    public List<Text> PlayerNamesInGame = new List<Text>();

    public List<Image> CardsImagePlayer1 = new List<Image>();
    public List<Image> CardsImagePlayer2 = new List<Image>();
    public List<Image> CardsImagePlayer3 = new List<Image>();
    public List<Image> CardsImagePlayer4 = new List<Image>();
    public List<Image> CardsImagePlayer5 = new List<Image>();
    public List<Image> CardsImagePlayer6 = new List<Image>();
    public List<Image> CardsImagePlayer7 = new List<Image>();
    public List<Image> CardsImagePlayer8 = new List<Image>();

    public List<Image> OthersFirstCards = new List<Image>();

    public Image CenterCard = null;

    public GameObject BetsPanelHUD = null;
    public List<GameObject> ButtonBets = new List<GameObject>();
    public GameObject LastBetsPanelHUD = null;

    public List<GameObject> TurnImages = new List<GameObject>();

    public GameObject ChooseCardPanel = null;

    public GameObject ContinueHUD = null;

    public Text LivesText = null;
    public Text BetText = null;
    public Text RoundsWinText = null;
    public Text WinnerText = null;

    public GameObject WinPanel = null;
    public Text WinText = null;
    public GameObject HostWinPanel = null;

    //------------------

    private bool Checking = false;


    // Start is called before the first frame update
    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();

        for (int i = 0; i < PlayerNamesInGame.Count; ++i)
        {
            PlayerNamesInGame[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < CardsImagePlayer1.Count; ++i)
        {
            CardsImagePlayer1[i].gameObject.SetActive(false);
            CardsImagePlayer2[i].gameObject.SetActive(false);
            CardsImagePlayer3[i].gameObject.SetActive(false);
            CardsImagePlayer4[i].gameObject.SetActive(false);
            CardsImagePlayer5[i].gameObject.SetActive(false);
            CardsImagePlayer6[i].gameObject.SetActive(false);
            CardsImagePlayer7[i].gameObject.SetActive(false);
            CardsImagePlayer8[i].gameObject.SetActive(false);
        }

        CenterCard.gameObject.SetActive(false);

        BetsPanelHUD.SetActive(false);
        LastBetsPanelHUD.SetActive(false);

        for(int i = 0; i < TurnImages.Count; ++i)
        {
            TurnImages[i].SetActive(false);
        }

        ChooseCardPanel.SetActive(false);

        ContinueHUD.SetActive(false);

        LivesText.gameObject.SetActive(false);
        BetText.gameObject.SetActive(false);
        RoundsWinText.gameObject.SetActive(false);
        WinnerText.gameObject.SetActive(false);

        WinPanel.gameObject.SetActive(false);

        BetsHUD.SetActive(false);

        CurrentCards = MaxNumCardsIni;
        NumCards = MaxNumCardsIni;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.isMasterClient)
        {
            if (gameState != GameState.NOSTARTED)
            {
                if (!Checking)
                {
                    Checking = true;
                    StartCoroutine(UpdateGame());
                }
            }
            
        }
    }

    private IEnumerator UpdateGame()
    {
        yield return new WaitForSeconds(10);
        switch (gameState)
        {
            case GameState.PREINITIAL:
                OpenBetsHUD();
                break;
            case GameState.TURN:
                if (!PlayersWithLives.Contains(PlayerTurn))
                {
                    ++PlayerTurn;
                    if (PlayerTurn >= NumPlayers)
                        PlayerTurn = 0;
                    _photonView.RPC(nameof(RPC_PlayerTurn), PhotonTargets.All, new object[] { PlayerTurn, SumBets, -1, -1 });
                }
                break;
            case GameState.CONTINUE:
                BetsClientHUD.SetActive(false);
                ContinueHUD.SetActive(true);
                break;
            case GameState.FINISHED:
                BetsHUD.SetActive(true);
                break;
            case GameState.WAITING:
                break;
            case GameState.NOSTARTED:
                break;
            default:
                print("ERROR, GAMESTATE INVALIDO");
                break;
        }
        Checking = false;
    }

    public void OpenBetsHUD()
    {
        gameState = GameState.PREINITIAL;
        if (PhotonNetwork.isMasterClient)
        {
            BetsHostHUD.SetActive(true);
            BetsClientHUD.SetActive(false);
        }
        else
        {
            BetsHostHUD.SetActive(false);
            BetsClientHUD.SetActive(true);
        }
        BetsHUD.SetActive(true);
    }

    public void SetPlayerNamesInGame()
    {
        //Busco mi nombre
        int p = 0;

        for (p = 0; p < PlayersNamesPlaying.Count; ++p)
        {
            if (PlayersNamesPlaying[p].CompareTo(PhotonNetwork.playerName) == 0)
            {
                break;
            }
        }

        if (p >= PlayersNamesPlaying.Count)
        {
            print("ERROR, Nombre no encontrado");
            return;
        }

        LocalPlayerNum = p;
        CurrentBetNum = 0; 


        for(int i = 0; i < PlayerNamesInGame.Count; ++i)
        {
            PlayerNamesInGame[i].color = Color.white;
        }

        PlayerNamesInGame[0].text = PhotonNetwork.playerName;
        PlayerNamesInGame[0].gameObject.SetActive(true);

        switch (p)
        {
            case 0:
                PlayerNamesInGame[1].text = PlayersNamesPlaying[1];
                PlayerNamesInGame[1].gameObject.SetActive(true);
                if (PlayersNamesPlaying.Count >= 3)
                {
                    PlayerNamesInGame[2].text = PlayersNamesPlaying[2];
                    PlayerNamesInGame[2].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[2].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 4)
                {
                    PlayerNamesInGame[3].text = PlayersNamesPlaying[3];
                    PlayerNamesInGame[3].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[3].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 5)
                {
                    PlayerNamesInGame[4].text = PlayersNamesPlaying[4];
                    PlayerNamesInGame[4].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[4].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 6)
                {
                    PlayerNamesInGame[5].text = PlayersNamesPlaying[5];
                    PlayerNamesInGame[5].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[5].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 7)
                {
                    PlayerNamesInGame[6].text = PlayersNamesPlaying[6];
                    PlayerNamesInGame[6].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[6].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 8)
                {
                    PlayerNamesInGame[7].text = PlayersNamesPlaying[7];
                    PlayerNamesInGame[7].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[7].gameObject.SetActive(false);
                break;
            case 1:
                if (PlayersNamesPlaying.Count >= 3)
                {
                    PlayerNamesInGame[1].text = PlayersNamesPlaying[2];
                    PlayerNamesInGame[1].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[1].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 4)
                {
                    PlayerNamesInGame[2].text = PlayersNamesPlaying[3];
                    PlayerNamesInGame[2].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[2].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 5)
                {
                    PlayerNamesInGame[3].text = PlayersNamesPlaying[4];
                    PlayerNamesInGame[3].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[3].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 6)
                {
                    PlayerNamesInGame[4].text = PlayersNamesPlaying[5];
                    PlayerNamesInGame[4].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[4].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 7)
                {
                    PlayerNamesInGame[5].text = PlayersNamesPlaying[6];
                    PlayerNamesInGame[5].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[5].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 8)
                {
                    PlayerNamesInGame[6].text = PlayersNamesPlaying[7];
                    PlayerNamesInGame[6].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[6].gameObject.SetActive(false);
                PlayerNamesInGame[7].text = PlayersNamesPlaying[0];
                PlayerNamesInGame[7].gameObject.SetActive(true);
                break;
            case 2:
                if (PlayersNamesPlaying.Count >= 4)
                {
                    PlayerNamesInGame[1].text = PlayersNamesPlaying[3];
                    PlayerNamesInGame[1].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[1].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 5)
                {
                    PlayerNamesInGame[2].text = PlayersNamesPlaying[4];
                    PlayerNamesInGame[2].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[2].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 6)
                {
                    PlayerNamesInGame[3].text = PlayersNamesPlaying[5];
                    PlayerNamesInGame[3].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[3].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 7)
                {
                    PlayerNamesInGame[4].text = PlayersNamesPlaying[6];
                    PlayerNamesInGame[4].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[4].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 8)
                {
                    PlayerNamesInGame[5].text = PlayersNamesPlaying[7];
                    PlayerNamesInGame[5].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[5].gameObject.SetActive(false);
                PlayerNamesInGame[6].text = PlayersNamesPlaying[0];
                PlayerNamesInGame[6].gameObject.SetActive(true);
                PlayerNamesInGame[7].text = PlayersNamesPlaying[1];
                PlayerNamesInGame[7].gameObject.SetActive(true);
                break;
            case 3:
                if (PlayersNamesPlaying.Count >= 5)
                {
                    PlayerNamesInGame[1].text = PlayersNamesPlaying[4];
                    PlayerNamesInGame[1].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[1].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 6)
                {
                    PlayerNamesInGame[2].text = PlayersNamesPlaying[5];
                    PlayerNamesInGame[2].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[2].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 7)
                {
                    PlayerNamesInGame[3].text = PlayersNamesPlaying[6];
                    PlayerNamesInGame[3].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[3].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 8)
                {
                    PlayerNamesInGame[4].text = PlayersNamesPlaying[7];
                    PlayerNamesInGame[4].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[4].gameObject.SetActive(false);
                PlayerNamesInGame[5].text = PlayersNamesPlaying[0];
                PlayerNamesInGame[5].gameObject.SetActive(true);
                PlayerNamesInGame[6].text = PlayersNamesPlaying[1];
                PlayerNamesInGame[6].gameObject.SetActive(true);
                PlayerNamesInGame[7].text = PlayersNamesPlaying[2];
                PlayerNamesInGame[7].gameObject.SetActive(true);
                break;
            case 4:
                if (PlayersNamesPlaying.Count >= 6)
                {
                    PlayerNamesInGame[1].text = PlayersNamesPlaying[5];
                    PlayerNamesInGame[1].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[1].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 7)
                {
                    PlayerNamesInGame[2].text = PlayersNamesPlaying[6];
                    PlayerNamesInGame[2].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[2].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 8)
                {
                    PlayerNamesInGame[3].text = PlayersNamesPlaying[7];
                    PlayerNamesInGame[3].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[3].gameObject.SetActive(false);
                PlayerNamesInGame[4].text = PlayersNamesPlaying[0];
                PlayerNamesInGame[4].gameObject.SetActive(true);
                PlayerNamesInGame[5].text = PlayersNamesPlaying[1];
                PlayerNamesInGame[5].gameObject.SetActive(true);
                PlayerNamesInGame[6].text = PlayersNamesPlaying[2];
                PlayerNamesInGame[6].gameObject.SetActive(true);
                PlayerNamesInGame[7].text = PlayersNamesPlaying[3];
                PlayerNamesInGame[7].gameObject.SetActive(true);
                break;
            case 5:
                if (PlayersNamesPlaying.Count >= 7)
                {
                    PlayerNamesInGame[1].text = PlayersNamesPlaying[6];
                    PlayerNamesInGame[1].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[1].gameObject.SetActive(false);
                if (PlayersNamesPlaying.Count >= 8)
                {
                    PlayerNamesInGame[2].text = PlayersNamesPlaying[7];
                    PlayerNamesInGame[2].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[2].gameObject.SetActive(false);
                PlayerNamesInGame[3].text = PlayersNamesPlaying[0];
                PlayerNamesInGame[3].gameObject.SetActive(true);
                PlayerNamesInGame[4].text = PlayersNamesPlaying[1];
                PlayerNamesInGame[4].gameObject.SetActive(true);
                PlayerNamesInGame[5].text = PlayersNamesPlaying[2];
                PlayerNamesInGame[5].gameObject.SetActive(true);
                PlayerNamesInGame[6].text = PlayersNamesPlaying[3];
                PlayerNamesInGame[6].gameObject.SetActive(true);
                PlayerNamesInGame[7].text = PlayersNamesPlaying[4];
                PlayerNamesInGame[7].gameObject.SetActive(true);
                break;
            case 6:
                if (PlayersNamesPlaying.Count >= 7)
                {
                    PlayerNamesInGame[1].text = PlayersNamesPlaying[7];
                    PlayerNamesInGame[1].gameObject.SetActive(true);
                }
                else
                    PlayerNamesInGame[1].gameObject.SetActive(false);
                PlayerNamesInGame[2].text = PlayersNamesPlaying[0];
                PlayerNamesInGame[2].gameObject.SetActive(true);
                PlayerNamesInGame[3].text = PlayersNamesPlaying[1];
                PlayerNamesInGame[3].gameObject.SetActive(true);
                PlayerNamesInGame[4].text = PlayersNamesPlaying[2];
                PlayerNamesInGame[4].gameObject.SetActive(true);
                PlayerNamesInGame[5].text = PlayersNamesPlaying[3];
                PlayerNamesInGame[5].gameObject.SetActive(true);
                PlayerNamesInGame[6].text = PlayersNamesPlaying[4];
                PlayerNamesInGame[6].gameObject.SetActive(true);
                PlayerNamesInGame[7].text = PlayersNamesPlaying[5];
                PlayerNamesInGame[7].gameObject.SetActive(true);
                break;
            case 7:
                PlayerNamesInGame[1].text = PlayersNamesPlaying[0];
                PlayerNamesInGame[1].gameObject.SetActive(true);
                PlayerNamesInGame[2].text = PlayersNamesPlaying[1];
                PlayerNamesInGame[2].gameObject.SetActive(true);
                PlayerNamesInGame[3].text = PlayersNamesPlaying[2];
                PlayerNamesInGame[3].gameObject.SetActive(true);
                PlayerNamesInGame[4].text = PlayersNamesPlaying[3];
                PlayerNamesInGame[4].gameObject.SetActive(true);
                PlayerNamesInGame[5].text = PlayersNamesPlaying[4];
                PlayerNamesInGame[5].gameObject.SetActive(true);
                PlayerNamesInGame[6].text = PlayersNamesPlaying[5];
                PlayerNamesInGame[6].gameObject.SetActive(true);
                PlayerNamesInGame[7].text = PlayersNamesPlaying[6];
                PlayerNamesInGame[7].gameObject.SetActive(true);
                break;
            default:
                print("ERROR,NUMERO INVALIDO");
                break;
        }


    }

    public void InicializeGame()
    {
        PlayerFinishUpdateLives = new bool[NumPlayers];
        PlayerFinish = new bool[NumPlayers];
        SumBets = 0;
        CardsChosenByPlayers = new List<Card>();
        PlayerTurn = 0;
        InitialPlayerTurn = PlayerTurn;
        Players = new List<Player>();
        PlayersWithLives = new List<int>();
        Player player = null;
        for (int i = 0; i < NumPlayers; ++i)
        {
            PlayersWithLives.Add(i);
            player = new Player(i, MaxNumLives);
            Players.Add(player);
            
        }
        

        int[] IDPlayersWithLives = new int[PlayersWithLives.Count];
        for (int i = 0; i < PlayersWithLives.Count; ++i)
        {
            IDPlayersWithLives[i] = PlayersWithLives[i];
        }

        _photonView.RPC(nameof(RPC_DistributePlayersWithLives), PhotonTargets.All, new object[] { IDPlayersWithLives });
        DistributeCards();

    }

    [PunRPC]
    private void RPC_DistributePlayersWithLives(int[] IDPlayersWithLives)
    {
        if (!gameManager.GameStarted) return;
        gameState = GameState.TURN;
        PlayersWithLives = new List<int>();

        for(int i = 0; i<IDPlayersWithLives.Length; ++i)
        {
            PlayersWithLives.Add(IDPlayersWithLives[i]);
        }
        
    }

    private void DistributeCards()
    {
        List<Card> CardsCopy = new List<Card>(Cards);
        Player p = null;

        int[] IDFirstCardsPlayers = new int[NumPlayers];
        for (int i = 0; i < NumPlayers; ++i)
        {
            IDFirstCardsPlayers[i] = -1;
            if (PlayersWithLives.Contains(i))
            {
                p = Players[i];
                p.Cards.Clear();

                int[] IDCards = new int[NumCards];
                for (int j = 0; j < NumCards; ++j)
                {
                    int randCard = Random.Range(0, CardsCopy.Count - 1);
                    Card c = CardsCopy[randCard];
                    CardsCopy.Remove(c);
                    p.Cards.Add(c);
                    IDCards[j] = c.ID;
                    if (j == 0)
                        IDFirstCardsPlayers[i] = c.ID;

                }
                _photonView.RPC(nameof(RPC_OnReceiveCards), PhotonTargets.All, new object[] { p.IDPlayer, p.Lives, IDCards });
            }
        }

        _photonView.RPC(nameof(RPC_DistribuiteCardsVisual), PhotonTargets.All, new object[] { IDFirstCardsPlayers});

    }

    [PunRPC]
    private void RPC_OnReceiveCards(int numPlayer, int lives,int[] IDCards)
    {
        if (!gameManager.GameStarted) return;
        if (numPlayer == LocalPlayerNum) //Si es mi personaje
        {
            UpdateLivesText(lives);
            UpdateBetText(-1);
            UpdateRoundsWinText();
            LocalPlayer = new Player(numPlayer, lives);
            LocalPlayer.Cards.Clear();
            for (int i = 0; i < IDCards.Length; ++i)
            {
                LocalPlayer.Cards.Add(Cards[IDCards[i]]);
            }
        }
    }

    private void UpdateLivesText(int lives)
    {
        LivesText.text = "Lives: " + lives;
        LivesText.gameObject.SetActive(true);
    }

    private void UpdateBetText(int bet)
    {
        if (PlayersWithLives.Contains(LocalPlayerNum))
        {
            BetText.text = "Bet: " + bet;
            BetText.gameObject.SetActive(true);
        }
        else
        {
            BetText.gameObject.SetActive(false);
        }
            
    }

    private void UpdateRoundsWinText()
    {
        if (PlayersWithLives.Contains(LocalPlayerNum))
        {
            RoundsWinText.text = "Win: " + CurrentBetNum;
            RoundsWinText.gameObject.SetActive(true);
        }
        else
        {
            RoundsWinText.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    private void RPC_DistribuiteCardsVisual(int[] IDCards)
    {
        if (!gameManager.GameStarted) return;
        FirstCardsOfAllPlayers = new List<Card>();
        for (int i = 0; i < NumPlayers; ++i)
        {
            if(IDCards[i] != -1)
            {
                Card c = new Card(Cards[IDCards[i]]);
                c.Owner = i;
                FirstCardsOfAllPlayers.Add(c);
            }
                
        }


        BetsHostHUD.SetActive(false);
        BetsClientHUD.SetActive(false);
        ContinueHUD.SetActive(false);

        ShowCards();
    }

    private void ShowCards()
    {
        if(NumCards == 1)
        {
            if(PlayersWithLives.Contains(LocalPlayerNum))
            {
                CardsImagePlayer1[0].sprite = BackImageCard;
                CardsImagePlayer1[0].gameObject.SetActive(true);
            }

            switch (LocalPlayerNum)
            {
                case 0:
                    if (PlayersWithLives.Contains(1)){
                        CardsImagePlayer2[0].sprite = FirstCardsOfAllPlayers[1].FrontImage;
                        CardsImagePlayer2[0].gameObject.SetActive(true);
                    }
                    
                    if (PlayersWithLives.Contains(2) && PlayersNamesPlaying.Count >= 3)
                    {
                        CardsImagePlayer3[0].sprite = FirstCardsOfAllPlayers[2].FrontImage;
                        CardsImagePlayer3[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer3[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(3) && PlayersNamesPlaying.Count >= 4)
                    {
                        CardsImagePlayer4[0].sprite = FirstCardsOfAllPlayers[3].FrontImage;
                        CardsImagePlayer4[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer4[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(4) && PlayersNamesPlaying.Count >= 5)
                    {
                        CardsImagePlayer5[0].sprite = FirstCardsOfAllPlayers[4].FrontImage;
                        CardsImagePlayer5[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer5[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(5) && PlayersNamesPlaying.Count >= 6)
                    {
                        CardsImagePlayer6[0].sprite = FirstCardsOfAllPlayers[5].FrontImage;
                        CardsImagePlayer6[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer6[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(6) && PlayersNamesPlaying.Count >= 7)
                    {
                        CardsImagePlayer7[0].sprite = FirstCardsOfAllPlayers[6].FrontImage;
                        CardsImagePlayer7[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer7[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                    {
                        CardsImagePlayer8[0].sprite = FirstCardsOfAllPlayers[7].FrontImage;
                        CardsImagePlayer8[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer8[0].gameObject.SetActive(false);
                    break;
                case 1:
                    if (PlayersWithLives.Contains(0))
                    {
                        CardsImagePlayer8[0].sprite = FirstCardsOfAllPlayers[0].FrontImage;
                        CardsImagePlayer8[0].gameObject.SetActive(true);
                    }
                    
                    if (PlayersWithLives.Contains(2) && PlayersNamesPlaying.Count >= 3)
                    {
                        CardsImagePlayer2[0].sprite = FirstCardsOfAllPlayers[2].FrontImage;
                        CardsImagePlayer2[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer2[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(3) && PlayersNamesPlaying.Count >= 4)
                    {
                        CardsImagePlayer3[0].sprite = FirstCardsOfAllPlayers[3].FrontImage;
                        CardsImagePlayer3[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer3[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(4) && PlayersNamesPlaying.Count >= 5)
                    {
                        CardsImagePlayer4[0].sprite = FirstCardsOfAllPlayers[4].FrontImage;
                        CardsImagePlayer4[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer4[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(5) && PlayersNamesPlaying.Count >= 6)
                    {
                        CardsImagePlayer5[0].sprite = FirstCardsOfAllPlayers[5].FrontImage;
                        CardsImagePlayer5[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer5[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(6) && PlayersNamesPlaying.Count >= 7)
                    {
                        CardsImagePlayer6[0].sprite = FirstCardsOfAllPlayers[6].FrontImage;
                        CardsImagePlayer6[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer6[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                    {
                        CardsImagePlayer7[0].sprite = FirstCardsOfAllPlayers[7].FrontImage;
                        CardsImagePlayer7[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer7[0].gameObject.SetActive(false);
                    break;
                case 2:
                    if (PlayersWithLives.Contains(0))
                    {
                        CardsImagePlayer7[0].sprite = FirstCardsOfAllPlayers[0].FrontImage;
                        CardsImagePlayer7[0].gameObject.SetActive(true);
                    }
                    if(PlayersWithLives.Contains(1))
                    {
                        CardsImagePlayer8[0].sprite = FirstCardsOfAllPlayers[1].FrontImage;
                        CardsImagePlayer8[0].gameObject.SetActive(true);
                    }

                    if (PlayersWithLives.Contains(3) && PlayersNamesPlaying.Count >= 4)
                    {
                        CardsImagePlayer2[0].sprite = FirstCardsOfAllPlayers[3].FrontImage;
                        CardsImagePlayer2[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer2[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(4) && PlayersNamesPlaying.Count >= 5)
                    {
                        CardsImagePlayer3[0].sprite = FirstCardsOfAllPlayers[4].FrontImage;
                        CardsImagePlayer3[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer3[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(5) && PlayersNamesPlaying.Count >= 6)
                    {
                        CardsImagePlayer4[0].sprite = FirstCardsOfAllPlayers[5].FrontImage;
                        CardsImagePlayer4[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer4[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(6) && PlayersNamesPlaying.Count >= 7)
                    {
                        CardsImagePlayer5[0].sprite = FirstCardsOfAllPlayers[6].FrontImage;
                        CardsImagePlayer5[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer5[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                    {
                        CardsImagePlayer6[0].sprite = FirstCardsOfAllPlayers[7].FrontImage;
                        CardsImagePlayer6[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer6[0].gameObject.SetActive(false);
                    break;
                case 3:
                    if (PlayersWithLives.Contains(0))
                    {
                        CardsImagePlayer6[0].sprite = FirstCardsOfAllPlayers[0].FrontImage;
                        CardsImagePlayer6[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(1))
                    {
                        CardsImagePlayer7[0].sprite = FirstCardsOfAllPlayers[1].FrontImage;
                        CardsImagePlayer7[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(2))
                    {
                        CardsImagePlayer8[0].sprite = FirstCardsOfAllPlayers[2].FrontImage;
                        CardsImagePlayer8[0].gameObject.SetActive(true);
                    }

                    if (PlayersWithLives.Contains(4) && PlayersNamesPlaying.Count >= 5)
                    {
                        CardsImagePlayer2[0].sprite = FirstCardsOfAllPlayers[4].FrontImage;
                        CardsImagePlayer2[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer2[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(5) && PlayersNamesPlaying.Count >= 6)
                    {
                        CardsImagePlayer3[0].sprite = FirstCardsOfAllPlayers[5].FrontImage;
                        CardsImagePlayer3[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer3[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(6) && PlayersNamesPlaying.Count >= 7)
                    {
                        CardsImagePlayer4[0].sprite = FirstCardsOfAllPlayers[6].FrontImage;
                        CardsImagePlayer4[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer4[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                    {
                        CardsImagePlayer5[0].sprite = FirstCardsOfAllPlayers[7].FrontImage;
                        CardsImagePlayer5[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer5[0].gameObject.SetActive(false);
                    break;
                case 4:
                    if (PlayersWithLives.Contains(0))
                    {
                        CardsImagePlayer5[0].sprite = FirstCardsOfAllPlayers[0].FrontImage;
                        CardsImagePlayer5[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(1))
                    {
                        CardsImagePlayer6[0].sprite = FirstCardsOfAllPlayers[1].FrontImage;
                        CardsImagePlayer6[0].gameObject.SetActive(true);
                    }
                    if(PlayersWithLives.Contains(2))
                    {
                        CardsImagePlayer7[0].sprite = FirstCardsOfAllPlayers[2].FrontImage;
                        CardsImagePlayer7[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(3))
                    {
                        CardsImagePlayer8[0].sprite = FirstCardsOfAllPlayers[3].FrontImage;
                        CardsImagePlayer8[0].gameObject.SetActive(true);
                    }
                    
                    if (PlayersWithLives.Contains(5) && PlayersNamesPlaying.Count >= 6)
                    {
                        CardsImagePlayer2[0].sprite = FirstCardsOfAllPlayers[5].FrontImage;
                        CardsImagePlayer2[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer2[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(6) && PlayersNamesPlaying.Count >= 7)
                    {
                        CardsImagePlayer3[0].sprite = FirstCardsOfAllPlayers[6].FrontImage;
                        CardsImagePlayer3[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer3[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                    {
                        CardsImagePlayer4[0].sprite = FirstCardsOfAllPlayers[7].FrontImage;
                        CardsImagePlayer4[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer4[0].gameObject.SetActive(false);
                    break;
                case 5:
                    if (PlayersWithLives.Contains(0))
                    {
                        CardsImagePlayer4[0].sprite = FirstCardsOfAllPlayers[0].FrontImage;
                        CardsImagePlayer4[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(1))
                    {
                        CardsImagePlayer5[0].sprite = FirstCardsOfAllPlayers[1].FrontImage;
                        CardsImagePlayer5[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(2))
                    {
                        CardsImagePlayer6[0].sprite = FirstCardsOfAllPlayers[2].FrontImage;
                        CardsImagePlayer6[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(3))
                    {
                        CardsImagePlayer7[0].sprite = FirstCardsOfAllPlayers[3].FrontImage;
                        CardsImagePlayer7[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(4))
                    {
                        CardsImagePlayer8[0].sprite = FirstCardsOfAllPlayers[4].FrontImage;
                        CardsImagePlayer8[0].gameObject.SetActive(true);
                    }

                    if (PlayersWithLives.Contains(6) && PlayersNamesPlaying.Count >= 7)
                    {
                        CardsImagePlayer2[0].sprite = FirstCardsOfAllPlayers[6].FrontImage;
                        CardsImagePlayer2[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer2[0].gameObject.SetActive(false);
                    if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                    {
                        CardsImagePlayer3[0].sprite = FirstCardsOfAllPlayers[7].FrontImage;
                        CardsImagePlayer3[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer3[0].gameObject.SetActive(false);
                    break;
                case 6:
                    if (PlayersWithLives.Contains(0))
                    {
                        CardsImagePlayer3[0].sprite = FirstCardsOfAllPlayers[0].FrontImage;
                        CardsImagePlayer3[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(1))
                    {
                        CardsImagePlayer4[0].sprite = FirstCardsOfAllPlayers[1].FrontImage;
                        CardsImagePlayer4[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(2))
                    {
                        CardsImagePlayer5[0].sprite = FirstCardsOfAllPlayers[2].FrontImage;
                        CardsImagePlayer5[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(3))
                    {
                        CardsImagePlayer6[0].sprite = FirstCardsOfAllPlayers[3].FrontImage;
                        CardsImagePlayer6[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(4))
                    {
                        CardsImagePlayer7[0].sprite = FirstCardsOfAllPlayers[4].FrontImage;
                        CardsImagePlayer7[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(5))
                    {
                        CardsImagePlayer8[0].sprite = FirstCardsOfAllPlayers[5].FrontImage;
                        CardsImagePlayer8[0].gameObject.SetActive(true);
                    }
                    

                    if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                    {
                        CardsImagePlayer2[0].sprite = FirstCardsOfAllPlayers[7].FrontImage;
                        CardsImagePlayer2[0].gameObject.SetActive(true);
                    }
                    else
                        CardsImagePlayer2[0].gameObject.SetActive(false);
                    break;
                case 7:
                    if (PlayersWithLives.Contains(0))
                    {
                        CardsImagePlayer2[0].sprite = FirstCardsOfAllPlayers[0].FrontImage;
                        CardsImagePlayer2[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(1))
                    {
                        CardsImagePlayer3[0].sprite = FirstCardsOfAllPlayers[1].FrontImage;
                        CardsImagePlayer3[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(2))
                    {
                        CardsImagePlayer4[0].sprite = FirstCardsOfAllPlayers[2].FrontImage;
                        CardsImagePlayer4[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(3))
                    {
                        CardsImagePlayer5[0].sprite = FirstCardsOfAllPlayers[3].FrontImage;
                        CardsImagePlayer5[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(4))
                    {
                        CardsImagePlayer6[0].sprite = FirstCardsOfAllPlayers[4].FrontImage;
                        CardsImagePlayer6[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(5))
                    {
                        CardsImagePlayer7[0].sprite = FirstCardsOfAllPlayers[5].FrontImage;
                        CardsImagePlayer7[0].gameObject.SetActive(true);
                    }
                    if (PlayersWithLives.Contains(6))
                    {
                        CardsImagePlayer8[0].sprite = FirstCardsOfAllPlayers[6].FrontImage;
                        CardsImagePlayer8[0].gameObject.SetActive(true);
                    }
                    
                    break;
                default:
                    print("ERROR,NUMERO INVALIDO");
                    break;
            }
        }
        else
        {
            for (int i = 0; i < NumCards; ++i)
            {
                if(PlayersWithLives.Contains(LocalPlayerNum))
                {
                    CardsImagePlayer1[i].sprite = LocalPlayer.Cards[i].FrontImage;
                    CardsImagePlayer1[i].gameObject.SetActive(true);
                }
                

                switch (LocalPlayerNum)
                {
                    case 0:
                        if (PlayersWithLives.Contains(1))
                        {
                            CardsImagePlayer2[i].sprite = BackImageCard;
                            CardsImagePlayer2[i].gameObject.SetActive(true);
                        }
                        
                        if (PlayersWithLives.Contains(2) && PlayersNamesPlaying.Count >= 3)
                        {
                            CardsImagePlayer3[i].sprite = BackImageCard;
                            CardsImagePlayer3[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer3[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(3) && PlayersNamesPlaying.Count >= 4)
                        {
                            CardsImagePlayer4[i].sprite = BackImageCard;
                            CardsImagePlayer4[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer4[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(4) && PlayersNamesPlaying.Count >= 5)
                        {
                            CardsImagePlayer5[i].sprite = BackImageCard;
                            CardsImagePlayer5[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer5[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(5) && PlayersNamesPlaying.Count >= 6)
                        {
                            CardsImagePlayer6[i].sprite = BackImageCard;
                            CardsImagePlayer6[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer6[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(6) && PlayersNamesPlaying.Count >= 7)
                        {
                            CardsImagePlayer7[i].sprite = BackImageCard;
                            CardsImagePlayer7[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer7[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                        {
                            CardsImagePlayer8[i].sprite = BackImageCard;
                            CardsImagePlayer8[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer8[i].gameObject.SetActive(false);
                        break;
                    case 1:
                        if (PlayersWithLives.Contains(0))
                        {
                            CardsImagePlayer8[i].sprite = BackImageCard;
                            CardsImagePlayer8[i].gameObject.SetActive(true);
                        }
                        
                        if (PlayersWithLives.Contains(2) && PlayersNamesPlaying.Count >= 3)
                        {
                            CardsImagePlayer2[i].sprite = BackImageCard;
                            CardsImagePlayer2[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer2[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(3) && PlayersNamesPlaying.Count >= 4)
                        {
                            CardsImagePlayer3[i].sprite = BackImageCard;
                            CardsImagePlayer3[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer3[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(4) && PlayersNamesPlaying.Count >= 5)
                        {
                            CardsImagePlayer4[i].sprite = BackImageCard;
                            CardsImagePlayer4[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer4[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(5) && PlayersNamesPlaying.Count >= 6)
                        {
                            CardsImagePlayer5[i].sprite = BackImageCard;
                            CardsImagePlayer5[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer5[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(6) && PlayersNamesPlaying.Count >= 7)
                        {
                            CardsImagePlayer6[i].sprite = BackImageCard;
                            CardsImagePlayer6[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer6[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                        {
                            CardsImagePlayer7[i].sprite = BackImageCard;
                            CardsImagePlayer7[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer7[i].gameObject.SetActive(false);
                        break;
                    case 2:
                        if (PlayersWithLives.Contains(0))
                        {
                            CardsImagePlayer7[i].sprite = BackImageCard;
                            CardsImagePlayer7[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(1)) 
                        {
                            CardsImagePlayer8[i].sprite = BackImageCard;
                            CardsImagePlayer8[i].gameObject.SetActive(true);
                        }

                        if (PlayersWithLives.Contains(3) && PlayersNamesPlaying.Count >= 4)
                        {
                            CardsImagePlayer2[i].sprite = BackImageCard;
                            CardsImagePlayer2[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer2[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(4) && PlayersNamesPlaying.Count >= 5)
                        {
                            CardsImagePlayer3[i].sprite = BackImageCard;
                            CardsImagePlayer3[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer3[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(5) && PlayersNamesPlaying.Count >= 6)
                        {
                            CardsImagePlayer4[i].sprite = BackImageCard;
                            CardsImagePlayer4[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer4[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(6) && PlayersNamesPlaying.Count >= 7)
                        {
                            CardsImagePlayer5[i].sprite = BackImageCard;
                            CardsImagePlayer5[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer5[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                        {
                            CardsImagePlayer6[i].sprite = BackImageCard;
                            CardsImagePlayer6[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer6[i].gameObject.SetActive(false);
                        break;
                    case 3:
                        if (PlayersWithLives.Contains(0))
                        {
                            CardsImagePlayer6[i].sprite = BackImageCard;
                            CardsImagePlayer6[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(1))
                        {
                            CardsImagePlayer7[i].sprite = BackImageCard;
                            CardsImagePlayer7[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(2))
                        {
                            CardsImagePlayer8[i].sprite = BackImageCard;
                            CardsImagePlayer8[i].gameObject.SetActive(true);
                        }
                        
                        if (PlayersWithLives.Contains(4) && PlayersNamesPlaying.Count >= 5)
                        {
                            CardsImagePlayer2[i].sprite = BackImageCard;
                            CardsImagePlayer2[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer2[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(5) && PlayersNamesPlaying.Count >= 6)
                        {
                            CardsImagePlayer3[i].sprite = BackImageCard;
                            CardsImagePlayer3[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer3[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(6) && PlayersNamesPlaying.Count >= 7)
                        {
                            CardsImagePlayer4[i].sprite = BackImageCard;
                            CardsImagePlayer4[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer4[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                        {
                            CardsImagePlayer5[i].sprite = BackImageCard;
                            CardsImagePlayer5[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer5[i].gameObject.SetActive(false);
                        break;
                    case 4:
                        if (PlayersWithLives.Contains(0))
                        {
                            CardsImagePlayer5[i].sprite = BackImageCard;
                            CardsImagePlayer5[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(1))
                        {
                            CardsImagePlayer6[i].sprite = BackImageCard;
                            CardsImagePlayer6[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(2))
                        {
                            CardsImagePlayer7[i].sprite = BackImageCard;
                            CardsImagePlayer7[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(3))
                        {
                            CardsImagePlayer8[i].sprite = BackImageCard;
                            CardsImagePlayer8[i].gameObject.SetActive(true);
                        }
                        
                        if (PlayersWithLives.Contains(5) && PlayersNamesPlaying.Count >= 6)
                        {
                            CardsImagePlayer2[i].sprite = BackImageCard;
                            CardsImagePlayer2[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer2[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(6) && PlayersNamesPlaying.Count >= 7)
                        {
                            CardsImagePlayer3[i].sprite = BackImageCard;
                            CardsImagePlayer3[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer3[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                        {
                            CardsImagePlayer4[i].sprite = BackImageCard;
                            CardsImagePlayer4[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer4[i].gameObject.SetActive(false);
                        break;
                    case 5:
                        if (PlayersWithLives.Contains(0))
                        {
                            CardsImagePlayer4[i].sprite = BackImageCard;
                            CardsImagePlayer4[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(1))
                        {
                            CardsImagePlayer5[i].sprite = BackImageCard;
                            CardsImagePlayer5[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(2))
                        {
                            CardsImagePlayer6[i].sprite = BackImageCard;
                            CardsImagePlayer6[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(3))
                        {
                            CardsImagePlayer7[i].sprite = BackImageCard;
                            CardsImagePlayer7[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(4))
                        {
                            CardsImagePlayer8[i].sprite = BackImageCard;
                            CardsImagePlayer8[i].gameObject.SetActive(true);
                        }

                        if (PlayersWithLives.Contains(6) && PlayersNamesPlaying.Count >= 7)
                        {
                            CardsImagePlayer2[i].sprite = BackImageCard;
                            CardsImagePlayer2[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer2[i].gameObject.SetActive(false);
                        if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                        {
                            CardsImagePlayer3[i].sprite = BackImageCard;
                            CardsImagePlayer3[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer3[i].gameObject.SetActive(false);
                        break;
                    case 6:
                        if (PlayersWithLives.Contains(0))
                        {
                            CardsImagePlayer3[i].sprite = BackImageCard;
                            CardsImagePlayer3[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(1))
                        {
                            CardsImagePlayer4[i].sprite = BackImageCard;
                            CardsImagePlayer4[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(2))
                        {
                            CardsImagePlayer5[i].sprite = BackImageCard;
                            CardsImagePlayer5[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(3))
                        {
                            CardsImagePlayer6[i].sprite = BackImageCard;
                            CardsImagePlayer6[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(4))
                        {
                            CardsImagePlayer7[i].sprite = BackImageCard;
                            CardsImagePlayer7[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(5))
                        {
                            CardsImagePlayer8[i].sprite = BackImageCard;
                            CardsImagePlayer8[i].gameObject.SetActive(true);
                        }

                        if (PlayersWithLives.Contains(7) && PlayersNamesPlaying.Count >= 8)
                        {
                            CardsImagePlayer2[i].sprite = BackImageCard;
                            CardsImagePlayer2[i].gameObject.SetActive(true);
                        }
                        else
                            CardsImagePlayer2[i].gameObject.SetActive(false);
                        break;
                    case 7:
                        if (PlayersWithLives.Contains(0))
                        {
                            CardsImagePlayer2[i].sprite = BackImageCard;
                            CardsImagePlayer2[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(1))
                        {
                            CardsImagePlayer3[i].sprite = BackImageCard;
                            CardsImagePlayer3[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(2))
                        {
                            CardsImagePlayer4[i].sprite = BackImageCard;
                            CardsImagePlayer4[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(3))
                        {
                            CardsImagePlayer5[i].sprite = BackImageCard;
                            CardsImagePlayer5[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(4))
                        {
                            CardsImagePlayer6[i].sprite = BackImageCard;
                            CardsImagePlayer6[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(5))
                        {
                            CardsImagePlayer7[i].sprite = BackImageCard;
                            CardsImagePlayer7[i].gameObject.SetActive(true);
                        }
                        if (PlayersWithLives.Contains(6))
                        {
                            CardsImagePlayer8[i].sprite = BackImageCard;
                            CardsImagePlayer8[i].gameObject.SetActive(true);
                        }
                        break;
                    default:
                        print("ERROR,NUMERO INVALIDO");
                        break;
                }

            }
        }
        
        _photonView.RPC(nameof(RPC_PlayerFinish), PhotonTargets.All, new object[] {LocalPlayerNum });
    }



    //Termina de recibir las cartas
    [PunRPC]
    private void RPC_PlayerFinish(int numPlayer)
    {
        if (!gameManager.GameStarted) return;
        if (PhotonNetwork.isMasterClient) 
        {
            PlayerFinish[numPlayer] = true;
            bool NotAllFinished = false;
            for (int i = 0; i < PlayerFinish.Length; ++i)
            {
                if (PlayerFinish[i] == false)
                    NotAllFinished = true;
            }

            if (!NotAllFinished)
            {
                PlayerFinish = new bool[NumPlayers];
                SumBets = 0;
                _photonView.RPC(nameof(RPC_PlayerTurn), PhotonTargets.All, new object[] { PlayerTurn, SumBets, -1, -1 });
            }
        }
    }

    [PunRPC]
    private void RPC_PlayerTurn(int iPlayerTurn, int iSumBets, int idCard, int ownerCard)
    {
        if (!gameManager.GameStarted) return;
        PlayerTurn = iPlayerTurn;
        SumBets = iSumBets;

        if (!PlayersWithLives.Contains(PlayerTurn))
        {
            if (PhotonNetwork.isMasterClient)
            {
                ++PlayerTurn;
                if (PlayerTurn >= NumPlayers)
                    PlayerTurn = 0;

                _photonView.RPC(nameof(RPC_PlayerTurn), PhotonTargets.All, new object[] { PlayerTurn, SumBets, -1, -1 });
            }
            else
            {
                return;
            }
        }

        if(idCard != -1) //Si alguien ha puesto una carta
        {
            Card c = Cards[idCard];
            
            c.Owner = ownerCard;
            if (PhotonNetwork.isMasterClient)
            {
                CardsChosenByPlayers.Add(c);
            }

            if(c.Owner != LocalPlayerNum) //Si es otro
            {
                int Num = c.Owner - LocalPlayerNum;
                
                if (Num > 0) //Esta por mi derecha
                {
                    OthersFirstCards[Num-1].sprite = c.FrontImage;

                }
                else
                {
                    OthersFirstCards[Num + 7].sprite = c.FrontImage;
                }
                
            }
        }

        SetTurnVisual();
        if(PlayerTurn == LocalPlayerNum) //Si es mi turno
        {
            if (!PlayersWithLives.Contains(LocalPlayerNum)) //Si no tengo vida, paso turno
            {
                ++PlayerTurn;
                if (PlayerTurn >= NumPlayers)
                    PlayerTurn = 0;

                _photonView.RPC(nameof(RPC_PlayerTurn), PhotonTargets.All, new object[] { PlayerTurn, SumBets, -1, -1 });
            }
            else
            {
                if (LocalBet != -1 && CardChosen == null)  //Si ya he votado y todavia no he escogido una carta
                {
                    if (NumCards > 1)
                    {
                        ChooseCardPanel.SetActive(true);
                        CanChooseCard = true;
                        return;
                    }
                    else
                    {
                        _photonView.RPC(nameof(RPC_ShowAllCards), PhotonTargets.All, new object[] { });
                        return;
                    }

                }
                else if (LocalBet != -1 && CardChosen != null) //Si ya he escogido una carta, termino la ronda
                {
                    _photonView.RPC(nameof(RPC_CheckWinnerRound), PhotonTargets.All, new object[] { });
                    return;
                }

                if (gameState != GameState.FINISHED)
                {
                    if (NumCards == 1) //Si solo tengo una carta
                    {
                        BetsPanelHUD.SetActive(false);
                        LastBetsPanelHUD.SetActive(true);
                    }
                    else
                    {
                        BetsPanelHUD.SetActive(true);
                        LastBetsPanelHUD.SetActive(false);

                        for (int i = 0; i < ButtonBets.Count; ++i)
                        {
                            ButtonBets[i].SetActive(true);
                            ButtonBets[i].GetComponent<Button>().interactable = true;
                        }

                        if (SumBets <= NumCards)
                        {
                            ButtonBets[NumCards - SumBets].GetComponent<Button>().interactable = false;
                        }

                        if (NumCards == 2)
                        {
                            ButtonBets[3].GetComponent<Button>().interactable = false;
                            ButtonBets[4].GetComponent<Button>().interactable = false;
                            ButtonBets[5].GetComponent<Button>().interactable = false;
                        }
                        else if (NumCards == 3)
                        {
                            ButtonBets[4].GetComponent<Button>().interactable = false;
                            ButtonBets[5].GetComponent<Button>().interactable = false;
                        }
                        else if (NumCards == 4)
                        {
                            ButtonBets[5].GetComponent<Button>().interactable = false;
                        }

                    }
                }
            }

        }        
    }

    private void SetTurnVisual()
    {
        for(int i = 0; i < TurnImages.Count; ++i)
        {
            TurnImages[i].SetActive(false);
        }

        if (PlayerTurn == LocalPlayerNum)
        {
            TurnImages[0].SetActive(true);
            return;
        }
            

        int Num = PlayerTurn - LocalPlayerNum;

        if (Num > 0) //Esta por mi derecha
        {
            TurnImages[Num].SetActive(true);

        }
        else
        {
            TurnImages[Num + 8].SetActive(true);
        }
    }

    public void SetBet(int value)
    {
        LocalBet = value;
        SumBets += LocalBet;

        BetsPanelHUD.SetActive(false);
        LastBetsPanelHUD.SetActive(false);
        UpdateBetText(value);

        ++PlayerTurn;
        if (PlayerTurn >= NumPlayers)
            PlayerTurn = 0;

        _photonView.RPC(nameof(RPC_PlayerTurn), PhotonTargets.All, new object[] { PlayerTurn, SumBets, -1, -1 });
    }

    public void SetCard(int value)
    {
        if (CanChooseCard)
        {
            CanChooseCard = false;
            Card c = LocalPlayer.Cards[value];
            CardChosen = c;
            LocalPlayer.Cards.Remove(c);
            RestoreCardsLocalPlayerVisual();
            CenterCard.sprite = c.FrontImage;
            CenterCard.gameObject.SetActive(true);
            ChooseCardPanel.SetActive(false);
            
            ++PlayerTurn;
            if (PlayerTurn >= NumPlayers)
                PlayerTurn = 0;
            _photonView.RPC(nameof(RPC_PlayerTurn), PhotonTargets.All, new object[] { PlayerTurn, SumBets, c.ID, LocalPlayerNum });
        }
    }

    private void RestoreCardsLocalPlayerVisual()
    {
        if (LocalPlayer.Cards.Count == 0)
        {
            CardsImagePlayer1[0].gameObject.SetActive(false);
            CardsImagePlayer1[1].gameObject.SetActive(false);
            CardsImagePlayer1[2].gameObject.SetActive(false);
            CardsImagePlayer1[3].gameObject.SetActive(false);
            CardsImagePlayer1[4].gameObject.SetActive(false);
        }else if (LocalPlayer.Cards.Count == 1)
        {
            CardsImagePlayer1[0].sprite = LocalPlayer.Cards[0].FrontImage;
            CardsImagePlayer1[0].gameObject.SetActive(true);
            CardsImagePlayer1[1].gameObject.SetActive(false);
            CardsImagePlayer1[2].gameObject.SetActive(false);
            CardsImagePlayer1[3].gameObject.SetActive(false);
            CardsImagePlayer1[4].gameObject.SetActive(false);
        }
        else if (LocalPlayer.Cards.Count == 2)
        {
            CardsImagePlayer1[0].sprite = LocalPlayer.Cards[0].FrontImage;
            CardsImagePlayer1[0].gameObject.SetActive(true);
            CardsImagePlayer1[1].sprite = LocalPlayer.Cards[1].FrontImage;
            CardsImagePlayer1[1].gameObject.SetActive(true);
            CardsImagePlayer1[2].gameObject.SetActive(false);
            CardsImagePlayer1[3].gameObject.SetActive(false);
            CardsImagePlayer1[4].gameObject.SetActive(false);
        }
        else if (LocalPlayer.Cards.Count == 3)
        {
            CardsImagePlayer1[0].sprite = LocalPlayer.Cards[0].FrontImage;
            CardsImagePlayer1[0].gameObject.SetActive(true);
            CardsImagePlayer1[1].sprite = LocalPlayer.Cards[1].FrontImage;
            CardsImagePlayer1[1].gameObject.SetActive(true);
            CardsImagePlayer1[2].sprite = LocalPlayer.Cards[2].FrontImage;
            CardsImagePlayer1[2].gameObject.SetActive(true);
            CardsImagePlayer1[3].gameObject.SetActive(false);
            CardsImagePlayer1[4].gameObject.SetActive(false);
        }
        else if (LocalPlayer.Cards.Count == 4)
        {
            CardsImagePlayer1[0].sprite = LocalPlayer.Cards[0].FrontImage;
            CardsImagePlayer1[0].gameObject.SetActive(true);
            CardsImagePlayer1[1].sprite = LocalPlayer.Cards[1].FrontImage;
            CardsImagePlayer1[1].gameObject.SetActive(true);
            CardsImagePlayer1[2].sprite = LocalPlayer.Cards[2].FrontImage;
            CardsImagePlayer1[2].gameObject.SetActive(true);
            CardsImagePlayer1[3].sprite = LocalPlayer.Cards[3].FrontImage;
            CardsImagePlayer1[3].gameObject.SetActive(true);
            CardsImagePlayer1[4].gameObject.SetActive(false);
        }
        else
        {
            CardsImagePlayer1[0].sprite = LocalPlayer.Cards[0].FrontImage;
            CardsImagePlayer1[0].gameObject.SetActive(true);
            CardsImagePlayer1[1].sprite = LocalPlayer.Cards[1].FrontImage;
            CardsImagePlayer1[1].gameObject.SetActive(true);
            CardsImagePlayer1[2].sprite = LocalPlayer.Cards[2].FrontImage;
            CardsImagePlayer1[2].gameObject.SetActive(true);
            CardsImagePlayer1[3].sprite = LocalPlayer.Cards[3].FrontImage;
            CardsImagePlayer1[3].gameObject.SetActive(true);
            CardsImagePlayer1[4].sprite = LocalPlayer.Cards[4].FrontImage;
            CardsImagePlayer1[4].gameObject.SetActive(true);
        }
    }

    [PunRPC]
    private void RPC_CheckWinnerRound()
    {
        if (!gameManager.GameStarted) return;
        gameState = GameState.WAITING;
        if (PhotonNetwork.isMasterClient)
        {
            Card HigherCard = null;
            for(int i = 0; i < CardsChosenByPlayers.Count; ++i)
            {
                if (HigherCard == null)
                    HigherCard = CardsChosenByPlayers[i];
                else
                {
                    if(CardsChosenByPlayers[i].Num > HigherCard.Num)
                    {
                        HigherCard = CardsChosenByPlayers[i];
                    }
                }
            }

            PlayerFinishWaitSeconds = new bool[NumPlayers];
            _photonView.RPC(nameof(RPC_FinishRound), PhotonTargets.All, new object[] { HigherCard.Owner });
        }
    }

    [PunRPC]
    private void RPC_FinishRound(int Winner)
    {
        if (!gameManager.GameStarted) return;
        WinnerText.text ="And the winner is...\n"+ PlayersNamesPlaying[Winner];
        WinnerText.gameObject.SetActive(true);
        if (Winner == LocalPlayerNum)
        {
            ++CurrentBetNum;
            UpdateRoundsWinText();
        }
        StartCoroutine(FinishARound(Winner, LocalPlayerNum));
    }

    [PunRPC]
    private void RPC_FinishLastRound(int[] Winners)
    {
        if (!gameManager.GameStarted) return;
        List<int> ListWinners = new List<int>();
        for(int i = 0; i < Winners.Length; ++i)
        {
            ListWinners.Add(Winners[i]);
        }
        int Winner = ListWinners[0];

        //Si hay más de un ganador
        if(ListWinners.Count > 1)
        {
            WinnerText.text = "Draw";
            WinnerText.gameObject.SetActive(true);
            for(int i = 0; i < ListWinners.Count; ++i)
            {
                if (ListWinners[i] == LocalPlayerNum)
                {
                    ++CurrentBetNum;
                    UpdateRoundsWinText();
                }
            }
            
        }
        else
        {
            WinnerText.text = "And the winner is...\n" + PlayersNamesPlaying[Winner];
            WinnerText.gameObject.SetActive(true);
            if (Winner == LocalPlayerNum)
            {
                ++CurrentBetNum;
                UpdateRoundsWinText();
            }
        }

        LocalPlayer.Cards.Clear();
        LastBetsPanelHUD.SetActive(false);

        StartCoroutine(FinishLastRound(Winner,LocalPlayerNum));

    }

    private IEnumerator FinishLastRound(int Winner, int iLocalPlayerNum)
    {
        yield return new WaitForSeconds(3);
        PlayerFinishWaitSeconds = new bool[NumPlayers];
        _photonView.RPC(nameof(RPC_FinishWaitingLastRound), PhotonTargets.All, new object[] { Winner, iLocalPlayerNum });

    }

    [PunRPC]
    private void RPC_FinishWaitingLastRound(int Winner, int iLocalPlayerNum)
    {
        if (!gameManager.GameStarted) return;
        if (PhotonNetwork.isMasterClient)
        {
            PlayerFinishWaitSeconds[iLocalPlayerNum] = true;
            bool NotAllFinished = false;
            for (int i = 0; i < PlayerFinishWaitSeconds.Length; ++i)
            {
                if (PlayersWithLives.Contains(i))
                {
                    if (PlayerFinishWaitSeconds[i] == false)
                        NotAllFinished = true;
                }
            }

            if (!NotAllFinished)
            {
                PlayerFinishWaitSeconds = new bool[NumPlayers];
                _photonView.RPC(nameof(RPC_FinishLastRoundTotally), PhotonTargets.All, new object[] { Winner });
            }
        }
    }

    [PunRPC]
    private void RPC_FinishLastRoundTotally(int Winner)
    {
        if (!gameManager.GameStarted) return;
        WinnerText.gameObject.SetActive(false);
        CenterCard.gameObject.SetActive(false);

        --CurrentCards;

        UpdateRivalsCards();

        if (PlayersWithLives.Contains(LocalPlayerNum) && CurrentBetNum != LocalBet)
        {
            --LocalPlayer.Lives;
            UpdateLivesText(LocalPlayer.Lives);
            _photonView.RPC(nameof(RPC_SetLoseLive), PhotonTargets.All, new object[] { LocalPlayer.IDPlayer });

        }
        else
        {
            _photonView.RPC(nameof(RPC_FinishUpdatingLive), PhotonTargets.All, new object[] { LocalPlayerNum });
        }
        
    }


    private IEnumerator FinishARound(int Winner, int iLocalPlayerNum)
    {
        yield return new WaitForSeconds(3);

        _photonView.RPC(nameof(RPC_FinishWaiting), PhotonTargets.All, new object[] { Winner, iLocalPlayerNum });

    }

    [PunRPC]
    private void RPC_FinishWaiting(int Winner, int iLocalPlayerNum)
    {
        if (!gameManager.GameStarted) return;
        if (PhotonNetwork.isMasterClient)
        {
            PlayerFinishWaitSeconds[iLocalPlayerNum] = true;
            bool NotAllFinished = false;
            for (int i = 0; i < PlayerFinishWaitSeconds.Length; ++i)
            {
                if (PlayersWithLives.Contains(i))
                {
                    if (PlayerFinishWaitSeconds[i] == false)
                        NotAllFinished = true;
                }
            }

            if (!NotAllFinished)
            {
                PlayerFinishWaitSeconds = new bool[NumPlayers];
                _photonView.RPC(nameof(RPC_FinishRoundTotally), PhotonTargets.All, new object[] { Winner });
            }
        }
    }

    [PunRPC]
    private void RPC_FinishRoundTotally(int Winner)
    {
        if (!gameManager.GameStarted) return;
        WinnerText.gameObject.SetActive(false);
        CenterCard.gameObject.SetActive(false);

        --CurrentCards;
        UpdateRivalsCards();

        //Si ya no me quedan cartas, comprobamos la apuesta
        if (CurrentCards == 0)
        {
            if (PlayersWithLives.Contains(LocalPlayerNum) && CurrentBetNum != LocalBet)
            {
                --LocalPlayer.Lives;
                UpdateLivesText(LocalPlayer.Lives);
                _photonView.RPC(nameof(RPC_SetLoseLive), PhotonTargets.All, new object[] { LocalPlayer.IDPlayer });

            }
            else
            {
                _photonView.RPC(nameof(RPC_FinishUpdatingLive), PhotonTargets.All, new object[] { LocalPlayerNum });
            }
        }
        else
        {
            gameState = GameState.TURN;
            //Reinicio variables locales
            CanChooseCard = false;
            CardChosen = null;
            if (PhotonNetwork.isMasterClient)
            {
                CardsChosenByPlayers = new List<Card>();
                PlayerTurn = Winner;
                _photonView.RPC(nameof(RPC_PlayerTurn), PhotonTargets.All, new object[] { PlayerTurn, SumBets, -1, -1 });
            }
        }
    }

    [PunRPC]
    private void RPC_FinishUpdatingLive(int IDPlayer)
    {
        if (!gameManager.GameStarted) return;
        if (PhotonNetwork.isMasterClient)
        {
            PlayerFinishUpdateLives[IDPlayer] = true;
            bool NotAllFinished = false;
            for (int i = 0; i < PlayerFinishUpdateLives.Length; ++i)
            {
                if (PlayersWithLives.Contains(i))
                {
                    if (PlayerFinishUpdateLives[i] == false)
                        NotAllFinished = true;
                }
            }

            if (!NotAllFinished)
            {
                PlayerFinishUpdateLives = new bool[NumPlayers];
                _photonView.RPC(nameof(RPC_CheckGameState), PhotonTargets.All, new object[] { });
            }
        }
    }

    [PunRPC]
    private void RPC_CheckGameState()
    {
        if (!gameManager.GameStarted) return;

        if (PhotonNetwork.isMasterClient && PlayersWithLives.Count == 1)
        {
            _photonView.RPC(nameof(RPC_SetWinner), PhotonTargets.All, new object[] { PlayersWithLives[0] });
            return;
        }
        else if (PhotonNetwork.isMasterClient && PlayersWithLives.Count == 0)
        {
            _photonView.RPC(nameof(RPC_SetWinner), PhotonTargets.All, new object[] { -1 });
            return;
        }

        //Reinicio variables locales
        LocalBet = -1;
        CurrentBetNum = 0;
        UpdateRoundsWinText();
        CanChooseCard = false;
        CardChosen = null;

        ++InitialPlayerTurn;
        if (InitialPlayerTurn >= NumPlayers)
            InitialPlayerTurn = 0;

        PlayerTurn = InitialPlayerTurn;

        CardsChosenByPlayers = new List<Card>();
        SumBets = 0;

        if (PhotonNetwork.isMasterClient)
        {
            BetsClientHUD.SetActive(false);
            ContinueHUD.SetActive(true);
        }
        else
        {
            BetsClientHUD.SetActive(true);
            ContinueHUD.SetActive(false);
        }
        gameState = GameState.CONTINUE;

    }

    private void UpdateRivalsCards()
    {
        if(CurrentCards == 0)
        {
            for (int i = 0; i < CardsImagePlayer1.Count; ++i)
            {
                CardsImagePlayer2[i].gameObject.SetActive(false);
                CardsImagePlayer3[i].gameObject.SetActive(false);
                CardsImagePlayer4[i].gameObject.SetActive(false);
                CardsImagePlayer5[i].gameObject.SetActive(false);
                CardsImagePlayer6[i].gameObject.SetActive(false);
                CardsImagePlayer7[i].gameObject.SetActive(false);
                CardsImagePlayer8[i].gameObject.SetActive(false);
            }
        }
        else
        {
            //Actualizo la primera imagen con la carta boca abajo
            CardsImagePlayer2[0].sprite = BackImageCard;
            CardsImagePlayer3[0].sprite = BackImageCard;
            CardsImagePlayer4[0].sprite = BackImageCard;
            CardsImagePlayer5[0].sprite = BackImageCard;
            CardsImagePlayer6[0].sprite = BackImageCard;
            CardsImagePlayer7[0].sprite = BackImageCard;
            CardsImagePlayer8[0].sprite = BackImageCard;

            for (int i = CurrentCards; i < CardsImagePlayer1.Count; ++i)
            {
                CardsImagePlayer2[i].gameObject.SetActive(false);
                CardsImagePlayer3[i].gameObject.SetActive(false);
                CardsImagePlayer4[i].gameObject.SetActive(false);
                CardsImagePlayer5[i].gameObject.SetActive(false);
                CardsImagePlayer6[i].gameObject.SetActive(false);
                CardsImagePlayer7[i].gameObject.SetActive(false);
                CardsImagePlayer8[i].gameObject.SetActive(false);
            }
        }
    }

    [PunRPC]
    private void RPC_ShowAllCards() //Cuando solo queda una carta, mostrar todas
    {
        if (!gameManager.GameStarted) return;
        gameState = GameState.WAITING;

        if(PlayersWithLives.Contains(LocalPlayerNum))
            CardsImagePlayer1[0].sprite = LocalPlayer.Cards[0].FrontImage;

        if (PhotonNetwork.isMasterClient)
        {
            Card HigherCard = null;

            List<int> ListOfWinners = new List<int>();

            for (int i = 0; i < FirstCardsOfAllPlayers.Count; ++i)
            {
                if (ListOfWinners.Count == 0)
                {
                    HigherCard = FirstCardsOfAllPlayers[i];
                    ListOfWinners.Add(HigherCard.Owner);
                } 
                else
                {
                    if (FirstCardsOfAllPlayers[i].Num == HigherCard.Num)
                    {
                        ListOfWinners.Add(FirstCardsOfAllPlayers[i].Owner);
                    }
                    else if(FirstCardsOfAllPlayers[i].Num > HigherCard.Num)
                    {
                        HigherCard = FirstCardsOfAllPlayers[i];
                        ListOfWinners.Clear();
                        ListOfWinners.Add(HigherCard.Owner);
                    }
                    
                }
            }

            int[] Winners = new int[ListOfWinners.Count];

            for(int i = 0; i < ListOfWinners.Count; ++i)
            {
                Winners[i] = ListOfWinners[i];
            }
            _photonView.RPC(nameof(RPC_FinishLastRound), PhotonTargets.All, new object[] { Winners });
        }

    }

    [PunRPC]
    private void RPC_SetLoseLive(int IDPlayer)
    {
        if (!gameManager.GameStarted) return;
        if (PhotonNetwork.isMasterClient)
        {
            --Players[IDPlayer].Lives;
            if(Players[IDPlayer].Lives <= 0)
            {
                _photonView.RPC(nameof(RPC_SetPlayerLose), PhotonTargets.All, new object[] { IDPlayer });
            }
            else
            {
                _photonView.RPC(nameof(RPC_FinishUpdatingLive), PhotonTargets.All, new object[] { IDPlayer });
            }
        }
    }

    [PunRPC]
    private void RPC_SetPlayerLose(int IDPlayer)
    {
        if (!gameManager.GameStarted) return;
        PlayersWithLives.Remove(IDPlayer);

        if(IDPlayer == LocalPlayerNum)
        {
            PlayerNamesInGame[0].color = Color.red;
            UpdateBetText(-1);
            CurrentBetNum = 0;
            UpdateRoundsWinText();
        }
        else
        {
            int Num = IDPlayer - LocalPlayerNum;

            if (Num > 0) //Esta por mi derecha
            {
                PlayerNamesInGame[Num].color = Color.red;
            }
            else  //Esta por mi izquierda
            {
                PlayerNamesInGame[Num + 8].color = Color.red;
            }
        }
        
        if(LocalPlayerNum == IDPlayer)
        {
            _photonView.RPC(nameof(RPC_FinishUpdatingLive), PhotonTargets.All, new object[] { LocalPlayerNum });
        }
    }

    [PunRPC]
    private void RPC_SetWinner(int IDPlayer)
    {
        if (!gameManager.GameStarted) return;
        gameState = GameState.FINISHED;
        RestoreBetsCanvas();
        BetsHUD.SetActive(true);

        //Reinicio variables locales
        LocalBet = -1;
        CurrentBetNum = 0;
        UpdateRoundsWinText();
        CanChooseCard = false;
        CardChosen = null;

        InitialPlayerTurn = 0;

        PlayerTurn = InitialPlayerTurn;

        CardsChosenByPlayers = new List<Card>();
        SumBets = 0;
        

        if(IDPlayer == -1)
            WinText.text = "DRAW!!!";
        else
            WinText.text = "CONGRATULATIONS, \n" + PlayersNamesPlaying[IDPlayer] + "!!!";

        WinText.gameObject.SetActive(true);
        WinPanel.SetActive(true);
        
        //Abrid hud de victoria
        if (PhotonNetwork.isMasterClient)
        {
            HostWinPanel.SetActive(true);
            BetsClientHUD.SetActive(false);
        }
        else
        {
            HostWinPanel.SetActive(false);
            BetsClientHUD.SetActive(true);
        }
    }

    public void ContinueGame()
    {
        _photonView.RPC(nameof(RPC_ContinueGame), PhotonTargets.All, new object[] { });
    }

    [PunRPC]
    private void RPC_ContinueGame()
    {
        if (!gameManager.GameStarted) return;
        gameState = GameState.TURN;
        if (NumCards == 1)
            NumCards = 5;
        else
            --NumCards;

        CurrentCards = NumCards;
        if (PhotonNetwork.isMasterClient)
            DistributeCards();
    }


    public void RestartGame()
    {
        PlayersNamesPlaying = new List<string>();
        for (int i = 0; i < PlayersNamesPlaying.Count; ++i)
        {
            PlayersNamesPlaying.Add(gameManager.PlayersNamesPlaying[i]);
        }

        NumPlayers = PlayersNamesPlaying.Count;

        if(NumPlayers > 1)
            _photonView.RPC(nameof(RPC_RestartBetsCanvas), PhotonTargets.All, new object[] { });
    }

    [PunRPC]
    private void RPC_RestartBetsCanvas()
    {
        if (!gameManager.GameStarted) return;
        gameState = GameState.NOSTARTED;
        RestoreBetsCanvas();
        PlayersNamesPlaying = new List<string>();
        for (int i = 0; i < PlayersNamesPlaying.Count; ++i)
        {
            PlayersNamesPlaying.Add(gameManager.PlayersNamesPlaying[i]);
        }

        NumPlayers = PlayersNamesPlaying.Count;
        CurrentCards = MaxNumCardsIni;
        NumCards = MaxNumCardsIni;
        SetPlayerNamesInGame();
        OpenBetsHUD();
    }

    private void RestoreBetsCanvas()
    {
        for (int i = 0; i < CardsImagePlayer1.Count; ++i)
        {
            CardsImagePlayer1[i].gameObject.SetActive(false);
            CardsImagePlayer2[i].gameObject.SetActive(false);
            CardsImagePlayer3[i].gameObject.SetActive(false);
            CardsImagePlayer4[i].gameObject.SetActive(false);
            CardsImagePlayer5[i].gameObject.SetActive(false);
            CardsImagePlayer6[i].gameObject.SetActive(false);
            CardsImagePlayer7[i].gameObject.SetActive(false);
            CardsImagePlayer8[i].gameObject.SetActive(false);
        }

        BetsHostHUD.SetActive(false);
        BetsClientHUD.SetActive(false);

    CenterCard.gameObject.SetActive(false);

        BetsPanelHUD.SetActive(false);
        LastBetsPanelHUD.SetActive(false);

        for (int i = 0; i < TurnImages.Count; ++i)
        {
            TurnImages[i].SetActive(false);
        }

        ChooseCardPanel.SetActive(false);

        ContinueHUD.SetActive(false);

        LivesText.gameObject.SetActive(false);
        BetText.gameObject.SetActive(false);
        RoundsWinText.gameObject.SetActive(false);
        WinnerText.gameObject.SetActive(false);

        WinPanel.SetActive(false);

        BetsHUD.SetActive(false);
    }

    public void BackToMenu()
    {
        PhotonNetwork.room.IsVisible = true;
        PhotonNetwork.room.IsOpen = true;
        _photonView.RPC(nameof(RPC_BackToMenu), PhotonTargets.All, new object[] { });
    }

    [PunRPC]
    private void RPC_BackToMenu()
    {
        if (!gameManager.GameStarted) return;
        gameState = GameState.NOSTARTED;
        for (int i = 0; i < PlayerNamesInGame.Count; ++i)
        {
            PlayerNamesInGame[i].gameObject.SetActive(false);
        }
        CurrentCards = MaxNumCardsIni;
        NumCards = MaxNumCardsIni;

        RestoreBetsCanvas();

        gameManager.BackToMenu();
    }

    public void SetPlayerLoseDisconnect(string playerName)
    {
        int idPlayer = 0;
        for (idPlayer = 0; idPlayer < PlayersNamesPlaying.Count; ++idPlayer)
        {
            if (PlayersNamesPlaying[idPlayer].CompareTo(playerName) == 0)
                break;
        }

        if (idPlayer >= PlayersNamesPlaying.Count)
        {
            print("ERROR, NO ENCONTRADO EL NOMBRE");
            return;
        }

        _photonView.RPC(nameof(RPC_SetPlayerLoseDisconnect), PhotonTargets.All, new object[] { idPlayer});
    }

    [PunRPC]
    private void RPC_SetPlayerLoseDisconnect(int idPlayer)
    {
        if (!gameManager.GameStarted) return;

        PlayersWithLives.Remove(idPlayer);

        int Num = idPlayer - LocalPlayerNum;

        if (Num > 0) //Esta por mi derecha
        {
            PlayerNamesInGame[Num].color = Color.red;
        }
        else  //Esta por mi izquierda
        {
            PlayerNamesInGame[Num + 8].color = Color.red;
        }

        
        if (FirstCardsOfAllPlayers.Count > idPlayer)
        {
            FirstCardsOfAllPlayers.Remove(FirstCardsOfAllPlayers[idPlayer]);
        }

        for (int i = 0; i < CardsChosenByPlayers.Count; ++i)
        {
            Card c = CardsChosenByPlayers[i];
            if (c.Owner == idPlayer)
            {
                CardsChosenByPlayers.Remove(c);
                break;
            }
        }

        if (PlayersWithLives.Count <= 1)
        {
            _photonView.RPC(nameof(RPC_CheckGameState), PhotonTargets.All, new object[] { });
            return;
        }


        if (PhotonNetwork.isMasterClient)
        {
            switch (gameState)
            {
                case GameState.PREINITIAL:
                    OpenBetsHUD();
                    break;
                case GameState.TURN:
                    if (PlayerTurn == idPlayer)
                    {
                        ++PlayerTurn;
                        if (PlayerTurn >= NumPlayers)
                            PlayerTurn = 0;
                        _photonView.RPC(nameof(RPC_PlayerTurn), PhotonTargets.All, new object[] { PlayerTurn, SumBets, -1, -1 });
                    }
                    break;
                case GameState.CONTINUE:
                    BetsClientHUD.SetActive(false);
                    ContinueHUD.SetActive(true);
                    break;
                case GameState.FINISHED:
                    BetsHUD.SetActive(true);
                    break;
                default:
                    print("ERROR, GAMESTATE INVALIDO");
                    break;
            }
        }
    }

    private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }

}
