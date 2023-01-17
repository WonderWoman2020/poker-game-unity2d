using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

using System;

public class CreateTableMenu : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button backToMenuButton;

    private string tableName;
    private string chips;
    private string xp;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnCreateButton()
    {
        HumanPlayer p = (HumanPlayer)MyGameManager.Instance.MainPlayer;
        if(p == null)
        {
            Debug.Log("Table not created. Player was null");
            return;
        }
        GameTable gameTable = p.CreateYourTable("Test Table from HumanPlayer", null);
        this.SetGameTableInputData(gameTable);
        MyGameManager.Instance.AddTableToGame(gameTable);
        Debug.Log("Player "+p.Nick+ " created table "+gameTable);
        //SceneManager.LoadScene("Table");
        SceneManager.LoadScene("PlayMenu");
    }
    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }

    public void ReadGameTableName(string name)
    {
        this.tableName = name;
        Debug.Log(this.tableName);
    }

    public void ReadChips(string chips)
    {
        this.chips = chips;
        Debug.Log(this.chips);
    }

    public void ReadXP(string xp)
    {
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

        return true;
    }
}
