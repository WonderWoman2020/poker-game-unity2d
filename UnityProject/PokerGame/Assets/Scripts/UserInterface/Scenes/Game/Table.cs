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


using static System.Net.Mime.MediaTypeNames;

// G��wny ekran gry - widok stolika, kart i graczy
public class Table : MonoBehaviour
{
    // Przyciski ruch�w gracza (menu w lewym dolnym rogu ekranu)
    [SerializeField] private Button checkButton;
    [SerializeField] private Button allInButton;
    [SerializeField] private Button passButton;
    [SerializeField] private Button bidButton;

    // Dane pobrane z pola input 'Bid' (menu w lewym dolnym rogu ekranu)
    private string betFieldText;

    // Wy�wietlanie informacji o g��wnym graczu
    /*
     * - Nick
     * - Ile ma �eton�w
     * - Ile postawi� �eton�w w tym rozdaniu
     * - Jego karty (GameObject'y)
     */
    [SerializeField] private TMP_Text InfoMainPlayerName;
    [SerializeField] private TMP_Text InfoMainPlayerChips;
    [SerializeField] private TMP_Text InfoMainPlayerBid;
    [SerializeField] private GameObject[] MainPlayerCards;


    // Obiekt menu ruch�w gracza, u�ywamy go do chowania lub pokazywania tego menu
    [SerializeField]
    private CanvasRenderer menuCanvas;

    // Lista sprite'�w kart, z kt�rej wybieramy odpowiedni
    // sprite do przypisania do GameObject'u karty gracza
    [SerializeField]
    private CardsSprites collection;

    // Prze��cznik ustawiany na 'true', kiedy serwer przy�le do klienta zapytanie o wykonanie ruchu
    private bool readyToSendMove = false;

    // Stan stolika, odebrany od serwera (wysy�a po ka�dym ruchu kogokolwiek)
    private GameTableState gameTableState;
    // Stany wszystkich graczy, odebrane od serwera (wysy�a po ka�dym ruchu kogokolwiek).
    // S�ownik: klucz - Nick danego gracza (odczytywany z PlayerState), warto�� - PlayerState
    private IDictionary<string, PlayerState> playersStates;

    // informacje o b��dach, komunikaty dla gracza
    // m.in. komunikat o tym czyj ruch jest teraz
    public GameObject PopupWindow;

    // GameObject'y na ekranie (update'ujemy je, �eby wy�wietla�y info z gry na ekranie)
    // GameObject'y kart stolika (TYLKO te 5 kart, kt�re le�� na stoliku!)
    private GameObject[] CardsObject;
    // GameObject'y graczy (w nich s� te� GameObject'y ich kart)
    private GameObject[] Players
    {get; set;}

    // Prze��czniki, kt�re wykorzystujemy w Update'owaniu sceny, �eby wiedzie�,
    // kiedy mamy wy�wietli� dany tekst informacyjny
    bool displayPlayerTurnPopup = false;
    bool displayWinnerPopup = false;

    // Nick zwyci�zcy gry, od serwera (wysy�a pod koniec gry)
    string winnerNick = null;


    // Start is called before the first frame update
    void Start()
    {
        ShowMenu(false); //zakrycie MENU na start
        ShowMenu(true);
        if (MyGameManager.Instance.MainPlayer == null)
            return;

        // Pierwszy update wy�wietlanego info g��wnego gracza
        this.InfoMainPlayerName.text = MyGameManager.Instance.MainPlayer.Nick;
        this.InfoMainPlayerChips.text = Convert.ToString(MyGameManager.Instance.MainPlayer.TokensCount) + " $";
        this.InfoMainPlayerBid.text = "Bet\n" + Convert.ToString(0) + " $";

        // Pobranie GameObject'�w przygotowanych na graczy i na karty stolika ze sceny
        // (Graczy mamy na sztywno utworzonych na scenie, a nie spawn'owanych po doj�ciu kogo� do stolika,
        // wi�c tutaj pobieramy wszystkie te puste szablony przygotowane na wy�wietlanie informacji o danym graczu)
        this.Players = GameObject.FindGameObjectsWithTag("Player");
        this.CardsObject = GameObject.FindGameObjectsWithTag("Card");

        // Testowanie pokazywania i ukrywania odpowiednich kart u graczy
        // oraz chowania i pokazywania tak�e graczy
        //
        //HideAllPlayers();
        //ShowPlayerOnTable(0, "Player1");
        //ChangePlayerBet(100, 0);
        //ChangePlayerMoney(200, 0);
        //HidePlayerOnTable(2);

        //Card card1 = new Card(CardSign.Heart, CardValue.Jack, 9);
        //Card card2 = new Card(CardSign.Diamond, CardValue.Ace, 38);
        //Card card3 = new Card(CardSign.Club, CardValue.Eight, 45);
        //Card card4 = new Card(CardSign.Heart, CardValue.Four, 2);
        //Card card5 = new Card(CardSign.Heart, CardValue.Five, 3);


        //ShowCardOnDeck(card1, 0);
        //ShowCardOnDeck(card2, 1);
        //ShowCardOnDeck(card3, 2);
        //ShowCardOnDeck(card4, 3);
        //ShowCardOnDeck(card5, 4);
        //List<Card> c = new List<Card>();
        //c.Add(card1);
        //c.Add(card2);

        //CardsCollection cc = new CardsCollection(c);
        //ShowPlayerCards(0, cc);
        //ShowMainPlayerCards(cc);
        //HidePlayerCards(0);
        //HideMainPlayerCards();
        //HideCardsOnDeck();
        //

        // Inicjalizacja stanu stolika i s�ownika stan�w graczy w grze
        this.gameTableState = new GameTableState();
        this.playersStates = new Dictionary<string, PlayerState>();

        // W��czenie osobnego w�tku do komunikacji z serwerem na porcie od komunikat�w z gry
        // W tym w�tku Unity nie pozwala zmienia� nic na ekranie - update'owa� wygl�d
        // ekranu mo�na tylko w w�tku g��wnym, w kt�rym dzia�a np. funkcja Start i Update
        new System.Threading.Thread(CommunicateWithServer).Start();
    }

    public void CommunicateWithServer()
    {
        // Pobranie strumienia do komunikacji z serwerem na porcie od zdarze� gry
        // (w��czamy te ��cze obecnie w ekranie Login po udanym logowaniu) - TODO mo�e przenie�� to dopiero do tego ekranu?
        NetworkStream gameStream = MyGameManager.Instance.gameServerConnection.stream;
        bool running = true;

        /* P�tla do odbierania komunikat�w od serwera
         * Tutaj odbierane s�:
         * - zapytania od serwera o wykonanie ruchu przez gracza
         * - komunikaty od serwera z informacjami o stanie stolika i wszystkich graczy (w tym ich kartach)
         * (TODO zmieni�, �eby odbierane by�y tylko karty naszego gracza, a reszty tylko na koniec gry - ale to akurat na serwerze)
         * - komunikaty od serwera czyj ruch i pod koniec gry, kto zwyci�y�
         */
        while (running)
        {
            if (gameStream.DataAvailable)
            {
                UnityEngine.Debug.Log("sa dane na strumieniu");
                string gameRequest = NetworkHelper.ReadNetworkStream(gameStream);
                gameStream.Flush();

                /* Wiadomo�ci o zdarzeniach z gry od serwera s� rozdzielane znacznikiem :G: (od Game)
                 * i maj� posta� Typ_wiadomo�ci|Tre��
                 * Podczas przesy�ania wiadomo�ci o stanie gry wysy�anych jest na raz kilka wiadomo�ci:
                 * (wszystkie zaczynaj� si� od znacznika :G:, wi�c w sumie to osobne wiadomo�ci, ale to tak dla jasno�ci jak to dzia�a)
                 * - wiadomo�� typu "Round" (kt�ra to runda gry) TODO doda� jej odbieranie tu i wy�wietlanie gdzie� na g�rze ekranu numeru rundy
                 * - wiadomo�� typu "Table state"
                 * - kilka wiadomo�ci typu "Player state" (o stanie ka�dego z graczy)
                 * - wiadomo�� typu "Which player turn"
                 */
                // TODO doda� jeszcze odbieranie wiadomo�ci typu 'Info' i wy�wietlanie takich komunikat�w na ekranie
                string[] splittedRequests = gameRequest.Split(new string(":G:"));

                foreach (string singleRequest in splittedRequests)
                {
                    Debug.Log(singleRequest);
                    string[] splitted = singleRequest.Split(new string("|"));
                    if (splitted[0] == "Which player turn") // komunikat czyj teraz ruch
                    {
                        if(!this.displayPlayerTurnPopup)
                        {
                            CommunicatePlayersTurn(splitted[1]);
                        }
                    }
                    else if (splitted[0] == "Move request") // zapytanie o wykonanie ruchu
                    {
                        MoveRequestResponse(splitted);
                    }
                    else if (splitted[0] == "Table state") // komunikat stanu stolika
                    {
                        this.gameTableState.UnpackGameState(splitted); // Tre�� wiadomo�ci o stanie stolika ma jak�� struktur�, kt�r� odpakowuje ta metoda
                        Debug.Log(this.gameTableState);
                    }
                    else if (splitted[0] == "Player state") // komunikat stanu kt�rego� z graczy
                    {
                        PlayerState playerState = new PlayerState();
                        playerState.UnpackGameState(splitted); // Tre�� wiadomo�ci o stanie gracza ma jak�� struktur�, kt�r� odpakowuje ta metoda
                        Debug.Log(playerState);
                        this.playersStates[playerState.Nick] = playerState;
                        Debug.Log("Player state count: " + this.playersStates.Count);
                    }
                    else if(splitted[0] == "Winner") // komunikat z Nick'iem zwyci�zcy
                    {
                        this.winnerNick = splitted[1];
                        this.displayWinnerPopup = true;
                    }
                }
            }
        }
    }

    // Przestawiene prze��cznika, �eby metoda Update w g��wnym w�tku wiedzia�a, �e ma pokaza� Popup
    // (inne w�tki ni� g��wny nie mog� zmienia� wygl�du sceny w Unity)
    void CommunicatePlayersTurn(string currentPlayer)
    {
        if (currentPlayer == MyGameManager.Instance.MainPlayer.Nick)
        {
            this.displayPlayerTurnPopup = true;
        }
    }

    // Kiedy przyjdzie zapytanie od serwera o ruch gracza, ustawiamy prze��cznik,
    // �e gracz mo�e teraz wys�a� jeden ruch (prze��cznik si� przestawia ponownie na 'false'
    // w trakcie wysy�ania ruchu przez gracza (po klikni�ciu przez niego kt�rego� z przycisk�w od ruch�w)
    void MoveRequestResponse(string[] splitted)
    {
        Debug.Log(splitted[0]);
        Debug.Log(splitted[1]);
        this.readyToSendMove = true;
        //Czekamy teraz na klikniecie ktoregos z przyciskow. wyslanie kolejnego requesta do serwera jest wykonywane w metodach przyciskow
    }

    // Metody od pokazywania i chowania kart, graczy i menu ruch�w
    // TODO przenie�� to do jakiej� osobnej klasy?
    void ShowCard(Card card, GameObject cardObject)
    {
        cardObject.GetComponent<UnityEngine.UI.Image>().sprite = collection.cardsSpriteSerialization[card.Id];
    }

    void ShowPlayerCards(int seatNumber, CardsCollection cards)
    {
        ShowCard(cards.Cards[0], Players[seatNumber].transform.Find("Cards/Card 1").gameObject);
        ShowCard(cards.Cards[1], Players[seatNumber].transform.Find("Cards/Card 2").gameObject);
    }
    void ShowMainPlayerCards(CardsCollection cards)
    {
        ShowCard(cards.Cards[0], MainPlayerCards[0]);
        ShowCard(cards.Cards[1], MainPlayerCards[1]);
    }

    void ShowCardOnDeck(Card card, int cardIdToShow)
    {
        ShowCard(card, CardsObject[cardIdToShow]);
    }
    void HideCard(GameObject cardObject)
    {
        cardObject.GetComponent<UnityEngine.UI.Image>().sprite = collection.cardsSpriteSerialization[52];
    }
    void HidePlayerCards(int seatNumber)
    {
        HideCard(Players[seatNumber].transform.Find("Cards/Card 1").gameObject);
        HideCard(Players[seatNumber].transform.Find("Cards/Card 2").gameObject);
    }
    void HideMainPlayerCards()
    {
        HideCard(MainPlayerCards[0]);
        HideCard(MainPlayerCards[1]);
    }
    void HideCardsOnDeck()
    {
        for(int i = 0; i < CardsObject.Length; i++)
        {
            HideCard(CardsObject[i]);
        }
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
    
    // Aktualizacja info danego gracza o jego zak�adzie i ile mu zosta�o �eton�w
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
        // P�tla po wszystkich graczach, �eby zaktualizowa� ich wy�wietlane informacje
        // (na razie tylko o zak�adach i posiadanych �etonach)
        // TODO doda� tu aktualizowanie wy�wietlania kart na stoliku i u graczy
        int i = 0;
        foreach (KeyValuePair<string, PlayerState> state in this.playersStates)
        {
            PlayerState playerState = state.Value;

            // Je�li to g��wny gracz, to mamy od tego osobne zmienne
            // TODO mo�na by to zmieni�, ale nwm, mo�e tak w sumie wygodniej?
            if (playerState.Nick == MyGameManager.Instance.MainPlayer.Nick)
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

        // Wy�wietlanie Popupu o kolejno�ci ruchu
        if (this.displayPlayerTurnPopup && PopupWindow)
        {
            var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
            popup.GetComponent<TextMeshProUGUI>().text = "It's your turn, make a move";
            this.displayPlayerTurnPopup = false;
        }
        // Wy�wietlanie Popupu o zwyci�zcy gry
        if (this.displayWinnerPopup && PopupWindow)
        {
            var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
            popup.GetComponent<TextMeshProUGUI>().text = "And the winner is:\n" + this.winnerNick + "\nCongrats!";
            this.displayWinnerPopup = false;
        }
    }

    // Wczytanie stawki z pola input 'Bid'
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

    // Obs�uga przycisk�w z menu ruch�w,
    // wysy�anie odpowiednich zapyta� do serwera w ka�dym z nich
    // TODO wysy�anie ��da� mo�na ewentualnie przenie�� do osobnej klasy
    public void OnCheckButton()
    {
        Debug.Log("Check");
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
        if (this.readyToSendMove)
        {
            NetworkStream gameStream = MyGameManager.Instance.gameServerConnection.stream;
            NetworkHelper.WriteNetworkStream(gameStream, "0 ");
            this.readyToSendMove = false;
        }
    }
    // TODO doda� sprawdzanie, czy podali�my jaki� zak�ad w polu input 'Bid' i czy to liczba,
    // bo aktualnie podajemy po prostu string
    public void OnBidButton()
    {
        Debug.Log("Bid");
        if (this.readyToSendMove)
        {
            NetworkStream gameStream = MyGameManager.Instance.gameServerConnection.stream;
            NetworkHelper.WriteNetworkStream(gameStream, "2 " + this.betFieldText.ToString());
            this.readyToSendMove = false;
        }
    }

    // Wysy�anie zapytania do serwera o rozpocz�cie gry
    // TODO mo�e przenie�� kiedy� do osobnej klasy
    public void onStartGameButton()
    {
        string token = MyGameManager.Instance.clientToken;
        byte[] toSend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "6" + ' ');
        MyGameManager.Instance.mainServerConnection.stream.Write(toSend, 0, toSend.Length);
        MyGameManager.Instance.mainServerConnection.stream.Flush();
        Thread.Sleep(1000);
    }
}
