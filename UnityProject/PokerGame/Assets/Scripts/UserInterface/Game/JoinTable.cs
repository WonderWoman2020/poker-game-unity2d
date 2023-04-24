using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

using TMPro;
using System;
using System.Net.Sockets;

using pGrServer;

public class JoinTable : MonoBehaviour
{
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backToMenuButton;

    [SerializeField] private Button table1Button;
    [SerializeField] private Button table2Button;
    [SerializeField] private Button table3Button;
    [SerializeField] private Button table4Button;

    // informacje o b³êdach, komunikaty dla gracza
    public GameObject PopupWindow;

    private int chosenTable;

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
        if (MyGameManager.Instance.GameTableList == null)
            return;

        this.chosenTable = -1;

        int tablesToShow = MyGameManager.Instance.GameTableList.Count;
        if (tablesToShow > 4)
            tablesToShow = 4;

        if(tablesToShow >= 1)
            this.Table1.text = MyGameManager.Instance.GameTableList[0].Name;
        if(tablesToShow >= 2)
            this.Table2.text = MyGameManager.Instance.GameTableList[1].Name;
        if (tablesToShow >= 3)
            this.Table3.text = MyGameManager.Instance.GameTableList[2].Name;
        if (tablesToShow >= 4)
            this.Table4.text = MyGameManager.Instance.GameTableList[3].Name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnJoinButton()
    {
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

        if(this.chosenTable == -1)
        {
            Debug.Log("You didn't choose any game table. Choose one to join it by clicking the tick near it. ");
            if (PopupWindow)
            {
                ShowPopup("You didn't choose any game table. Choose one to join it by clicking the tick near it. ");
            }
            return;
        }

        GameTableInfo gameTable = MyGameManager.Instance.GameTableList[this.chosenTable];
        PlayerState player = MyGameManager.Instance.MainPlayer;

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
        else
        {
            Debug.Log("Added player " + player.Nick + " to " + gameTable.Name);

            byte[] tosend = System.Text.Encoding.ASCII.GetBytes(MyGameManager.Instance.clientToken + ' ' + "1" + ' ' + MyGameManager.Instance.GameTableList[this.chosenTable].Name + ' ');
            NetworkStream ns = MyGameManager.Instance.mainServerConnection.stream;
            ns.Write(tosend, 0, tosend.Length);


            SceneManager.LoadScene("Table");
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

    private bool UpdateGameTableInfo(GameTableInfo gameTable)
    {
        this.InfoPlayersCount.text = gameTable.HumanCount;
        this.InfoBotsCount.text = gameTable.BotCount;
        this.InfoMinChips.text = gameTable.minChips;
        this.InfoMinXP.text = gameTable.minXp;

        return true;
    }

    public void OnTable1Button()
    {
        if (MyGameManager.Instance.GameTableList.Count >= 1)
        {
            this.chosenTable = 0;
            this.UpdateGameTableInfo(MyGameManager.Instance.GameTableList[this.chosenTable]);
        }
    }

    public void OnTable2Button()
    {
        if (MyGameManager.Instance.GameTableList.Count >= 2)
        {
            this.chosenTable = 1;
            this.UpdateGameTableInfo(MyGameManager.Instance.GameTableList[this.chosenTable]);
        }
    }

    public void OnTable3Button()
    {
        if (MyGameManager.Instance.GameTableList.Count >= 3)
        {
            this.chosenTable = 2;
            this.UpdateGameTableInfo(MyGameManager.Instance.GameTableList[this.chosenTable]);
        }
    }

    public void OnTable4Button()
    {
        if (MyGameManager.Instance.GameTableList.Count >= 4)
        {
            this.chosenTable = 3;
            this.UpdateGameTableInfo(MyGameManager.Instance.GameTableList[this.chosenTable]);
        }
    }
}
