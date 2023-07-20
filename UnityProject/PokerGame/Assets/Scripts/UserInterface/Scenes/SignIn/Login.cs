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
using System.Security.Cryptography;


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

    [SerializeField] private TMP_InputField passwordField;

    // informacje o b��dach, komunikaty dla gracza
    public GameObject PopupWindow;

    // Start is called before the first frame update
    void Start()
    {
        this.passwordField.contentType = TMP_InputField.ContentType.Password;
        this.passwordField.asteriskChar = '*';
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
        if (!this.ValidateIP(this.IP))
        {
            this.ShowPopup("Incorrect IP. Input valid IP");
            return;
        }

        // Nawi�zanie po��czenia z serwerem na porcie od zdarze� w menu aplikacji
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;
        mainServer.Start();

        byte[] myReadBuffer = new byte[1024];
        int numberOfBytesRead = 0;
        StringBuilder publicKeyMessage = new StringBuilder();
        numberOfBytesRead = mainServer.stream.Read(myReadBuffer, 0, myReadBuffer.Length);
        publicKeyMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
        string[] request = publicKeyMessage.ToString().Split(new char[] { ' ' });
        string publicKeyXml = request[0];

        var rsa = new RSACryptoServiceProvider();
        rsa.PersistKeyInCsp = false;
        rsa.FromXmlString(publicKeyXml);

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

        // TODO doda� kiedy� do osobnej klasy
        //////////////
        // wy�lij zapytanie o logowanie do serwera
        string message = this.playerLogin + ' ' + this.playerPassword;
        byte[] encryptedMessage = rsa.Encrypt(Encoding.ASCII.GetBytes(message), false);
        mainServer.stream.Write(encryptedMessage, 0, encryptedMessage.Length);
        mainServer.stream.Flush();

        // odbierz odpowied�
        StringBuilder myCompleteMessage = new StringBuilder();
        numberOfBytesRead = mainServer.stream.Read(myReadBuffer, 0, myReadBuffer.Length);
        myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
        request = myCompleteMessage.ToString().Split(new char[] { ' ' });
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
            // stw�rz g��wnego gracza
            PlayerState player = new PlayerState(nick, null, coins, 0, xp, 0);
            ///////////////

            // zapami�taj g��wnego gracza na ca�y czas dzia�ania aplikacji
            MyGameManager.Instance.AddPlayerToGame(player);
            // Nawi�zanie po��czenia z serwerem na porcie od zdarze� w grze
            MyGameManager.Instance.gameServerConnection.Start();
            SceneManager.LoadScene("PlayMenu");
        }
    }

    // TODO tak� funkcj� mo�na by mo�e przenie�� do klasy PopupText, upubliczni� i stamt�d u�ywa�?
    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }

    public bool ValidateIP(string IP)
    {
        // funkcja systemowa do parsowania IP interpretuje pojedyncz� liczb� r�wnie� jako adres, np. 3 = 0.0.0.3
        // po to sprawdzamy, czy zosta�y podane kropki, �eby wykluczy� pojedyncz� liczb� jako dobry input
        System.Net.IPAddress parsedIP; // t� zmienn� ignoruj�, i tak chc� po prostu da� string'a do TcpConnection, a nie obiekt tej klasy
        if (IP.Contains("."))
            return System.Net.IPAddress.TryParse(IP, out parsedIP);

        return false;
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
        MyGameManager.Instance.ServerIP = IP;
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
