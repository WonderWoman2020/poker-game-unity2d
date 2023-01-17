using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

using TMPro;
using System;

public class JoinTable : MonoBehaviour
{
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backToMenuButton;

    [SerializeField] private Button table1Button;
    [SerializeField] private Button table2Button;
    [SerializeField] private Button table3Button;
    [SerializeField] private Button table4Button;

    private int choosenTable;

    //TODO make this a table of tables later
    [SerializeField] private TMP_Text Table1;
    [SerializeField] private TMP_Text Table2;
    [SerializeField] private TMP_Text Table3;
    [SerializeField] private TMP_Text Table4;

    [SerializeField] private TMP_Text InfoPlayersCount;
    [SerializeField] private TMP_Text InfoBotsCount;
    [SerializeField] private TMP_Text InfoMinChips;
    [SerializeField] private TMP_Text InfoMinXP;

    // Start is called before the first frame update
    void Start()
    {
        if (MyGameManager.Instance.GameTables == null)
            return;

        this.choosenTable = 0;

        int tablesToShow = MyGameManager.Instance.GameTables.Count;
        if (tablesToShow > 4)
            tablesToShow = 4;

        if(tablesToShow >= 1)
            this.Table1.text = MyGameManager.Instance.GameTables[0].Name;
        if(tablesToShow >= 2)
            this.Table2.text = MyGameManager.Instance.GameTables[1].Name;
        if (tablesToShow >= 3)
            this.Table3.text = MyGameManager.Instance.GameTables[2].Name;
        if (tablesToShow >= 4)
            this.Table4.text = MyGameManager.Instance.GameTables[3].Name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnJoinButton()
    {
        if(MyGameManager.Instance.GameTables.Count == 0)
        {
            Debug.Log("There are no game tables to join. Create one first");
            SceneManager.LoadScene("PlayMenu");
            return;
        }

        GameTable gameTable = MyGameManager.Instance.GameTables[this.choosenTable];
        Player player = MyGameManager.Instance.MainPlayer;
        gameTable.AddPlayer(player);
        Debug.Log("Added player " + player.Nick + " to "+gameTable.Name);
        SceneManager.LoadScene("Table");
    }
    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }

    private bool UpdateGameTableInfo(GameTable gameTable)
    {
        this.InfoPlayersCount.text = Convert.ToString(gameTable.Players.Count);
        this.InfoBotsCount.text = Convert.ToString(gameTable.GetPlayerTypeCount(PlayerType.Bot));
        this.InfoMinChips.text = Convert.ToString(gameTable.Settings.MinPlayersTokenCount);
        this.InfoMinXP.text = Convert.ToString(gameTable.Settings.MinPlayersXP);

        return true;
    }

    public void OnTable1Button()
    {
        if (MyGameManager.Instance.GameTables.Count >= 1)
        {
            this.choosenTable = 0;
            this.UpdateGameTableInfo(MyGameManager.Instance.GameTables[this.choosenTable]);
        }
    }

    public void OnTable2Button()
    {
        if (MyGameManager.Instance.GameTables.Count >= 2)
        {
            this.choosenTable = 1;
            this.UpdateGameTableInfo(MyGameManager.Instance.GameTables[this.choosenTable]);
        }
    }

    public void OnTable3Button()
    {
        if (MyGameManager.Instance.GameTables.Count >= 3)
        {
            this.choosenTable = 2;
            this.UpdateGameTableInfo(MyGameManager.Instance.GameTables[this.choosenTable]);
        }
    }

    public void OnTable4Button()
    {
        if (MyGameManager.Instance.GameTables.Count >= 4)
        {
            this.choosenTable = 3;
            this.UpdateGameTableInfo(MyGameManager.Instance.GameTables[this.choosenTable]);
        }
    }
}
