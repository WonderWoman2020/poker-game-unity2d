using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using TMPro;
using System;

using PokerGameClasses;

public class CreateTable : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button backToMenuButton;

    [SerializeField] private Button modeNoBotsButton;
    [SerializeField] private Button modeYouAndBotsButton;
    [SerializeField] private Button modeMixedButton;

    // TODO dodaæ zczytywanie liczby botów! Jest pole w interfejsie, nie ma tu jeszcze metody onClick pobieraj¹cej z niego
    //[SerializeField] private TMP_Text botsNumberField;

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
    public void OnCreateButton()
    {
        HumanPlayer p = (HumanPlayer)MyGameManager.Instance.MainPlayer;
        if(p == null)
        {
            Debug.Log("Table not created. Player was null");
            if (PopupWindow)
            {
                ShowPlayerNullPopup();
            }
            return;
        }

        if(this.tableName == null)
        {
            Debug.Log("You must set at least table name to create it.");
            if (PopupWindow)
            {
                ShowTableNameEmptyPopup();
            }
            return;
        }

        SendTableToServer();
        // TODO dodaæ kiedyœ czekanie na odpowiedŸ od serwera czy siê uda³o stworzyæ stolik
        SceneManager.LoadScene("Table");
    }


    //NA CHWILE OBECNA, LICZBA BOTOW JEST HARDKODOWANA (patrz linijki 34, 163, 171, 178). DO POPRAWY POZNIEJ
    // TODO dodaæ wartoœci domyœlne dla pól innych ni¿ nazwa stolika, jeœli gracz ich nie poda³, skoro obowi¹zkowo wymagamy tylko podania nazwy stolika
    void SendTableToServer()
    {
        if (this.numberOfBots == null)
            this.numberOfBots = "0";

        int mode = (int)this.chosenMode;

        string token = MyGameManager.Instance.clientToken;
        byte[] toSend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "0" + ' ' + this.tableName + ' ' + mode.ToString() + ' ' + this.numberOfBots + ' ' + this.xp + ' ' + this.chips + ' ');
        MyGameManager.Instance.mainServerConnection.stream.Write(toSend, 0, toSend.Length);
        MyGameManager.Instance.mainServerConnection.stream.Flush();
    }

    void ShowPlayerNullPopup()
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = "Table not created. Player was null";
    }
    void ShowTableNameEmptyPopup()
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = "You must set at least table name to create it.";
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

    public void OnModeNoBotsButton()
    {
        this.chosenMode = GameMode.No_Bots;
        this.numberOfBots = "0";
        Debug.Log(this.chosenMode);
    }

    public void OnYouAndBotsButton()
    {
        this.chosenMode = GameMode.You_And_Bots;
        this.numberOfBots = "3";
        Debug.Log(this.chosenMode);
    }

    public void OnMixedButton()
    {
        this.chosenMode = GameMode.Mixed;
        Debug.Log(this.chosenMode);
    }
}
