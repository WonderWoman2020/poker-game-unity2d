using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;
using pGrServer;

using TMPro;
using System;
using System.Threading;
using System.Net.NetworkInformation;
using System.Text;

/* Menu opcji jakie dzia�anie w grze chcemy podj��
 * - stworzy� stolik
 * - wybra� i do��czy� do stolika
 * - kupi� �etony (na razie kupuje zahardkowan� warto�� 1000 �eton�w) - TODO (cz. PGGP-107) poprawi� to (ekran do podawania liczby �eton�w do kupienia czy co�)
 * - zmieni� ustawienia naszego stolika/rozgrywki (na razie nie mamy) - TODO (cz. PGGP-107) doda� taki ekran i pod��czy� tak� logik�
 */
public class PlayMenu : MonoBehaviour
{
    // Przyciski menu
    [SerializeField] private Button joinTableButton;
    [SerializeField] private Button createTableButton;
    [SerializeField] private Button getChipsButton;
    [SerializeField] private Button changeSettingsButton;

    // Informacje o graczu wy�wietlane na ekranie obok menu
    [SerializeField] private TMP_Text InfoPlayerNick;
    [SerializeField] private TMP_Text InfoPlayerChips;
    [SerializeField] private TMP_Text InfoPlayerXP;

    // Start is called before the first frame update
    void Start()
    {
        this.ChangePlayerInfo();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // TODO doda� kiedy� do osobnej klasy
    public void loadTables()
    {
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;
        byte[] request = System.Text.Encoding.ASCII.GetBytes(MyGameManager.Instance.clientToken + ' ' + "2");
        mainServer.stream.Write(request, 0, request.Length);
        MyGameManager.Instance.mainServerConnection.stream.Flush();
        Thread.Sleep(1000);
        if(mainServer.stream.DataAvailable)
        {
            // Usu� poprzednio za�adowane stoliki
            MyGameManager.Instance.GameTableList.Clear();
            byte[] readBuf = new byte[4096];
            StringBuilder menuRequestStr = new StringBuilder();
            int nrbyt = mainServer.stream.Read(readBuf, 0, readBuf.Length);
            MyGameManager.Instance.mainServerConnection.stream.Flush();
            menuRequestStr.AppendFormat("{0}", Encoding.ASCII.GetString(readBuf, 0, nrbyt));
            string[] tables = menuRequestStr.ToString().Split(new string(":T:"));
            for (int i = 1; i < tables.Length; i++)
            {
                UnityEngine.Debug.Log(tables[i]);
                parseTableData(tables[i]);
            }
        }
    }

    // TODO doda� kiedy� do osobnej klasy
    public void parseTableData(string serverResponse)
    {
        string[] data = serverResponse.Split(' ');
        string name = data[0];
        string owner = data[1];
        string humanCount = data[2];
        string botCount = data[3];
        string minXp = data[4];
        string minChips = data[5];

        GameTableInfo table = new GameTableInfo(name, owner, humanCount, botCount, minXp, minChips);
        MyGameManager.Instance.AddTableToListed(table);
    }


    public void ChangePlayerInfo()
    {
        if (MyGameManager.Instance.MainPlayer == null)
            return;

        this.InfoPlayerNick.text = MyGameManager.Instance.MainPlayer.Nick;
        this.InfoPlayerChips.text = Convert.ToString(MyGameManager.Instance.MainPlayer.TokensCount) + " $";
        this.InfoPlayerXP.text = Convert.ToString(MyGameManager.Instance.MainPlayer.Xp) + " XP";
    }

    public void OnJoinTableButton()
    {
        // TODO (cz. PGGP-69) zrobi� tak, �eby klient co jaki� czas (np. 10s)
        // automatycznie wysy�a� pro�b� o aktualizacj� stolik�w,
        // a ten przycisk tylko przenosi� do kolejnego ekranu
        // (bo czekanie na odpowied� serwera zamula dzia�anie GUI)
        loadTables();
        SceneManager.LoadScene("JoinTable");
    }
    public void OnCreateTableButton()
    {
        SceneManager.LoadScene("CreateTable");
    }

    // TODO (cz. PGGP-107) ekran Get Chips, przenie�� t� metod� tam i poprawi�, �eby nie pobiera�o zahardkodowanej warto�ci �eton�w
    public void OnGetChipsButton()
    {
        TcpConnection mainServer = MyGameManager.Instance.mainServerConnection;
        NetworkHelper.WriteNetworkStream(mainServer.stream, MyGameManager.Instance.clientToken + ' ' + "7" +' '+ "1000");
        mainServer.stream.Flush();
        Thread.Sleep(1000);
        if (mainServer.stream.DataAvailable)
        {
            string response = NetworkHelper.ReadNetworkStream(mainServer.stream);
            mainServer.stream.Flush();
            string[] splitted = response.Split(' ');
            // arg 0 - bool czy si� uda�o zmieni� coins gracza na serwerze
            if(splitted[0] == "1")
            {
                // arg 1 - aktualna warto�� coins gracza, je�li si� uda�o
                int coins = Convert.ToInt32(splitted[1]);
                MyGameManager.Instance.MainPlayer.TokensCount = coins;
                this.ChangePlayerInfo();
            }
        }
        Debug.Log("Get Chips");
    }

    // TODO (cz. PGGP-107) ekran Settings od zmieniania ustawie� stolika/rozgrywki
    public void OnChangeSettingsButton()
    {
        // TODO doda� nie wpuszczanie do tego ekranu, je�li nie siedzimy przy �adnym stoliku i odpowiedni Popup z info
        SceneManager.LoadScene("TableSettings");
    }

    public void OnAccountSettingsButton()
    {
        SceneManager.LoadScene("SettingsMenu");
    }
}
