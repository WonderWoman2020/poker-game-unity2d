using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PokerGameClasses;
using System.Net.NetworkInformation;

// Klasa trzymaj�ca zmienne �yj�ce ca�� gr� i dost�pne w ka�dym ekranie
public class MyGameManager : MonoBehaviour
{
    // Dzia�a wed�ug wzorca singleton
    public static MyGameManager Instance
    { get; set; }

    // Zmienne trzymaj�ce dane, kt�re maj� by� dost�pne we wszystkich ekranach
    public PlayerState MainPlayer
    { get; set; }
    public List<GameTableInfo> GameTableList
    { get; set; }

    // Klienci tcp do ��czenia si� z serwerem na porcie od obs�ugi menu (6937)
    // i od obs�ugi zdarze� z gry (6938)
    public TcpConnection mainServerConnection;
    public TcpConnection gameServerConnection;

    // Token gracza, dost�pny po zalogowaniu
    public string clientToken;

    public string ServerIP
    { get; set; }

    void Awake()
    {
        // Wzorzec singleton (ma zawsze istnie� tylko 1 instancja tej klasy)
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        this.MainPlayer = null;
        this.GameTableList = new List<GameTableInfo>();

        mainServerConnection = new TcpConnection();
        mainServerConnection.port = 6937;

        gameServerConnection = new TcpConnection();
        gameServerConnection.port = 6938;

        this.ServerIP = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool AddPlayerToGame(PlayerState p)
    {
        this.MainPlayer = p;
        return true;
    }

    public bool AddTableToListed(GameTableInfo gt)
    {
        this.GameTableList.Add(gt);
        return true;
    }
}
