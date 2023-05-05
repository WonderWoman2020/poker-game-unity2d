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

// G³ówny ekran gry - widok stolika, kart i graczy
public class Table : MonoBehaviour
{
    // Przyciski ruchów gracza (menu w lewym dolnym rogu ekranu)
    [SerializeField] private Button checkButton;
    [SerializeField] private Button allInButton;
    [SerializeField] private Button passButton;
    [SerializeField] private Button bidButton;

    // Dane pobrane z pola input 'Bid' (menu w lewym dolnym rogu ekranu)
    private string betFieldText;

    // Wyœwietlanie informacji o g³ównym graczu
    /*
     * - Nick
     * - Ile ma ¿etonów
     * - Ile postawi³ ¿etonów w tym rozdaniu
     * - Jego karty (GameObject'y)
     */
    [SerializeField] private TMP_Text InfoMainPlayerName;
    [SerializeField] private TMP_Text InfoMainPlayerChips;
    [SerializeField] private TMP_Text InfoMainPlayerBid;
    [SerializeField] private GameObject[] MainPlayerCards;


    // Obiekt menu ruchów gracza, u¿ywamy go do chowania lub pokazywania tego menu
    [SerializeField]
    private CanvasRenderer menuCanvas;

    // Lista sprite'ów kart, z której wybieramy odpowiedni
    // sprite do przypisania do GameObject'u karty gracza
    [SerializeField]
    private CardsSprites collection;

    // Prze³¹cznik ustawiany na 'true', kiedy serwer przyœle do klienta zapytanie o wykonanie ruchu
    private bool readyToSendMove = false;

    // Stan stolika, odebrany od serwera (wysy³a po ka¿dym ruchu kogokolwiek)
    private GameTableState gameTableState;
    // Stany wszystkich graczy, odebrane od serwera (wysy³a po ka¿dym ruchu kogokolwiek).
    // S³ownik: klucz - Nick danego gracza (odczytywany z PlayerState), wartoœæ - PlayerState
    private IDictionary<string, PlayerState> playersStates;

    // informacje o b³êdach, komunikaty dla gracza
    // m.in. komunikat o tym czyj ruch jest teraz
    public GameObject PopupWindow;

    // GameObject'y na ekranie (update'ujemy je, ¿eby wyœwietla³y info z gry na ekranie)
    // GameObject'y kart stolika (TYLKO te 5 kart, które le¿¹ na stoliku!)
    private GameObject[] CardsObject;
    // GameObject'y graczy (w nich s¹ te¿ GameObject'y ich kart)
    private GameObject[] Players
    {get; set;}

    // Prze³¹czniki, które wykorzystujemy w Update'owaniu sceny, ¿eby wiedzieæ,
    // kiedy mamy wyœwietliæ dany tekst informacyjny
    bool displayPlayerTurnPopup = false;
    bool displayWinnerPopup = false;

    // Nick zwyciêzcy gry, od serwera (wysy³a pod koniec gry)
    string winnerNick = null;


    // Start is called before the first frame update
    void Start()
    {
        ShowMenu(false); //zakrycie MENU na start
        ShowMenu(true);
        if (MyGameManager.Instance.MainPlayer == null)
            return;

        // Pierwszy update wyœwietlanego info g³ównego gracza
        this.InfoMainPlayerName.text = MyGameManager.Instance.MainPlayer.Nick;
        this.InfoMainPlayerChips.text = Convert.ToString(MyGameManager.Instance.MainPlayer.TokensCount) + " $";
        this.InfoMainPlayerBid.text = "Bet\n" + Convert.ToString(0) + " $";

        // Pobranie GameObject'ów przygotowanych na graczy i na karty stolika ze sceny
        // (Graczy mamy na sztywno utworzonych na scenie, a nie spawn'owanych po dojœciu kogoœ do stolika,
        // wiêc tutaj pobieramy wszystkie te puste szablony przygotowane na wyœwietlanie informacji o danym graczu)
        this.Players = GameObject.FindGameObjectsWithTag("Player");
        this.CardsObject = GameObject.FindGameObjectsWithTag("Card");

        // Testowanie pokazywania i ukrywania odpowiednich kart u graczy
        // oraz chowania i pokazywania tak¿e graczy
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

        // Inicjalizacja stanu stolika i s³ownika stanów graczy w grze
        this.gameTableState = new GameTableState();
        this.playersStates = new Dictionary<string, PlayerState>();

        // W³¹czenie osobnego w¹tku do komunikacji z serwerem na porcie od komunikatów z gry
        // W tym w¹tku Unity nie pozwala zmieniaæ nic na ekranie - update'owaæ wygl¹d
        // ekranu mo¿na tylko w w¹tku g³ównym, w którym dzia³a np. funkcja Start i Update
        new System.Threading.Thread(CommunicateWithServer).Start();
    }

    public void CommunicateWithServer()
    {
        // Pobranie strumienia do komunikacji z serwerem na porcie od zdarzeñ gry
        // (w³¹czamy te ³¹cze obecnie w ekranie Login po udanym logowaniu) - TODO mo¿e przenieœæ to dopiero do tego ekranu?
        NetworkStream gameStream = MyGameManager.Instance.gameServerConnection.stream;
        bool running = true;

        /* Pêtla do odbierania komunikatów od serwera
         * Tutaj odbierane s¹:
         * - zapytania od serwera o wykonanie ruchu przez gracza
         * - komunikaty od serwera z informacjami o stanie stolika i wszystkich graczy (w tym ich kartach)
         * (TODO (PGGP-54) zmieniæ, ¿eby odbierane by³y tylko karty naszego gracza, a reszty tylko na koniec gry - ale to akurat na serwerze)
         * - komunikaty od serwera czyj ruch i pod koniec gry, kto zwyciê¿y³
         */
        while (running)
        {
            if (gameStream.DataAvailable)
            {
                UnityEngine.Debug.Log("sa dane na strumieniu");
                string gameRequest = NetworkHelper.ReadNetworkStream(gameStream);
                gameStream.Flush();

                /* Wiadomoœci o zdarzeniach z gry od serwera s¹ rozdzielane znacznikiem :G: (od Game)
                 * i maj¹ postaæ Typ_wiadomoœci|Treœæ
                 * Podczas przesy³ania wiadomoœci o stanie gry wysy³anych jest na raz kilka wiadomoœci:
                 * (wszystkie zaczynaj¹ siê od znacznika :G:, wiêc w sumie to osobne wiadomoœci, ale to tak dla jasnoœci jak to dzia³a)
                 * - wiadomoœæ typu "Round" (która to runda gry) TODO (cz. PGGP-44) dodaæ jej odbieranie tu i wyœwietlanie gdzieœ na górze ekranu numeru rundy
                 * - wiadomoœæ typu "Table state"
                 * - kilka wiadomoœci typu "Player state" (o stanie ka¿dego z graczy)
                 * - wiadomoœæ typu "Which player turn"
                 */
                // TODO (cz. PGGP-44) dodaæ jeszcze odbieranie wiadomoœci typu 'Info' i wyœwietlanie takich komunikatów na ekranie
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
                        this.gameTableState.UnpackGameState(splitted); // Treœæ wiadomoœci o stanie stolika ma jak¹œ strukturê, któr¹ odpakowuje ta metoda
                        Debug.Log(this.gameTableState);
                    }
                    else if (splitted[0] == "Player state") // komunikat stanu któregoœ z graczy
                    {
                        PlayerState playerState = new PlayerState();
                        playerState.UnpackGameState(splitted); // Treœæ wiadomoœci o stanie gracza ma jak¹œ strukturê, któr¹ odpakowuje ta metoda
                        Debug.Log(playerState);
                        this.playersStates[playerState.Nick] = playerState;
                        Debug.Log("Player state count: " + this.playersStates.Count);
                    }
                    else if(splitted[0] == "Winner") // komunikat z Nick'iem zwyciêzcy
                    {
                        this.winnerNick = splitted[1];
                        this.displayWinnerPopup = true;
                    }
                }
            }
        }
    }

    // Przestawiene prze³¹cznika, ¿eby metoda Update w g³ównym w¹tku wiedzia³a, ¿e ma pokazaæ Popup
    // (inne w¹tki ni¿ g³ówny nie mog¹ zmieniaæ wygl¹du sceny w Unity)
    void CommunicatePlayersTurn(string currentPlayer)
    {
        if (currentPlayer == MyGameManager.Instance.MainPlayer.Nick)
        {
            this.displayPlayerTurnPopup = true;
        }
    }

    // Kiedy przyjdzie zapytanie od serwera o ruch gracza, ustawiamy prze³¹cznik,
    // ¿e gracz mo¿e teraz wys³aæ jeden ruch (prze³¹cznik siê przestawia ponownie na 'false'
    // w trakcie wysy³ania ruchu przez gracza (po klikniêciu przez niego któregoœ z przycisków od ruchów)
    void MoveRequestResponse(string[] splitted)
    {
        Debug.Log(splitted[0]);
        Debug.Log(splitted[1]);
        this.readyToSendMove = true;
        //Czekamy teraz na klikniecie ktoregos z przyciskow. wyslanie kolejnego requesta do serwera jest wykonywane w metodach przyciskow
    }

    // Metody od pokazywania i chowania kart, graczy i menu ruchów
    // TODO przenieœæ to do jakiejœ osobnej klasy?
    void ShowCard(Card card, GameObject cardObject)
    {
        cardObject.GetComponent<UnityEngine.UI.Image>().sprite = collection.cardsSpriteSerialization[card.Id];
    }

    void ShowPlayerCards(int seatNumber, CardsCollection cards)
    {
        if (cards == null)
        {
            Card cardBackSprite = new Card(0, 0, 52);
            for (int i = 0; i < MainPlayerCards.Length; i++)
                ShowCard(cardBackSprite, Players[seatNumber].transform.Find("Cards/Card " + (i + 1)).gameObject);
            return;
        }

        for (int i = 0; i < cards.Cards.Count; i++)
        {
            if (i >= 2)
                break;

            ShowCard(cards.Cards[i], Players[seatNumber].transform.Find("Cards/Card "+(i+1)).gameObject);
        }
    }
    void ShowMainPlayerCards(CardsCollection cards)
    {
        if (cards == null)
        {
            Card cardBackSprite = new Card(0, 0, 52);
            for (int i = 0; i < MainPlayerCards.Length; i++)
                ShowCard(cardBackSprite, MainPlayerCards[i]);
            return;
        }

        for (int i = 0; i < cards.Cards.Count; i++)
        {
            if (i >= 2)
                break;

            ShowCard(cards.Cards[i], MainPlayerCards[i]);
        }
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
    
    // Aktualizacja info danego gracza o jego zak³adzie i ile mu zosta³o ¿etonów
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
        // Pêtla po wszystkich graczach, ¿eby zaktualizowaæ ich wyœwietlane informacje
        // (na razie tylko o zak³adach i posiadanych ¿etonach)
        // TODO dodaæ tu aktualizowanie wyœwietlania kart na stoliku i u graczy
        int i = 0;
        foreach (KeyValuePair<string, PlayerState> state in this.playersStates)
        {
            PlayerState playerState = state.Value;

            // Jeœli to g³ówny gracz, to mamy od tego osobne zmienne
            // TODO mo¿na by to zmieniæ, ale nwm, mo¿e tak w sumie wygodniej?
            if (playerState.Nick == MyGameManager.Instance.MainPlayer.Nick)
            {
                this.InfoMainPlayerName.text = playerState.Nick;
                this.InfoMainPlayerChips.text = Convert.ToString(playerState.TokensCount) + " $";
                this.InfoMainPlayerBid.text = "Bet\n" + Convert.ToString(playerState.CurrentBet) + " $";
                this.ShowMainPlayerCards(playerState.Hand); // karty g³ównego gracza
                continue;
            }

            this.ShowPlayerOnTable(i, playerState.Nick);
            this.ChangePlayerBet(playerState.CurrentBet, i);
            this.ChangePlayerMoney(playerState.TokensCount, i);
            this.ShowPlayerCards(i, playerState.Hand); // karty wspó³graczy
            i++;
        }

        // Wyœwietlanie kart na stoliku
        if (this.gameTableState.Cards != null)
        {
            for (int j = 0; j < this.gameTableState.Cards.Cards.Count; j++)
                ShowCardOnDeck(this.gameTableState.Cards.Cards[j], j);
        }
        else
        {
            Card cardBackSprite = new Card(0, 0, 52);
            for (int j = 0; j < CardsObject.Length; j++)
                ShowCardOnDeck(cardBackSprite, j);
        }

        // Wyœwietlanie Popupu o kolejnoœci ruchu
        if (this.displayPlayerTurnPopup && PopupWindow)
        {
            var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
            popup.GetComponent<TextMeshProUGUI>().text = "It's your turn, make a move";
            this.displayPlayerTurnPopup = false;
        }
        // Wyœwietlanie Popupu o zwyciêzcy gry
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

    // Obs³uga przycisków z menu ruchów,
    // wysy³anie odpowiednich zapytañ do serwera w ka¿dym z nich
    // TODO wysy³anie ¿¹dañ mo¿na ewentualnie przenieœæ do osobnej klasy
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
    // TODO (cz. PGGP-106) dodaæ sprawdzanie, czy podaliœmy jakiœ zak³ad w polu input 'Bid' i czy to liczba,
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

    // Wysy³anie zapytania do serwera o rozpoczêcie gry
    // TODO mo¿e przenieœæ kiedyœ do osobnej klasy
    public void onStartGameButton()
    {
        string token = MyGameManager.Instance.clientToken;
        byte[] toSend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "6" + ' ');
        MyGameManager.Instance.mainServerConnection.stream.Write(toSend, 0, toSend.Length);
        MyGameManager.Instance.mainServerConnection.stream.Flush();
        Thread.Sleep(1000);
    }
}
