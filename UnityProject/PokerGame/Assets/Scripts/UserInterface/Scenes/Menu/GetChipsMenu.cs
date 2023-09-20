using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using TMPro;
using System;
using System.Net.NetworkInformation;

using PokerGameClasses;
using System.Text;
using System.Linq;

public class GetChipsMenu : MonoBehaviour
{
    [SerializeField] private Button getChipsButton;
    [SerializeField] private Button backToMenuButton;
    public GameObject PopupWindow;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //TODO after the communication protocol is changed
    public void onGetChipsButton()
    {
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;
        string token = MyGameManager.Instance.clientToken;
        byte[] toSend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "7" + ' ');
        mainServer.stream.Write(toSend, 0, toSend.Length);
        mainServer.stream.Flush();
        Thread.Sleep(100);
        if (mainServer.stream.DataAvailable)
        {
            byte[] readBuf = new byte[4096];
            StringBuilder menuRequestStr = new StringBuilder();
            int nrbyt = mainServer.stream.Read(readBuf, 0, readBuf.Length);
            mainServer.stream.Flush();
            menuRequestStr.AppendFormat("{0}", Encoding.ASCII.GetString(readBuf, 0, nrbyt));
            string[] response = menuRequestStr.ToString().Split(new string(":T:"));
            string[] splitResponse = response[0].Split(' ');

            Debug.Log("Chips response: " + response[0]);
            Debug.Log("Chips splitResponse: " + splitResponse[2]);

            if (response[0] == "answer Z 1 ")
            {
                Debug.Log("IF ERROR");
                ShowPopup("Error: bad request");
            }
            else if (splitResponse[2] == "1")
            {
                Debug.Log("IF 1");
                ShowPopup("You can't receive more chips now! You'll be able to collect more in " + splitResponse[3] + " hours");
                return;
            }
            else if (splitResponse[2] == "2")
            {
                Debug.Log("IF 2");
                ShowPopup("You can't get more tokens while sitting at a table!");
                return;
            }
            else if (splitResponse[2] == "0")
            {
                Debug.Log("IF 0");
                ShowPopup("Received " + splitResponse[3] + " chips!");
                int coins = Convert.ToInt32(splitResponse[3]);
                MyGameManager.Instance.MainPlayer.TokensCount += coins;
                return;
            }
            else if (response[0] == "answer 7 A ")
            {
                Debug.Log("IF BAD");
                ShowPopup("Something went wrong with sending information to the server, please try again later");
                return;
            }
            Debug.Log("Chips response: " + response[0]);
            Debug.Log("Chips splitResponse: " + splitResponse[2]);
            Debug.Log("KONIEC1");
        }
        Debug.Log("KONIEC2");
    }

    public void onBackToMenuButton ()
    {
        SceneManager.LoadScene("PlayMenu");
    }

    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }
}
