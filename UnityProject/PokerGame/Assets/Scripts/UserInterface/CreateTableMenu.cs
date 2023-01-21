using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

using TMPro;
using System;

public class CreateTableMenu : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button backToMenuButton;

    [SerializeField] private Button modeNoBotsButton;
    [SerializeField] private Button modeYouAndBotsButton;
    [SerializeField] private Button modeMixedButton;

    private GameMode chosenMode;

    public GameObject PopupWindow;

    private string tableName;
    private string chips;
    private string xp;

    // Start is called before the first frame update
    void Start()
    {
        this.chosenMode = GameMode.No_Bots;
        this.tableName = null;
        this.chips = null;
        this.xp = null;
    }

    // Update is called once per frame
    void Update()
    {
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

        GameTable gameTable = p.CreateYourTable("Unnamed table", null);
        this.SetGameTableInputData(gameTable);
        MyGameManager.Instance.AddTableToGame(gameTable);
        Debug.Log("Player "+p.Nick+ " created table "+gameTable);
        //SceneManager.LoadScene("Table");
        SceneManager.LoadScene("PlayMenu");
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

    private bool SetGameTableInputData(GameTable gameTable)
    {
        //data from input
        if (this.tableName != null)
            gameTable.ChangeName(this.tableName);

        if (this.chips != null)
            gameTable.Settings.changeMinTokens(Convert.ToInt32(this.chips));

        if (this.xp != null)
            gameTable.Settings.changeMinXP(Convert.ToInt32(this.xp));

        gameTable.Settings.changeMode(this.chosenMode);

        return true;
    }

    public void OnModeNoBotsButton()
    {
        this.chosenMode = GameMode.No_Bots;
        Debug.Log(this.chosenMode);
    }

    public void OnYouAndBotsButton()
    {
        this.chosenMode = GameMode.You_And_Bots;
        Debug.Log(this.chosenMode);
    }

    public void OnMixedButton()
    {
        this.chosenMode = GameMode.Mixed;
        Debug.Log(this.chosenMode);
    }
}
