using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PokerGameClasses;
using System.Net.NetworkInformation;

public class MyGameManager : MonoBehaviour
{
    public static MyGameManager Instance
    { get; set; }

    public Player MainPlayer
    { get; set; }

    public TcpConnection mainServerConnection;
    public TcpConnection gameServerConnection;

    public List<Player> HotSeatPlayers
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
        this.HotSeatPlayers = new List<Player>();

        mainServerConnection = new TcpConnection();
        mainServerConnection.port = 6937;
        mainServerConnection.Start();

        gameServerConnection = new TcpConnection();
        gameServerConnection.port = 6938;
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

    private bool CreateHotSeatPlayers()
    {
        this.HotSeatPlayers.Add(new Player("Gamer#1", PlayerType.Human,0,1000));
        this.HotSeatPlayers.Add(new Player("PokerLover123", PlayerType.Human, 0, 1000));
        this.HotSeatPlayers.Add(new Player("Joker", PlayerType.Human, 0, 1000));
        this.HotSeatPlayers.Add(new Player("I'm Rich", PlayerType.Human, 0, 1000));
        this.HotSeatPlayers.Add(new Player("Card Games Enjoyer", PlayerType.Human, 0, 1000));
        return true;
    }
}
