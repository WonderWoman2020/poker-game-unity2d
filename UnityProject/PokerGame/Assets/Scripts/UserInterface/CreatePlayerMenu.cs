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
    private string chips;
    private string xp;

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

    public void ReadChips(string chips)
    {
        if (chips.Length == 0)
        {
            this.chips = null;
            return;
        }

        this.chips = chips;
        Debug.Log(this.chips);
    }

    public void ReadXP(string xp)
    {
        if (xp.Length == 0)
        {
            this.xp = null;
            return;
        }

        this.xp = xp;
        Debug.Log(this.xp);
    }

    private bool SetPlayerInputData(Player player)
    {
        //data from input
        if (this.playerNick != null)
            player.ChangeNick(this.playerNick);

        if (this.chips != null)
            player.TokensCount = Convert.ToInt32(this.chips);

        if (this.xp != null)
            player.XP = Convert.ToInt32(this.xp);

        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.playerNick = null;
        this.chips = null;
        this.xp = null;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (this.playerNick != null && this.chips != null && this.xp != null)
                this.OnCreateButton();
        }
    }
}
