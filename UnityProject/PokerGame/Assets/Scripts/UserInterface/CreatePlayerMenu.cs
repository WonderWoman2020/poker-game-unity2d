using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

using System;

public class CreatePlayerMenu : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button backToMenuButton;

    private string playerNick;
    private string chips;
    private string xp;

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void OnCreateButton()
    {
        Player player = new HumanPlayer("I'm main player", PlayerType.Human);
        this.SetPlayerInputData(player);
        MyGameManager.Instance.AddPlayerToGame(player);
        Debug.Log("Created player "+player.Nick);
        //SceneManager.LoadScene("Table");
        SceneManager.LoadScene("PlayMenu");
    }

    public void ReadPlayerNick(string nick)
    {
        this.playerNick = nick;
        Debug.Log(this.playerNick);
    }

    public void ReadChips(string chips)
    {
        this.chips = chips;
        Debug.Log(this.chips);
    }

    public void ReadXP(string xp)
    {
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
        
    }
}
