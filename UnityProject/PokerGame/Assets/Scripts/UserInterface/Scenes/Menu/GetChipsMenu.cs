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
        //TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;
        //NetworkHelper.WriteNetworkStream(mainServer.stream, MyGameManager.Instance.clientToken + ' ' + "7" + ' ' + "1000");
        //mainServer.stream.Flush();
        //Thread.Sleep(1000);
        //if (mainServer.stream.DataAvailable)
        //{
        //    string response = NetworkHelper.ReadNetworkStream(mainServer.stream);
        //    mainServer.stream.Flush();
        //    string[] splitted = response.Split(' ');
        //    for (int i = 0; i < splitted.Length; i++)
        //    {
        //        Debug.Log(splitted[i]);
        //    }
        //    // arg 0 - bool czy siê uda³o zmieniæ coins gracza na serwerze
        //    if (splitted[0] == "1")
        //    {
        //        // arg 1 - aktualna wartoœæ coins gracza, jeœli siê uda³o
        //        int coins = Convert.ToInt32(splitted[1]);
        //        MyGameManager.Instance.MainPlayer.TokensCount = coins;
        //    }
        //}
        //Debug.Log("Get Chips");
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
