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

using PokerGameClasses;

//using System.Diagnostics;
//Jesli ktos bedzie potrzebowal tej biblioteki, to bedzie potrzeba zamienic wszystkie Debug.Log(...) na UnityEngine.Debug.Log(...)


public class Login : MonoBehaviour
{
    private string playerLogin;
    private string playerPassword;
    private string IP;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button backToMenuButton;

    // informacje o b³êdach, komunikaty dla gracza
    public GameObject PopupWindow;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnLoginButton()
    {
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;

        if (this.playerLogin == null)
        {
            this.ShowWrongInputPopup("Add your login");
            return;
        }

        if(this.playerPassword == null)
        {
            this.ShowWrongInputPopup("Add your password");
            return;
        }

        // TODO dodaæ kiedyœ do klasy MenuRequestManager
        //////////////
        // wyœlij zapytanie o logowanie do serwerze
        byte[] message = System.Text.Encoding.ASCII.GetBytes(this.playerLogin + ' ' + this.playerPassword);
        mainServer.stream.Write(message, 0, message.Length);
        mainServer.stream.Flush();

        // odbierz odpowiedŸ
        byte[] myReadBuffer = new byte[1024];
        int numberOfBytesRead = 0;
        StringBuilder myCompleteMessage = new StringBuilder();
        numberOfBytesRead = mainServer.stream.Read(myReadBuffer, 0, myReadBuffer.Length);
        myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

        // TODO dodaæ reagowanie na b³êdn¹ odpowiedŸ od serwera (z³y login itd.)

        // odczytaj dane gracza z odpowiedzi (token, xp, coins, nick)
        string[] request = myCompleteMessage.ToString().Split(new char[] { ' ' });
        MyGameManager.Instance.clientToken = request[0];
        var xp = Int32.Parse(request[1]);
        var coins = Int32.Parse(request[2]);
        var nick = request[3];
        // stwórz g³ównego gracza
        Player player = new HumanPlayer(nick, PlayerType.Human, xp, coins);
        ///////////////
        
        // zapamiêtaj g³ównego gracza na ca³y czas dzia³ania aplikacji
        MyGameManager.Instance.AddPlayerToGame(player);
        SceneManager.LoadScene("PlayMenu");  
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
}
