using PokerGameClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Cache;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//using System.Diagnostics;
//Jesli ktos bedzie potrzebowal tej biblioteki, to bedzie potrzeba zamienic wszystkie Debug.Log(...) na UnityEngine.Debug.Log(...)


public class LoginPlayer : MonoBehaviour
{
    private string playerLogin;
    private string playerPassword;
    private string IP;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button backToMenuButton;

    public GameObject PopupWindow;

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnLoginButton()
    {
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;

        byte[] message = System.Text.Encoding.ASCII.GetBytes(this.playerLogin + ' ' + this.playerPassword);
        mainServer.stream.Write(message, 0, message.Length);
        mainServer.stream.Flush();

        byte[] myReadBuffer = new byte[1024];
        int numberOfBytesRead = 0;
        StringBuilder myCompleteMessage = new StringBuilder();
        numberOfBytesRead = mainServer.stream.Read(myReadBuffer, 0, myReadBuffer.Length);
        myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

        string[] request = myCompleteMessage.ToString().Split(new char[] { ' ' });
        MyGameManager.Instance.clientToken = request[0];
        var xp = Int32.Parse(request[1]);
        var coins = Int32.Parse(request[2]);
        var nick = request[3];
        Player player = new HumanPlayer(nick, PlayerType.Human, xp, coins);
        MyGameManager.Instance.AddPlayerToGame(player);

        //StartCoroutine(SendNewUser());

        
        SceneManager.LoadScene("PlayMenu");

        
    }
    IEnumerator SendNewUser()
    {
        var request = new UnityWebRequest("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/loginuser", "POST");

        string str = "{\"login\":\"" + this.playerLogin + "\",\"password\":\"" + this.playerPassword + "\"}";

        byte[] body = System.Text.Encoding.ASCII.GetBytes(str);

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        //Debug.Log(request.downloadHandler.text);
        var dataFromDatabase = request.downloadHandler.text.Split("\"");
        var responseCode = Regex.Match(dataFromDatabase[2], @"\d+").Value;

        if (responseCode == "412") // login exist
        {
            ShowWrongInputPopup("Login does not exists.");
        }
        else if (responseCode == "416") // ok
        {
            ShowWrongInputPopup("Wrong password.");
        }
        else if (responseCode == "210") // ok
        {
            var xp = Int32.Parse(Regex.Match(dataFromDatabase[6], @"\d+").Value);
            var coins = Int32.Parse(Regex.Match(dataFromDatabase[12], @"\d+").Value);
            var login = dataFromDatabase[9];
            var nick = dataFromDatabase[15];
            Player player = new HumanPlayer(nick, PlayerType.Human, xp, coins);
            MyGameManager.Instance.AddPlayerToGame(player);
            //Debug.Log("Logged player "+player.Nick);
            //SceneManager.LoadScene("Table");
            SceneManager.LoadScene("PlayMenu");
            //SceneManager.LoadScene("LoginPlayer");
        }
        else //failed
        {
            ShowWrongInputPopup("Server error.");
        }
    }

    void ShowWrongInputPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }
    public void ReadLogin(string login)
    {
        if (login.Length == 0)
        {
            this.playerLogin = null;
            return;
        }

        this.playerLogin = login;
    }

    public void ReadIP(string IP)
    {
        if(IP.Length == 0)
        {
            this.IP = null;
            return;
        }
        this.IP = IP;
    }
    public void ReadPassword(string password)
    {
        if (password.Length == 0)
        {
            this.playerPassword = null;
            return;
        }

        this.playerPassword = password;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
