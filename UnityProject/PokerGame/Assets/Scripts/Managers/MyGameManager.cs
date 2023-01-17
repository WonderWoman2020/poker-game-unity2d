using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PokerGameClasses;

public class MyGameManager : MonoBehaviour
{
    public static MyGameManager Instance
    { get; set; }

    public Player MainPlayer
    { get; set; }

    public List<GameTable> GameTables
    { get; set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        //DontDestroyOnLoad(Instance.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        this.MainPlayer = null;
        //this.MainPlayer = new HumanPlayer("Test main player", PlayerType.Human);
        this.GameTables = new List<GameTable>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool AddPlayerToGame(Player p)
    {
        this.MainPlayer = p;
        return true;
    }

    public bool AddTableToGame(GameTable gt)
    {
        this.GameTables.Add(gt);
        return true;
    }
}
