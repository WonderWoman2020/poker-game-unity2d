using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public GameObject PopupWindow;

    private string password;

    // Start is called before the first frame update
    void Start()
    {
        
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

        // odbierz odpowiedü
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
        }
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("SettingsMenu");
    }

    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }
}
