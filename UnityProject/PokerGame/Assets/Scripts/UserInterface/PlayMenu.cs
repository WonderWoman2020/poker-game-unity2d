using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

using TMPro;
using System;

public class PlayMenu : MonoBehaviour
{
    [SerializeField] private Button joinTableButton;
    [SerializeField] private Button createTableButton;
    [SerializeField] private Button getChipsButton;
    [SerializeField] private Button changeSettingsButton;

    [SerializeField] private TMP_Text InfoPlayerNick;
    [SerializeField] private TMP_Text InfoPlayerChips;
    [SerializeField] private TMP_Text InfoPlayerXP;

    // Start is called before the first frame update
    void Start()
    {
        if (MyGameManager.Instance.MainPlayer == null)
            return;

        this.InfoPlayerNick.text = MyGameManager.Instance.MainPlayer.Nick;
        this.InfoPlayerChips.text = Convert.ToString(MyGameManager.Instance.MainPlayer.TokensCount)+" $";
        this.InfoPlayerXP.text = Convert.ToString(MyGameManager.Instance.MainPlayer.XP)+ " XP";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnJoinTableButton()
    {
        SceneManager.LoadScene("JoinTable");
    }
    public void OnCreateTableButton()
    {
        SceneManager.LoadScene("CreateTableMenu");
    }
    public void OnGetChipsButton()
    {
        Debug.Log("Get Chips");
    }
    public void OnChangeSettingsButton()
    {
        Debug.Log("Settings");
    }
}
