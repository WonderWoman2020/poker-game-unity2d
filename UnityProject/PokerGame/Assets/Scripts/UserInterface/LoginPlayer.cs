using PokerGameClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


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
        Debug.Log("button clicked");
        //SceneManager.LoadScene("PlayMenu");
        StartCoroutine(SendNewUser());
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
        Debug.Log(request.downloadHandler.text);
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
        Debug.Log(this.playerLogin);
    }

    public void ReadIP(string IP)
    {
        if(IP.Length == 0)
        {
            this.IP = null;
            return;
        }
        this.IP = IP;
        Debug.Log(this.IP);
    }
    public void ReadPassword(string password)
    {
        if (password.Length == 0)
        {
            this.playerPassword = null;
            return;
        }

        this.playerPassword = password;
        Debug.Log(this.playerPassword);
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
