using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

public class CreatePlayerMenu : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button backToMenuButton;

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void OnCreateButton()
    {
        Player player = new HumanPlayer("I'm main player", PlayerType.Human);
        MyGameManager.Instance.AddPlayerToGame(player);
        Debug.Log("Creating player");
        SceneManager.LoadScene("Table");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
