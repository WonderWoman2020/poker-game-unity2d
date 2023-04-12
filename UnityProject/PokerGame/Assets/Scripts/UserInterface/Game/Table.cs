using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;
using pGrServer;
using System.Threading;

using TMPro;
using System;
using System.Net.Sockets;

using ClientSideCardsHelper;
using static System.Net.Mime.MediaTypeNames;

public class Table : MonoBehaviour
{
    [SerializeField] private Button checkButton;
    [SerializeField] private Button allInButton;
    [SerializeField] private Button passButton;
    [SerializeField] private Button bidButton;


    [SerializeField] private TMP_Text InfoMainPlayerName;
    [SerializeField] private TMP_Text InfoMainPlayerChips;
    [SerializeField] private TMP_Text InfoMainPlayerBid;

    [SerializeField]
    private CanvasRenderer menuCanvas;

    private bool readyToSendMove = false;
    private GameTableState gameTableState;
    private IDictionary<string, PlayerState> playersStates;

    public GameObject PopupWindow;

    private GameObject[] Players
    {get; set;}
    private Component[] Components
    { get; set; }
    private string betFieldText;

    bool displayPlayerTurnPopup = false;


    // Start is called before the first frame update
    void Start()
    {
        ShowMenu(false); //zakrycie MENU na start
        ShowMenu(true);
        if (MyGameManager.Instance.MainPlayer == null)
            return;

        this.InfoMainPlayerName.text = MyGameManager.Instance.MainPlayer.Nick;
        this.InfoMainPlayerChips.text = Convert.ToString(MyGameManager.Instance.MainPlayer.TokensCount) + " $";
        this.InfoMainPlayerBid.text = "Bet\n" + Convert.ToString(0) + " $";

        this.Players = GameObject.FindGameObjectsWithTag("Player");
        //this.Components = Players[0].GetComponents(typeof(Component));

        //HideAllPlayers();
        //ShowPlayerOnTable(0, "Player1");
        //ChangePlayerBet(100, 0);
        //ChangePlayerMoney(200, 0);
        //HidePlayerOnTable(2);

//

        this.gameTableState = new GameTableState();
        this.playersStates = new Dictionary<string, PlayerState>();

        //
        new System.Threading.Thread(CommunicateWithServer).Start();
    }

    public void CommunicateWithServer()
    {
        NetworkStream gameStream = MyGameManager.Instance.gameServerConnection.stream;
        bool running = true;

        while (running)
        {
            int playerCounter = 0;
            if (gameStream.DataAvailable)
            {
                UnityEngine.Debug.Log("sa dane na strumieniu");
                string gameRequest = NetworkHelper.ReadNetworkStream(gameStream);
                gameStream.Flush();
                string[] splittedRequests = gameRequest.Split(new string(":G:"));

                foreach (string singleRequest in splittedRequests)
                {
                    Debug.Log(singleRequest);
                    string[] splitted = singleRequest.Split(new string("|"));
                    if (splitted[0] == "Which player turn")
                    {
                        if(!this.displayPlayerTurnPopup)
                        {
                            CommunicatePlayersTurn(splitted[1]);
                        }
                    }
                    else if (splitted[0] == "Move request")
                    {
                        MoveRequestResponse(splitted);
                    }
                    else if (splitted[0] == "Table state")
                    {
                        this.gameTableState.UnpackGameState(splitted);
                        Debug.Log(this.gameTableState);
                    }
                    else if (splitted[0] == "Player state")
                    {
                        PlayerState playerState = new PlayerState();
                        playerState.UnpackGameState(splitted);
                        Debug.Log(playerState);
                        this.playersStates[playerState.Nick] = playerState;
                        Debug.Log("Player state count: " + this.playersStates.Count);
                        /*this.ShowPlayerOnTable(playerCounter, playerState.Nick);
                        this.ChangePlayerBet(playerState.CurrentBet, playerCounter);
                        this.ChangePlayerMoney(playerState.TokensCount, playerCounter);
                        playerCounter++;*/
                    }
                }
            }
        }
    }

    void CommunicatePlayersTurn(string currentPlayer)
    {
        if (currentPlayer == MyGameManager.Instance.MainPlayer.Nick)
        {
            this.displayPlayerTurnPopup = true;
        }
    }

    void MoveRequestResponse(string[] splitted)
    {
        Debug.Log(splitted[0]);
        Debug.Log(splitted[1]);
        this.readyToSendMove = true;
        //Czekamy teraz na klikniecie ktoregos z przyciskow. wyslanie kolejnego requesta do serwera jest wykonywane w metodach przyciskow
    }

    void HideAllPlayers()
    { 
        foreach (GameObject player in Players)
        {
            player.transform.localScale = Vector3.zero;
        }

    }

    public void ShowPlayerOnTable(int seatNumber, string playerNick)
    {
        GameObject nick = Players[seatNumber].transform.Find("Informations/Name/NickText").gameObject;
        if (nick != null)
        {
            nick.GetComponent<TMP_Text>().text = playerNick;
            nick.GetComponent<TMP_Text>().fontSize = 21.75f;    //nie dziala, bo autosize w unity
        }
        Players[seatNumber].transform.localScale = Vector3.one;
    }

    public void HidePlayerOnTable(int seatNumber)
    {
        Players[seatNumber].transform.localScale = Vector3.zero;
    }

    public void ShowMenu(bool isMenuToShow)
    {
        if(isMenuToShow == true)
            menuCanvas.transform.localScale = Vector3.one;
        else
            menuCanvas.transform.localScale = Vector3.zero;
    }
    
    public void ChangePlayerBet(int amount, int seatNumber)
    {
        GameObject bet = Players[seatNumber].transform.Find("Informations/Bet/BetText").gameObject;
        if (bet != null)
        {
            bet.GetComponent<TMP_Text>().text = "Bet\n"+amount.ToString()+" $";
        }
    }
    public void ChangePlayerMoney(int amount, int seatNumber) 
    {
        GameObject money = Players[seatNumber].transform.Find("Informations/Name/Money/MoneyText").gameObject;
        if (money != null)
        {
            money.GetComponent<TMP_Text>().text = amount.ToString() +" $";
        }
    }
    // Update is called once per frame
    void Update()
    {
        int i = 0;
        foreach(KeyValuePair<string, PlayerState> state in this.playersStates)
        {
            PlayerState playerState = state.Value;

            if(playerState.Nick == MyGameManager.Instance.MainPlayer.Nick)
            {
                this.InfoMainPlayerName.text = playerState.Nick;
                this.InfoMainPlayerChips.text = Convert.ToString(playerState.TokensCount) + " $";
                this.InfoMainPlayerBid.text = "Bet\n" + Convert.ToString(playerState.CurrentBet) + " $";
                continue;
            }

            this.ShowPlayerOnTable(i, playerState.Nick);
            this.ChangePlayerBet(playerState.CurrentBet, i);
            this.ChangePlayerMoney(playerState.TokensCount, i);
            i++;
        }
        if (this.displayPlayerTurnPopup && PopupWindow)
        {
            var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
            popup.GetComponent<TextMeshProUGUI>().text = "It's your turn, make a move";
            this.displayPlayerTurnPopup = false;
        }
    }
    public void ReadInputBet(string inputBet)
    {
        if (inputBet.Length == 0)
        {
            this.betFieldText = null;
            return;
        }
        this.betFieldText = inputBet;
        Debug.Log(this.betFieldText);
    }
    public void OnCheckButton()
    {
        Debug.Log("Check");
        //MyGameManager.Instance.MainPlayer.Check();

        if(this.readyToSendMove)
        {
            NetworkStream gameStream = MyGameManager.Instance.gameServerConnection.stream;
            NetworkHelper.WriteNetworkStream(gameStream, "1 ");
            this.readyToSendMove = false;
        }
    }
    public void OnAllInButton()
    {
        Debug.Log("All in");
        //MyGameManager.Instance.MainPlayer.AllIn();

        if (this.readyToSendMove)
        {
            NetworkStream gameStream = MyGameManager.Instance.gameServerConnection.stream;
            NetworkHelper.WriteNetworkStream(gameStream, "3 ");
            this.readyToSendMove = false;
        }
    }
    public void OnPassButton()
    {
        Debug.Log("Pass");
        //MyGameManager.Instance.MainPlayer.Fold();

        if (this.readyToSendMove)
        {
            NetworkStream gameStream = MyGameManager.Instance.gameServerConnection.stream;
            NetworkHelper.WriteNetworkStream(gameStream, "0 ");
            this.readyToSendMove = false;
        }
    }
    public void OnBidButton()
    {
        Debug.Log("Bid");
        //MyGameManager.Instance.MainPlayer.Raise(Convert.ToInt32(this.betFieldText));

        if (this.readyToSendMove)
        {
            NetworkStream gameStream = MyGameManager.Instance.gameServerConnection.stream;
            int input = Convert.ToInt32(Console.ReadLine());
            NetworkHelper.WriteNetworkStream(gameStream, "2 " + this.betFieldText.ToString());
            this.readyToSendMove = false;
        }
    }

    public void onStartGameButton()
    {
        string token = MyGameManager.Instance.clientToken;
        byte[] toSend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "6" + ' ');
        MyGameManager.Instance.mainServerConnection.stream.Write(toSend, 0, toSend.Length);
        MyGameManager.Instance.mainServerConnection.stream.Flush();
        Thread.Sleep(1000);

    }
}
