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

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Button changePasswordButton;
    [SerializeField] private Button changeNameButton;
    [SerializeField] private Button deleteAccountButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button backButton;

    public GameObject PopupWindow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChangePasswordButton()
    {
        SceneManager.LoadScene("ChangePasswordMenu");
    }

    public void OnChangeNameButton()
    {
        SceneManager.LoadScene("ChangeNameMenu");
    }

    public void OnDeleteAccountButton()
    {
        // TODO sprawdza�, czy gracz jest zalogowany i nie wpuszcza� go tu je�li nie jest (+ Popup z odpowiednim info czemu nie m�g� wej��)
        SceneManager.LoadScene("DeleteAccount");
    }

    public void OnLogoutButton()
    {
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;
        string token = MyGameManager.Instance.clientToken;
        byte[] toSend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "3" + ' ');
        mainServer.stream.Write(toSend, 0, toSend.Length);
        mainServer.stream.Flush();

        // odbierz odpowied�
        byte[] readBuf = new byte[4096];
        StringBuilder menuRequestStr = new StringBuilder();
        int nrbyt = mainServer.stream.Read(readBuf, 0, readBuf.Length);
        mainServer.stream.Flush();
        menuRequestStr.AppendFormat("{0}", Encoding.ASCII.GetString(readBuf, 0, nrbyt));
        string[] response = menuRequestStr.ToString().Split(new string(":T:"));
        Debug.Log(response[0]);
        if (response[0] == "answer Z 1 ")
        {
            ShowPopup("Error: bad request");
        }
        else if (response[0] == "answer 3 A ")
        {
            ShowPopup("Something went wrong with sending information to the server, please try again later");
            return;
        }
        else if (response[0] == "answer 3 0 ")
        {
            ShowPopup("Logged out successfuly!");
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            ShowPopup("Something went wrong, please try again later");
        }

        //if (mainServer.stream.DataAvailable)
        //{
            
        //}
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }

    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }
}
