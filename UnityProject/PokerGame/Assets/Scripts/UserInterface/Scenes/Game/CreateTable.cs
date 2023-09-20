using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using TMPro;
using System;

using PokerGameClasses;
using System.Text;
using System.Linq;

// Ekran do podawania danych do stworzenia stolika
public class CreateTable : MonoBehaviour
{
    // Przycisku menu ekranu
    [SerializeField] private Button createButton;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] ToggleGroup toggleGroup;
    /* Przyciski do wyboru trybu gry (enum GameMode)
     * - Bez botów
     * - Tylko Ty i boty
     * - Gracze i boty
     */

    // informacje o b³êdach, komunikaty dla gracza
    public GameObject PopupWindow;

    // dane z formularza
    private string numberOfBots;
    private string tableName;
    private string chips;
    private string xp;
    private GameMode chosenMode;

    // Start is called before the first frame update
    void Start()
    {
        this.chosenMode = GameMode.No_Bots;
        this.numberOfBots = "0";
        this.tableName = null;
        this.chips = null;
        this.xp = null;
    }

    // Update is called once per frame
    void Update()
    {
        // wciœniêcie enter robi to samo co wciœniêcie przycisku 'Create'
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (this.tableName != null && this.chips != null && this.xp != null)
                this.OnCreateButton();
        }
    }
    public void GameModeSelection()
    {
        Toggle toggle = toggleGroup.ActiveToggles().FirstOrDefault();
        String txt = toggle.GetComponentInChildren<Text>().text;
        if(txt == "No bots")
        {
            this.chosenMode = GameMode.No_Bots;
            this.numberOfBots = "0";
            Debug.Log(this.chosenMode);
        }
        else if(txt == "Mixed")
        {
            this.chosenMode = GameMode.Mixed;
            Debug.Log(this.chosenMode);
        }
        else if(txt == "You and bots")
        {
            this.chosenMode = GameMode.You_And_Bots;
            Debug.Log(this.chosenMode);
        }
    }
    public void OnCreateButton()
    { 
        PlayerState p = MyGameManager.Instance.MainPlayer;
        if(p == null)
        {
            Debug.Log("Table not created. Player was null");
            if (PopupWindow)
            {
                ShowPopup("Table not created. Player was null");
            }
            return;
        }

        if(this.tableName == null)
        {
            Debug.Log("You must set at least table name to create it.");
            if (PopupWindow)
            {
                ShowPopup("You must set at least table name to create it.");
            }
            return;
        }
        GameModeSelection();
        SendTableToServer();
        // TODO (cz. PGGP-56) dodaæ kiedyœ czekanie na odpowiedŸ od serwera czy siê uda³o stworzyæ stolik
    }

    // TODO (cz. PGGP-106) dodaæ wartoœci domyœlne dla pól innych ni¿ nazwa stolika,
    // jeœli gracz ich nie poda³, skoro obowi¹zkowo wymagamy tylko podania nazwy stolika
    // TODO dodaæ kiedyœ do osobnej klasy
    void SendTableToServer()
    {
        if (this.numberOfBots == null)
            this.numberOfBots = "0";

        int mode = (int)this.chosenMode;

        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;

        string token = MyGameManager.Instance.clientToken;
        byte[] toSend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "0" + ' ' + this.tableName + ' ' + mode.ToString() + ' ' + this.numberOfBots + ' ' + this.xp + ' ' + this.chips + ' ' + "20" + ' ');
        mainServer.stream.Write(toSend, 0, toSend.Length);
        mainServer.stream.Flush();

        // odbierz odpowiedŸ
        if (mainServer.stream.DataAvailable)
        {
            byte[] readBuf = new byte[4096];
            StringBuilder menuRequestStr = new StringBuilder();
            int nrbyt = mainServer.stream.Read(readBuf, 0, readBuf.Length);
            mainServer.stream.Flush();
            menuRequestStr.AppendFormat("{0}", Encoding.ASCII.GetString(readBuf, 0, nrbyt));
            string[] response = menuRequestStr.ToString().Split(new string(":T:"));
            if (response[0] == "answer Z 1 ")
            {
                ShowPopup("Error: bad request");
                return;
            }
            else if (response[0] == "answer 0 1 ") {
                ShowPopup("You are already sitting at a table!");
                return;
            }
            else if (response[0] == "answer 0 2 ")
            {
                ShowPopup("A table of this name already exists!");
                return;
            }
            else if (response[0] == "answer 0 9 ")
            {
                ShowPopup("Validation error! Please check if table name and other data is valid");
                return;
            }
            else if (response[0] == "answer 0 A ")
            {
                ShowPopup("Something went wrong with sending information to the server, please try again later");
                return;
            }
            else if (response[0] == "answer 0 0 ")
            {
                SceneManager.LoadScene("Table");
            }
        }
    }

    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }

    public void ReadGameTableName(string name)
    {
        if (name.Length == 0)
        {
            this.tableName = null;
            return;
        }

        this.tableName = name;
        Debug.Log(this.tableName);
    }

    public void ReadChips(string chips)
    {
        if (chips.Length == 0)
        {
            this.chips = null;
            return;
        }

        this.chips = chips;
        Debug.Log(this.chips);
    }

    public void ReadXP(string xp)
    {
        if (xp.Length == 0)
        {
            this.xp = null;
            return;
        }

        this.xp = xp;
        Debug.Log(this.xp);
    }

    public void ReadBotsNumber(string botsNumber)
    {
        if (botsNumber.Length == 0)
        {
            this.numberOfBots = null;
            return;
        }

        this.numberOfBots = botsNumber;
        Debug.Log(this.numberOfBots);
    }

}
