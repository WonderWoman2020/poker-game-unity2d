using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

using TMPro;
using System;
using System.Threading;
using System.Net.NetworkInformation;
using System.Text;

public class PlayMenu : MonoBehaviour
{
    [SerializeField] private Button joinTableButton;
    [SerializeField] private Button createTableButton;
    [SerializeField] private Button getChipsButton;
    [SerializeField] private Button changeSettingsButton;

    [SerializeField] private TMP_Text InfoPlayerNick;
    [SerializeField] private TMP_Text InfoPlayerChips;
    [SerializeField] private TMP_Text InfoPlayerXP;

    // Start is called before the first frame update
    void Start()
    {
        if (MyGameManager.Instance.MainPlayer == null)
            return;

        this.InfoPlayerNick.text = MyGameManager.Instance.MainPlayer.Nick;
        this.InfoPlayerChips.text = Convert.ToString(MyGameManager.Instance.MainPlayer.TokensCount)+" $";
        this.InfoPlayerXP.text = Convert.ToString(MyGameManager.Instance.MainPlayer.XP)+ " XP";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void loadTables()
    {
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;
        byte[] request = System.Text.Encoding.ASCII.GetBytes(MyGameManager.Instance.clientToken + ' ' + "2");
        mainServer.stream.Write(request, 0, request.Length);
        MyGameManager.Instance.mainServerConnection.stream.Flush();
        Thread.Sleep(1000);
        if(mainServer.stream.DataAvailable)
        {
            byte[] readBuf = new byte[4096];
            StringBuilder menuRequestStr = new StringBuilder();
            int nrbyt = mainServer.stream.Read(readBuf, 0, readBuf.Length);
            MyGameManager.Instance.mainServerConnection.stream.Flush();
            menuRequestStr.AppendFormat("{0}", Encoding.ASCII.GetString(readBuf, 0, nrbyt));
            string[] tables = menuRequestStr.ToString().Split(new string(":T:"));
            for (int i = 1; i < tables.Length; i++)
            {
                UnityEngine.Debug.Log(tables[i]);
                parseTableData(tables[i]);
            }
        }
    }

    public void parseTableData(string serverResponse)
    {
        string[] data = serverResponse.Split(' ');
        string name = data[0];
        string owner = data[1];
        string humanCount = data[2];
        string botCount = data[3];
        string minXp = data[4];
        string minChips = data[5];

        GameTableInfo table = new GameTableInfo(name, owner, humanCount, botCount, minXp, minChips);
        MyGameManager.Instance.AddTableToListed(table);
    }


    public void OnJoinTableButton()
    {
        loadTables();
        SceneManager.LoadScene("JoinTable");
    }
    public void OnCreateTableButton()
    {
        SceneManager.LoadScene("CreateTable");
    }
    public void OnGetChipsButton()
    {
        Debug.Log("Get Chips");
    }
    public void OnChangeSettingsButton()
    {
        Debug.Log("Settings");
    }
}
