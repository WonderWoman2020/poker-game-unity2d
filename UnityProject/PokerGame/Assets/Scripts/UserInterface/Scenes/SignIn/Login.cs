using System;
using System.Collections;
using System.Collections.Generic;
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


// Ekran do podawania danych logowania
public class Login : MonoBehaviour
{
    // Dane pobierane z formularza logowania
    private string playerLogin;
    private string playerPassword;
    private string IP;

    // Przyciski ekranu
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
        mainServer.Start();

        if (this.playerLogin == null)
        {
            this.ShowPopup("Add your login");
            return;
        }

        if(this.playerPassword == null)
        {
            this.ShowPopup("Add your password");
            return;
        }

        // TODO dodaæ kiedyœ do osobnej klasy
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
        string[] request = myCompleteMessage.ToString().Split(new char[] { ' ' });
        string token = request[0];

        if (token == "##&&@@0000")
        {
            this.ShowPopup("Server Error. Please try again later");
            mainServer.Close();
            return;
        }
        else if (token == "##&&@@0001" || token == "##&&@@0002")
        {
            this.ShowPopup("Login or password are incorrect");
            mainServer.Close();
            return;
        }
        else if (token == "##&&@@0003")
        {
            this.ShowPopup("This account is already logged in");
            mainServer.Close();
            return;
        }
        else  //Jesli wszystko sie udalo z logowaniem
        {
            // odczytaj dane gracza z odpowiedzi (token, xp, coins, nick)
            MyGameManager.Instance.clientToken = token;
            var xp = Int32.Parse(request[1]);
            var coins = Int32.Parse(request[2]);
            var nick = request[3];
            // stwórz g³ównego gracza
            PlayerState player = new PlayerState(nick, null, coins, 0, xp);
            ///////////////

            // zapamiêtaj g³ównego gracza na ca³y czas dzia³ania aplikacji
            MyGameManager.Instance.AddPlayerToGame(player);
            MyGameManager.Instance.gameServerConnection.Start();
            SceneManager.LoadScene("PlayMenu");
        }
    }

    // TODO tak¹ funkcjê mo¿na by mo¿e przenieœæ do klasy PopupText, upubliczniæ i stamt¹d u¿ywaæ?
    void ShowPopup(string text)
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
