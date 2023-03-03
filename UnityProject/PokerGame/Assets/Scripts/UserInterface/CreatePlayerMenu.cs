using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

using TMPro;
using System;

public class CreatePlayerMenu : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button backToMenuButton;

    public GameObject PopupWindow;

    private string playerNick;
    private string password1;
    private string password2;
    private string login;

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void OnCreateButton()
    {
        if(this.playerNick == null)
        {
            Debug.Log("You must set at least player's nick to create them.");
            if (PopupWindow)
            {
                ShowWrongInputPopup();
            }
            return;
        }

        Player player = new HumanPlayer("I'm main player", PlayerType.Human);
        this.SetPlayerInputData(player);
        MyGameManager.Instance.AddPlayerToGame(player);
        Debug.Log("Created player "+player.Nick);
        //SceneManager.LoadScene("Table");
        SceneManager.LoadScene("PlayMenu");
    }

    void ShowWrongInputPopup()
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = "You must set at least player's nick to create them.";
    }

    public void ReadPlayerNick(string nick)
    {
        if (nick.Length == 0)
        {
            this.playerNick = null;
            return;
        }

        this.playerNick = nick;
        Debug.Log(this.playerNick);
    }

    public void ReadLogin(string login)
    {
        if (login.Length == 0)
        {
            this.login = null;
            return;
        }

        this.login = login;
        Debug.Log(this.login);
    }
    public void ReadPassword(string password)
    {
        if (password.Length == 0)
        {
            this.password1 = null;
            return;
        }

        this.password1 = password;
        Debug.Log(this.password1);
    }

    private bool SetPlayerInputData(Player player)
    {
        //data from input
        if (this.playerNick != null)
            player.ChangeNick(this.playerNick);

      //  if (this.chips != null)
        //    player.TokensCount = Convert.ToInt32(this.chips);

      //  if (this.xp != null)
        //    player.XP = Convert.ToInt32(this.xp);

        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.playerNick = null;
        this.password1 = null;
        this.password2 = null;
        this.login = null;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (this.playerNick != null && this.password1 != null && this.password2 != null && this.login != null)
                this.OnCreateButton();
        }
    }
}
