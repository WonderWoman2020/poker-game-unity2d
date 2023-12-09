using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using UnityEngine.SceneManagement;

//using System.Net.Sockets;

using PokerGameClasses;
using System.Text;
//using pGrServer;



public class TableSettings : MonoBehaviour
{

    [SerializeField] private Button changeSettingsButton;
    [SerializeField] private Button backButton;

    public GameObject PopupWindow;

    private string bigBlind;
    private string minTokens;
    private string maxPlayers;
    private string minPlayers;
    private string numberOfBots;
    private string maxMoveTime;
    private string minXp;

    private GameMode chosenMode;

    // TODO w stoliku jest jeszcze ustawienie min XP - mo¿na potem te¿ dodaæ do tego input

    // Start is called before the first frame update
    void Start()
    {
        this.bigBlind = null;
        this.minTokens = null;
        this.maxPlayers = null;
        this.minPlayers = null;
        this.numberOfBots = "0";
        this.maxMoveTime = null;
        this.minXp = null;

        this.chosenMode = GameMode.No_Bots;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChangeSettingsButton()
    {
        int mode = (int)this.chosenMode;

        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;

        string token = MyGameManager.Instance.clientToken;
        byte[] toSend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "5" + ' ' + mode.ToString() + ' ' + this.numberOfBots + ' ' + this.minXp + ' ' + this.minTokens + ' ' + this.bigBlind + ' ');
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
            else if (response[0] == "answer 5 1 ")
            {
                ShowPopup("You aren't at the table!");
                return;
            }
            else if (response[0] == "answer 5 2 ")
            {
                ShowPopup("You can't update table while a game is ongoing on it!");
                return;
            }
            else if (response[0] == "answer 5 9 ")
            {
                ShowPopup("A validation error has occured. Please check if all inputs are filled in correctly");
                return;
            }
            else if (response[0] == "answer 5 A ")
            {
                ShowPopup("Something went wrong with sending information to the server, please try again later");
                return;
            }
            else if (response[0] == "answer 5 0 ")
            {
                ShowPopup("Table settings updated!");
                SceneManager.LoadScene("Table");
            }
        }
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("Table");
    }

    public void ReadBigBlind(string bigBlind)
    {
        if (bigBlind.Length == 0)
        {
            this.bigBlind = null;
            return;
        }

        this.bigBlind = bigBlind;
        Debug.Log(this.bigBlind);
    }

    public void ReadMinTokens(string minTokens)
    {
        if (minTokens.Length == 0)
        {
            this.minTokens = null;
            return;
        }

        this.minTokens = minTokens;
        Debug.Log(this.minTokens);
    }

    public void ReadMinXp(string minXp)
    {
        if (minXp.Length == 0)
        {
            this.minXp = null;
            return;
        }

        this.minXp = minXp;
        Debug.Log(this.minXp);
    }

    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }
}
