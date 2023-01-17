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
        HumanPlayer p = new HumanPlayer("Test player", PlayerType.Human);
        GameTable gameTable = new GameTable("Test table",p);
        MyGameManager.Instance.AddTableToGame(gameTable);
        Debug.Log("Created table "+gameTable);
        //SceneManager.LoadScene("Table");
        SceneManager.LoadScene("PlayMenu");
    }
    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }
}
