using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

using TMPro;
using System;

public class Table : MonoBehaviour
{
    [SerializeField] private Button checkButton;
    [SerializeField] private Button allInButton;
    [SerializeField] private Button passButton;
    [SerializeField] private Button bidButton;


    [SerializeField] private TMP_Text InfoMainPlayerName;
    [SerializeField] private TMP_Text InfoMainPlayerChips;
    [SerializeField] private TMP_Text InfoMainPlayerBid;

    private GameObject[] Players;
    private Component[] Components
    { get; set; }
    private string betFieldText;
    // Start is called before the first frame update
    void Start()
    {
        if (MyGameManager.Instance.MainPlayer == null)
            return;

        this.InfoMainPlayerName.text = MyGameManager.Instance.MainPlayer.Nick;
        this.InfoMainPlayerChips.text = Convert.ToString(MyGameManager.Instance.MainPlayer.TokensCount) + " $";
        this.InfoMainPlayerBid.text = "Bet\n"+Convert.ToString(0) + " $";

        this.Players = GameObject.FindGameObjectsWithTag("Player");
        this.Components = Players[0].GetComponents(typeof(Component));
        foreach (Component component in Components)
        {
            Debug.Log(component.ToString());
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ReadInputBet(string inputBet)
    {
        if (inputBet.Length == 0)
        {
            this.betFieldText = null;
            return;
        }
        this.betFieldText = inputBet;
        Debug.Log(this.betFieldText);
    }
    public void OnCheckButton()
    {
        Debug.Log("Check");
        MyGameManager.Instance.MainPlayer.Check();
    }
    public void OnAllInButton()
    {
        Debug.Log("All in");
        MyGameManager.Instance.MainPlayer.AllIn();
    }
    public void OnPassButton()
    {
        Debug.Log("Pass");
        MyGameManager.Instance.MainPlayer.Fold();
    }
    public void OnBidButton()
    {
        Debug.Log("Bid");
        MyGameManager.Instance.MainPlayer.Raise(Convert.ToInt32(this.betFieldText));
    }
}
