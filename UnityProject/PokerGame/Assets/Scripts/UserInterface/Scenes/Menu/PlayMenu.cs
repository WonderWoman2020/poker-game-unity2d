using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Diagnostics;

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

    public void OnGetChipsButton()
    {
        SceneManager.LoadScene("GetTokensMenu");
    }

    public void OnAccountSettingsButton()
    {
        SceneManager.LoadScene("SettingsMenu");
    }
}
