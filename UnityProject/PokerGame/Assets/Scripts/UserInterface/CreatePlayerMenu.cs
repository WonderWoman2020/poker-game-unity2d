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
        Debug.Log("Created player "+player.Nick);
        //SceneManager.LoadScene("Table");
        SceneManager.LoadScene("PlayMenu");
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
