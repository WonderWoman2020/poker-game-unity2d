using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

public class CreateTableMenu : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button backToMenuButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnCreateButton()
    {
        HumanPlayer p = (HumanPlayer)MyGameManager.Instance.MainPlayer;
        if(p == null)
        {
            Debug.Log("Table not created. Player was null");
            return;
        }
        GameTable gameTable = p.CreateYourTable("Test Table from HumanPlayer", null);
        MyGameManager.Instance.AddTableToGame(gameTable);
        Debug.Log("Player "+p.Nick+ " created table "+gameTable);
        //SceneManager.LoadScene("Table");
        SceneManager.LoadScene("PlayMenu");
    }
    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }
}
