using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Diagnostics;

using PokerGameClasses;

using TMPro;
using System;
using System.Net.Sockets;
using System.Threading;

using pGrServer;
using System.Text;


// Ekran do wyboru stolika do do��czenia, z list� istniej�cych na serwerze stolik�w
public class JoinTable : MonoBehaviour
{
    // Przyciski menu ekranu
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backToMenuButton;

    [SerializeField] private GameObject tableTemplate;
    [SerializeField] private GameObject noTableText;
    [SerializeField] private GameObject tablesContainer;
    // informacje o b��dach, komunikaty dla gracza
    public GameObject PopupWindow;

    // Numer przycisku obok stolika, kt�ry zosta� wybrany
    private int chosenTable;
    private string chosenTableName;

    // Zmienna sprawdzajaca, czy wybrany stolik wciaz istnieje
    private bool chosenTableStillExists;


    // Informacje o wst�pnie zaznaczonym stoliku, wy�wietlane w lewym dolnym rogu ekranu
    [SerializeField] private TMP_Text InfoPlayersCount;
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



        // Domy�lnie nie wybrano stolika
        this.chosenTable = -1;

    }

    // TODO doda� kiedy� do osobnej klasy
    public void loadTables()
    {
        
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;
        byte[] request = System.Text.Encoding.ASCII.GetBytes(MyGameManager.Instance.clientToken + ' ' + "2");
        mainServer.stream.Write(request, 0, request.Length);
        MyGameManager.Instance.mainServerConnection.stream.Flush();

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        while(stopwatch.Elapsed.TotalSeconds < 5 && !mainServer.stream.DataAvailable) {}
        stopwatch.Stop();

        if (mainServer.stream.DataAvailable)
        {
            // Usu� poprzednio za�adowane stoliki
            MyGameManager.Instance.GameTableList.Clear();
            byte[] readBuf = new byte[4096];
            StringBuilder menuRequestStr = new StringBuilder();
            int nrbyt = mainServer.stream.Read(readBuf, 0, readBuf.Length);
            MyGameManager.Instance.mainServerConnection.stream.Flush();
            menuRequestStr.AppendFormat("{0}", Encoding.ASCII.GetString(readBuf, 0, nrbyt));
            string[] response = menuRequestStr.ToString().Split(new string(":T:"));

            if (response[0] == "answer Z 1 ")
            {
                ShowPopup("Error: bad request");
                return;
            }
            else if (response[0] == "answer 2 1 ")
            {
                //TODO: Display the message on the list, permanently, instead of creating a popup
                ShowPopup("There are no tables currently!");
                return;
            }
            else if (response[0] == "answer 2 2 ")
            {
                ShowPopup("You are already sitting at a table!");
                return;
            }
            else if (response[0] == "answer 2 0 ")
            {
                this.chosenTableStillExists = false;
                for (int i = 1; i < response.Length; i++)
                {
                    parseTableData(response[i]);
                }
                if (!this.chosenTableStillExists)
                {
                    this.chosenTable = -1;
                }
                displayTables();
            }
        } else {
            ShowPopup("Couldn't get a response from the server, please try again later");
        }
    }

    // TODO doda� kiedy� do osobnej klasy
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
        // Je�li stolik�w jest wi�cej ni� tyle ile si� zmie�ci w naszym menu (obecnie 4 opcje),
        // wy�wietlamy tylko 4 pierwsze z listy
        // (TODO (cz. PGGP-34) mo�e warto zmieni�, �eby wy�wietla� 4 najnowsze, czyli 4 ostatnie?) 
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
        UnityEngine.Debug.Log(index);
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

        // Je�li nie ma dost�pnych stolik�w, cofa do ekranu PlayMenu
        if(MyGameManager.Instance.GameTableList.Count == 0)
        {
            UnityEngine.Debug.Log("There are no game tables to join. Create one first");
            if (PopupWindow)
            {
                ShowPopup("There are no game tables to join. Create one first");
            }
            SceneManager.LoadScene("PlayMenu");
            return;
        }

        // Info, �e nie wybrano �adnego stolika (nie cofa, zostajemy w tym ekranie)
        if(this.chosenTable == -1)
        {
            UnityEngine.Debug.Log("You didn't choose any game table. Choose one to join it by clicking the tick near it. ");
            if (PopupWindow)
            {
                ShowPopup("You didn't choose any game table. Choose one to join it by clicking the tick near it. ");
            }
            return;
        }

        // Pobranie wybranego przez gracza stolika z listy w MyGameManager
        GameTableInfo gameTable = MyGameManager.Instance.GameTableList[this.chosenTable];
        PlayerState player = MyGameManager.Instance.MainPlayer;

        // Sprawdzenie, czy gracz spe�nia warunki do��czenia do stolika
        // TODO mo�na by to doda� do osobnej metody
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
            UnityEngine.Debug.Log("Sending request to add player " + player.Nick + " to " + gameTable.Name);
            // zapytanie o dodanie do stolika na serwerze
            byte[] tosend = System.Text.Encoding.ASCII.GetBytes(MyGameManager.Instance.clientToken + ' ' + "1" + ' ' + MyGameManager.Instance.GameTableList[this.chosenTable].Name + ' ');
            NetworkStream ns = MyGameManager.Instance.mainServerConnection.stream;
            ns.Write(tosend, 0, tosend.Length);

            // Czekanie na odpowied� od serwera, czy zostali�my dodani
            // do wybranego stolika, zanim przejdziemy do sceny stolika
            // TODO zrobi� to lepiej ni� z sekundowym czasem oczekiwania, powinni�my gdzie� w osobnym w�tku odbiera� odpowiedzi
            Thread.Sleep(1000);
            bool joinedTheTable = false;
            if (ns.DataAvailable)
            {
                string response = NetworkHelper.ReadNetworkStream(ns);
                ns.Flush();
                UnityEngine.Debug.Log("Received response: "+response);
                string[] splitted = response.Split(' ');
                // arg 0 - numer rodzaju odpowiedzi (odpowied� na zapytanie o dodanie do stolika)
                if (splitted[1] == "1")
                {
                    // arg 1 - bool czy si� uda�o doda� do stolika
                    UnityEngine.Debug.Log(splitted[2]);
                    if (splitted[2] == "0")
                    {
                        joinedTheTable = true;
                    }
                    else if (splitted[2] == "1")
                    {
                        joinedTheTable = false;
                        ShowPopup("Couldn't join the game, you are already sitting at a table");
                    }
                    else if (splitted[2] == "2")
                    {
                        joinedTheTable = false;
                        ShowPopup("Couldn't find a table with this name");
                    }
                    else if (splitted[2] == "3")
                    {
                        joinedTheTable = false;
                        ShowPopup("Couldn't join the game, please try again later");
                    }
                    else if (splitted[2] == "A")
                    {
                        joinedTheTable = false;
                        ShowPopup("Something went wrong with sending information to the server, please try again later");
                    }
                    else if (response == "answer Z 1 ")
                    {
                        joinedTheTable = false;
                        ShowPopup("Error: bad request");
                    }
                }
                UnityEngine.Debug.Log("Joined bool value: " + joinedTheTable);
            }
            if (!joinedTheTable)
            {
                UnityEngine.Debug.Log("Player " + player.Nick + " wasn't added to " + gameTable.Name);
                //this.ShowPopup("Joining the table failed. The game by it has already started or it is an error.");
            }
            else
            {
                UnityEngine.Debug.Log("Added player " + player.Nick + " to " + gameTable.Name);
                SceneManager.LoadScene("Table");
            }
        }   
    }

    // TODO je�li dodamy tak� metod� do PopupText, wyrzuci�
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
        this.InfoMinChips.text = gameTable.minChips;
        this.InfoMinXP.text = gameTable.minXp;

        return true;
    }

    // Zapisanie numeru wybranego stolika po klikni�ciu przycisku obok niego, ze sprawdzaniem,
    // czy numer wybranego stolika nie jest wi�kszy, ni� liczba dost�pnych stolik�w
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
