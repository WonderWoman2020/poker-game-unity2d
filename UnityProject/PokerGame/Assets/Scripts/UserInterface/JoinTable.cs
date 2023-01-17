using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using PokerGameClasses;

public class JoinTable : MonoBehaviour
{
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backToMenuButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnJoinButton()
    {
        if (MyGameManager.Instance.GameTables.Count > 0)
        {
            GameTable gameTable = MyGameManager.Instance.GameTables[0];
            Player player = MyGameManager.Instance.MainPlayer;
            gameTable.AddPlayer(player);
            Debug.Log("Added player " + player.Nick + " to "+gameTable.Name);
        }
        SceneManager.LoadScene("Table");
    }
    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }
}
