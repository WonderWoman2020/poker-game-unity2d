using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using TMPro;
using System;
using System.Diagnostics;

using PokerGameClasses;
using System.Text;
using System.Linq;

// Ekran do podawania danych do stworzenia stolika
public class CreateTable : MonoBehaviour
{
    // Przycisku menu ekranu
    [SerializeField] private Button createButton;
    [SerializeField] private Button backToMenuButton;
    //[SerializeField] ToggleGroup toggleGroup;

    // informacje o b��dach, komunikaty dla gracza
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
        // wci�ni�cie enter robi to samo co wci�ni�cie przycisku 'Create'
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (this.tableName != null && this.chips != null && this.xp != null)
                this.OnCreateButton();
        }
    }

    public void OnCreateButton()
    { 
        PlayerState p = MyGameManager.Instance.MainPlayer;
        if(p == null)
        {
            UnityEngine.Debug.Log("Table not created. Player was null");
            if (PopupWindow)
            {
                ShowPopup("Table not created. Player was null");
            }
            return;
        }

        if(this.tableName == null)
        {
            UnityEngine.Debug.Log("You must set at least table name to create it.");
            if (PopupWindow)
            {
                ShowPopup("You must set at least table name to create it.");
            }
            return;
        }
        //GameModeSelection();
        SendTableToServer();
        // TODO (cz. PGGP-56) doda� kiedy� czekanie na odpowied� od serwera czy si� uda�o stworzy� stolik
    }

    // TODO (cz. PGGP-106) doda� warto�ci domy�lne dla p�l innych ni� nazwa stolika,
    // je�li gracz ich nie poda�, skoro obowi�zkowo wymagamy tylko podania nazwy stolika
    // TODO doda� kiedy� do osobnej klasy
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
        //UnityEngine.Debug.Log("data available: " + mainServer.stream.DataAvailable);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        while(stopwatch.Elapsed.TotalSeconds < 5 && !mainServer.stream.DataAvailable) {}
        stopwatch.Stop();

        // odbierz odpowied�
        if (mainServer.stream.DataAvailable)
        {
            byte[] readBuf = new byte[4096];
            StringBuilder menuRequestStr = new StringBuilder();
            int nrbyt = mainServer.stream.Read(readBuf, 0, readBuf.Length);
            mainServer.stream.Flush();
            menuRequestStr.AppendFormat("{0}", Encoding.ASCII.GetString(readBuf, 0, nrbyt));
            string[] response = menuRequestStr.ToString().Split(new string(":T:"));
            UnityEngine.Debug.Log("response" + response[0]);
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
                MyGameManager.Instance.owner = MyGameManager.Instance.clientToken;
                SceneManager.LoadScene("Table");
            }
        } else {
            ShowPopup("Couldn't get a response from the server, please try again later");
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
        UnityEngine.Debug.Log(this.tableName);
    }

    public void ReadChips(string chips)
    {
        if (chips.Length == 0)
        {
            this.chips = null;
            return;
        }

        this.chips = chips;
        UnityEngine.Debug.Log(this.chips);
    }

    public void ReadXP(string xp)
    {
        if (xp.Length == 0)
        {
            this.xp = null;
            return;
        }

        this.xp = xp;
        UnityEngine.Debug.Log(this.xp);
    }
}
