using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

using TMPro;
using System;
using System.Net.Sockets;
using System.Threading;

using pGrServer;
using System.Text;


// Ekran do wyboru stolika do do³¹czenia, z list¹ istniej¹cych na serwerze stolików
public class JoinTable : MonoBehaviour
{
    // Przyciski menu ekranu
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backToMenuButton;

    [SerializeField] private GameObject tableTemplate;
    [SerializeField] private GameObject noTableText;
    [SerializeField] private GameObject tablesContainer;
    // informacje o b³êdach, komunikaty dla gracza
    public GameObject PopupWindow;

    // Numer przycisku obok stolika, który zosta³ wybrany
    private int chosenTable;
    private string chosenTableName;

    // Zmienna sprawdzajaca, czy wybrany stolik wciaz istnieje
    private bool chosenTableStillExists;


    // Informacje o wstêpnie zaznaczonym stoliku, wyœwietlane w lewym dolnym rogu ekranu
    [SerializeField] private TMP_Text InfoPlayersCount;
    [SerializeField] private TMP_Text InfoBotsCount;
    [SerializeField] private TMP_Text InfoMinChips;
    [SerializeField] private TMP_Text InfoMinXP;
    private int id = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (MyGameManager.Instance.GameTableList == null)
            return;
        DeleteTablesOnCanva();
        InvokeRepeating("loadTables", 0.0f, 10.0f);



        // Domyœlnie nie wybrano stolika
        this.chosenTable = -1;

    }

    // TODO dodaæ kiedyœ do osobnej klasy
    public void loadTables()
    {
        
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;
        byte[] request = System.Text.Encoding.ASCII.GetBytes(MyGameManager.Instance.clientToken + ' ' + "2");
        mainServer.stream.Write(request, 0, request.Length);
        MyGameManager.Instance.mainServerConnection.stream.Flush();
        Thread.Sleep(1000);
        if (mainServer.stream.DataAvailable)
        {
            // Usuñ poprzednio za³adowane stoliki
            MyGameManager.Instance.GameTableList.Clear();
            byte[] readBuf = new byte[4096];
            StringBuilder menuRequestStr = new StringBuilder();
            int nrbyt = mainServer.stream.Read(readBuf, 0, readBuf.Length);
            MyGameManager.Instance.mainServerConnection.stream.Flush();
            menuRequestStr.AppendFormat("{0}", Encoding.ASCII.GetString(readBuf, 0, nrbyt));
            string[] tables = menuRequestStr.ToString().Split(new string(":T:"));
            this.chosenTableStillExists = false;
            for (int i = 1; i < tables.Length; i++)
            {
                UnityEngine.Debug.Log(tables[i]);
                parseTableData(tables[i]);
            }
            if(!this.chosenTableStillExists)
            {
                this.chosenTable = -1;
            }
            displayTables();

        }
    }

    // TODO dodaæ kiedyœ do osobnej klasy
    public void parseTableData(string serverResponse)
    {
        string[] data = serverResponse.Split(' ');
        string name = data[0];
        string owner = data[1];
        string humanCount = data[2];
        string botCount = data[3];
        string minXp = data[4];
        string minChips = data[5];

        if (name == this.chosenTableName) 
        {
            this.chosenTableStillExists = true;
        }

        GameTableInfo table = new GameTableInfo(name, owner, humanCount, botCount, minXp, minChips);
        MyGameManager.Instance.AddTableToListed(table);
    }

    public void displayTables()
    {
        // Jeœli stolików jest wiêcej ni¿ tyle ile siê zmieœci w naszym menu (obecnie 4 opcje),
        // wyœwietlamy tylko 4 pierwsze z listy
        // (TODO (cz. PGGP-34) mo¿e warto zmieniæ, ¿eby wyœwietlaæ 4 najnowsze, czyli 4 ostatnie?) 
        DeleteTablesOnCanva();
        int tablesToShow = MyGameManager.Instance.GameTableList.Count;
        GameObject table;
        GameObject tableList = GameObject.FindGameObjectWithTag("TablesList");
        GameObject tableContainer = Instantiate(tablesContainer, tableList.transform);

        for (int i = 0; i < tablesToShow; i++)
        {
            
            table = Instantiate(tableTemplate, tableContainer.transform);
            GameObject tableNameGameObject = table.transform.Find("Button/TableName").gameObject;
            // Pokazywanie nazwy danego stoliku obok przycisku wyboru
            tableNameGameObject.GetComponent<TMP_Text>().text = MyGameManager.Instance.GameTableList[i].Name;
            Button tableButton = table.transform.Find("Button").gameObject.GetComponent<Button>();
            tableButton.AddEventListener(id, OnTableButton);
            id++;
        }
    }

    void OnTableButton(int index)
    {
        Debug.Log(index);
        this.chosenTable = index;
        this.UpdateGameTableInfo(MyGameManager.Instance.GameTableList[this.chosenTable]);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    void DeleteTablesOnCanva()
    {
        id = 0;
        GameObject tables = GameObject.FindGameObjectWithTag("TablesList");
        GameObject tablesContainer;
        try
        {
            tablesContainer = tables.transform.Find("TablesContainer(Clone)").gameObject;
            Destroy(tablesContainer);
        }
        catch (Exception e) { }
    }

    public void OnJoinButton()
    {

        // Jeœli nie ma dostêpnych stolików, cofa do ekranu PlayMenu
        if(MyGameManager.Instance.GameTableList.Count == 0)
        {
            Debug.Log("There are no game tables to join. Create one first");
            if (PopupWindow)
            {
                ShowPopup("There are no game tables to join. Create one first");
            }
            SceneManager.LoadScene("PlayMenu");
            return;
        }

        // Info, ¿e nie wybrano ¿adnego stolika (nie cofa, zostajemy w tym ekranie)
        if(this.chosenTable == -1)
        {
            Debug.Log("You didn't choose any game table. Choose one to join it by clicking the tick near it. ");
            if (PopupWindow)
            {
                ShowPopup("You didn't choose any game table. Choose one to join it by clicking the tick near it. ");
            }
            return;
        }

        // Pobranie wybranego przez gracza stolika z listy w MyGameManager
        GameTableInfo gameTable = MyGameManager.Instance.GameTableList[this.chosenTable];
        PlayerState player = MyGameManager.Instance.MainPlayer;

        // Sprawdzenie, czy gracz spe³nia warunki do³¹czenia do stolika
        // TODO mo¿na by to dodaæ do osobnej metody
        if(Int32.Parse(gameTable.minXp) > player.Xp)
        {
            this.ShowPopup("You can't join this table. You don't have enough XP");
            return;
        }
        else if (Int32.Parse(gameTable.minChips) > player.TokensCount)
        {
            this.ShowPopup("You can't join this table. You don't have enough chips");
            return;
        }
        else // ok
        {
            Debug.Log("Sending request to add player " + player.Nick + " to " + gameTable.Name);
            // zapytanie o dodanie do stolika na serwerze
            byte[] tosend = System.Text.Encoding.ASCII.GetBytes(MyGameManager.Instance.clientToken + ' ' + "1" + ' ' + MyGameManager.Instance.GameTableList[this.chosenTable].Name + ' ');
            NetworkStream ns = MyGameManager.Instance.mainServerConnection.stream;
            ns.Write(tosend, 0, tosend.Length);

            // Czekanie na odpowiedŸ od serwera, czy zostaliœmy dodani
            // do wybranego stolika, zanim przejdziemy do sceny stolika
            // TODO zrobiæ to lepiej ni¿ z sekundowym czasem oczekiwania, powinniœmy gdzieœ w osobnym w¹tku odbieraæ odpowiedzi
            Thread.Sleep(1000);
            bool joinedTheTable = false;
            if (ns.DataAvailable)
            {
                string response = NetworkHelper.ReadNetworkStream(ns);
                ns.Flush();
                Debug.Log("Received response: "+response);
                string[] splitted = response.Split(' ');
                // arg 0 - numer rodzaju odpowiedzi (odpowiedŸ na zapytanie o dodanie do stolika)
                if (splitted[1] == "1")
                {
                    // arg 1 - bool czy siê uda³o dodaæ do stolika
                    Debug.Log(splitted[2]);
                    if (splitted[2] == "0")
                    {
                        joinedTheTable = true;
                    }
                    else
                    {
                        joinedTheTable = false;
                        ShowPopup("Couldn't join table, the game is currently in progress");
                    }
                }
                Debug.Log("Joined bool value: " + joinedTheTable);
            }
            if (!joinedTheTable)
            {
                Debug.Log("Player " + player.Nick + " wasn't added to " + gameTable.Name);
                //this.ShowPopup("Joining the table failed. The game by it has already started or it is an error.");
            }
            else
            {
                Debug.Log("Added player " + player.Nick + " to " + gameTable.Name);
                SceneManager.LoadScene("Table");
            }
        }   
    }

    // TODO jeœli dodamy tak¹ metodê do PopupText, wyrzuciæ
    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }

    // Update info o wybranym stoliku w lewym dolnym rogu ekranu
    private bool UpdateGameTableInfo(GameTableInfo gameTable)
    {
        this.chosenTableName = gameTable.Name;
        this.InfoPlayersCount.text = gameTable.HumanCount;
        this.InfoBotsCount.text = gameTable.BotCount;
        this.InfoMinChips.text = gameTable.minChips;
        this.InfoMinXP.text = gameTable.minXp;

        return true;
    }

    // Zapisanie numeru wybranego stolika po klikniêciu przycisku obok niego, ze sprawdzaniem,
    // czy numer wybranego stolika nie jest wiêkszy, ni¿ liczba dostêpnych stolików
    //public void OnTable1Button()
    //{
    //    if (MyGameManager.Instance.GameTableList.Count >= 1)
    //    {
    //        this.chosenTable = 0;
    //        this.UpdateGameTableInfo(MyGameManager.Instance.GameTableList[this.chosenTable]);
    //    }
    //}

    //public void OnTable2Button()
    //{
    //    if (MyGameManager.Instance.GameTableList.Count >= 2)
    //    {
    //        this.chosenTable = 1;
    //        this.UpdateGameTableInfo(MyGameManager.Instance.GameTableList[this.chosenTable]);
    //    }
    //}

    //public void OnTable3Button()
    //{
    //    if (MyGameManager.Instance.GameTableList.Count >= 3)
    //    {
    //        this.chosenTable = 2;
    //        this.UpdateGameTableInfo(MyGameManager.Instance.GameTableList[this.chosenTable]);
    //    }
    //}

    //public void OnTable4Button()
    //{
    //    if (MyGameManager.Instance.GameTableList.Count >= 4)
    //    {
    //        this.chosenTable = 3;
    //        this.UpdateGameTableInfo(MyGameManager.Instance.GameTableList[this.chosenTable]);
    //    }
    //}
}
