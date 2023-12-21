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



public class ChangePasswordMenu : MonoBehaviour
{

    [SerializeField] private Button changePasswordButton;
    [SerializeField] private Button backButton;

    [SerializeField] private TMP_InputField currentPasswordField;
    [SerializeField] private TMP_InputField newPasswordField;
    [SerializeField] private TMP_InputField confirmPasswordField;

    public GameObject PopupWindow;

    private string currentPassword;
    private string newPassword;
    private string confirmPassword;

    // Start is called before the first frame update
    void Start()
    {
        this.currentPasswordField.contentType = TMP_InputField.ContentType.Password;
        this.currentPasswordField.asteriskChar = '*';

        this.newPasswordField.contentType = TMP_InputField.ContentType.Password;
        this.newPasswordField.asteriskChar = '*';

        this.confirmPasswordField.contentType = TMP_InputField.ContentType.Password;
        this.confirmPasswordField.asteriskChar = '*';
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChangePasswordButton()
    {
        if (this.newPassword != this.confirmPassword) {
            ShowPopup("New password and password confirmation need to match");
            return;
        }
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;

        string token = MyGameManager.Instance.clientToken;
        byte[] toSend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "9" + ' ' + this.currentPassword + ' ' + this.newPassword + ' ' + this.confirmPassword + ' ' );
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
            else if (response[0] == "answer 9 0 ")
            {
                ShowPopup("Password changed successfuly!");
                return;
            }
            else if (response[0] == "answer 9 1 ")
            {
                ShowPopup("New password and password confirmation need to match!");
                return;
            }
            else if (response[0] == "answer 9 2 ")
            {
                ShowPopup("Incorrect password");
                return;
            }
            else if (response[0] == "answer 9 3 ")
            {
                ShowPopup("An error with the database occured, please try again later");
                return;
            }
            else if (response[0] == "answer 9 A ")
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

    public void ReadCurrentPassword(string currentPswd)
    {
        if (currentPswd.Length == 0)
        {
            this.currentPassword = null;
            return;
        }

        this.currentPassword = currentPswd;
        Debug.Log(this.currentPassword);
    }

    public void ReadNewPassword(string newPswd)
    {
        if (newPswd.Length == 0)
        {
            this.newPassword = null;
            return;
        }

        this.newPassword = newPswd;
        Debug.Log(this.newPassword);
    }

    public void ReadConfirmPassword(string confirmPswd)
    {
        if (confirmPswd.Length == 0)
        {
            this.confirmPassword = null;
            return;
        }

        this.confirmPassword = confirmPswd;
        Debug.Log(this.confirmPassword);
    }

    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }
}
