using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

using TMPro;
using UnityEngine.SceneManagement;
using System.Text;

//using System.Net.Sockets;

//using PokerGameClasses;
//using pGrServer;

public class DeleteAccount : MonoBehaviour
{

    [SerializeField] private Button deleteAccountButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_InputField passwordField;

    public GameObject PopupWindow;

    private string password;

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

    public void OnDeleteAccountButton()
    {
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;

        string token = MyGameManager.Instance.clientToken;
        byte[] toSend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "A" + ' ' + this.password + ' ');
        mainServer.stream.Write(toSend, 0, toSend.Length);
        mainServer.stream.Flush();

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        while(stopwatch.Elapsed.TotalSeconds < 5 && !mainServer.stream.DataAvailable) {}
        stopwatch.Stop();
        // odbierz odpowied�
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
            else if (response[0] == "answer A 0 ")
            {
                ShowPopup("Account deleted successfuly!");
                MyGameManager.Instance.MainPlayer = null;
                SceneManager.LoadScene("MainMenu");
            }
            else if (response[0] == "answer A 1 ")
            {
                ShowPopup("Incorrect password");
                return;
            }
            else if (response[0] == "answer A 2 ")
            {
                ShowPopup("An error with the database occured, please try again later");
                return;
            }
            else if (response[0] == "answer A 3 ")
            {
                ShowPopup("You can't delete your account currently");
                return;
            }
            else if (response[0] == "answer A A ")
            {
                ShowPopup("Something went wrong with sending information to the server, please try again later");
                return;
            }
        } else {
            ShowPopup("Couldn't get a response from the server, please try again later");
        }
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("SettingsMenu");
    }

    public void ReadPassword(string password)
    {
        if (password.Length == 0)
        {
            this.password = null;
            return;
        }

        this.password = password;
        UnityEngine.Debug.Log(this.password);
    }

    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }
}
