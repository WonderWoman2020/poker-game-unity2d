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


public class ChangeNameMenu : MonoBehaviour
{

    [SerializeField] private Button changeNameButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_InputField newNameField;

    public GameObject PopupWindow;

    private string newName;
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

    public void OnChangeNameButton()
    {
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;

        string token = MyGameManager.Instance.clientToken;
        byte[] toSend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "8" + ' ' + this.newName + ' ' + this.password + ' ');
        mainServer.stream.Write(toSend, 0, toSend.Length);
        mainServer.stream.Flush();

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
            else if (response[0] == "answer 8 0 ")
            {
                ShowPopup("Nick changed successfuly!");
                return;
            }
            else if (response[0] == "answer 8 1 ")
            {
                ShowPopup("This nick is already in use. Try a different one");
                return;
            }
            else if (response[0] == "answer 8 2 ")
            {
                ShowPopup("Incorrect password");
                return;
            }
            else if (response[0] == "answer 8 3 ")
            {
                ShowPopup("An error with the database occured, please try again later");
                return;
            }
            else if (response[0] == "answer 8 A ")
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


    public void ReadNewName(string newName)
    {
        if (newName.Length == 0)
        {
            this.newName = null;
            return;
        }

        this.newName = newName;
        Debug.Log(this.newName);
    }

    public void ReadPassword(string password)
    {
        if (password.Length == 0)
        {
            this.password = null;
            return;
        }

        this.password = password;
        Debug.Log(this.password);
    }


    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }

}
