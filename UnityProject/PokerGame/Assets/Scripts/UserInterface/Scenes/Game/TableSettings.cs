using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using UnityEngine.SceneManagement;

//using System.Net.Sockets;

using PokerGameClasses;
//using pGrServer;



public class TableSettings : MonoBehaviour
{

    [SerializeField] private Button changeSettingsButton;
    [SerializeField] private Button backButton;

    [SerializeField] private Button modeNoBotsButton;
    [SerializeField] private Button modeYouAndBotsButton;
    [SerializeField] private Button modeMixedButton;

    public GameObject PopupWindow;

    private string bigBlind;
    private string minTokens;
    private string maxPlayers;
    private string minPlayers;
    private string numberOfBots;
    private string maxMoveTime;

    private GameMode chosenMode;

    // TODO w stoliku jest jeszcze ustawienie min XP - mo¿na potem te¿ dodaæ do tego input

    // Start is called before the first frame update
    void Start()
    {
        this.bigBlind = null;
        this.minTokens = null;
        this.maxPlayers = null;
        this.minPlayers = null;
        this.numberOfBots = "0";
        this.maxMoveTime = null;

        this.chosenMode = GameMode.No_Bots;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChangeSettingsButton()
    {
        Debug.Log("Change");
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }

    public void ReadBigBlind(string bigBlind)
    {
        if (bigBlind.Length == 0)
        {
            this.bigBlind = null;
            return;
        }

        this.bigBlind = bigBlind;
        Debug.Log(this.bigBlind);
    }

    public void ReadMinTokens(string minTokens)
    {
        if (minTokens.Length == 0)
        {
            this.minTokens = null;
            return;
        }

        this.minTokens = minTokens;
        Debug.Log(this.minTokens);
    }

    public void ReadMaxPlayers(string maxPlayers)
    {
        if (maxPlayers.Length == 0)
        {
            this.maxPlayers = null;
            return;
        }

        this.maxPlayers = maxPlayers;
        Debug.Log(this.maxPlayers);
    }

    public void ReadMinPlayers(string minPlayers)
    {
        if (minPlayers.Length == 0)
        {
            this.minPlayers = null;
            return;
        }

        this.minPlayers = minPlayers;
        Debug.Log(this.minPlayers);
    }

    public void ReadBotsNumber(string botsNumber)
    {
        if (botsNumber.Length == 0)
        {
            this.numberOfBots = null;
            return;
        }

        this.numberOfBots = botsNumber;
        Debug.Log(this.numberOfBots);
    }

    public void ReadMaxMoveTime(string maxMoveTime)
    {
        if (maxMoveTime.Length == 0)
        {
            this.maxMoveTime = null;
            return;
        }

        this.maxMoveTime = maxMoveTime;
        Debug.Log(this.maxMoveTime);
    }

    public void OnModeNoBotsButton()
    {
        this.chosenMode = GameMode.No_Bots;
        this.numberOfBots = "0";
        Debug.Log(this.chosenMode);
    }

    public void OnYouAndBotsButton()
    {
        this.chosenMode = GameMode.You_And_Bots;
        Debug.Log(this.chosenMode);
    }

    public void OnMixedButton()
    {
        this.chosenMode = GameMode.Mixed;
        Debug.Log(this.chosenMode);
    }

    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }
}
