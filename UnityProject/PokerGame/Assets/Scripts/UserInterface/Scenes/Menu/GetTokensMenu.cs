using pGrServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GetTokensMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnGetButton()
    {
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;
        NetworkHelper.WriteNetworkStream(mainServer.stream, MyGameManager.Instance.clientToken + ' ' + "7" + ' ' + "1000");
        mainServer.stream.Flush();
        Thread.Sleep(1000);
        if (mainServer.stream.DataAvailable)
        {
            string response = NetworkHelper.ReadNetworkStream(mainServer.stream);
            mainServer.stream.Flush();
            string[] splitted = response.Split(' ');
            // arg 0 - bool czy siê uda³o zmieniæ coins gracza na serwerze
            if (splitted[0] == "1")
            {
                // arg 1 - aktualna wartoœæ coins gracza, jeœli siê uda³o
                int coins = Convert.ToInt32(splitted[1]);
                MyGameManager.Instance.MainPlayer.TokensCount = coins;
               // this.ChangePlayerInfo();
            }
            
        }
        SceneManager.LoadScene("PlayMenu");
        Debug.Log("Get Chips");
    }

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }
}
