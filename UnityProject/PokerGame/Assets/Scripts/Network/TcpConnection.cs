using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

// Klasa-wrapper dla klasy TcpClient
public class TcpConnection
{
    public TcpClient client;
    public NetworkStream stream;

    // TODO doda� konstruktor przyjmuj�cy port jako parametr
    public int port;

    public void Start()
    {
        client = new TcpClient();
        //client.Connect("127.0.0.1", port);
        client.Connect(MyGameManager.Instance.ServerIP, port);
        stream = client.GetStream();
    }

    public void Close()
    {
        client.Close();
        stream.Dispose();
    }
}
