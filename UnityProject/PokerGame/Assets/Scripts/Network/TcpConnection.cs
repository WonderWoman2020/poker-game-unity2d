using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TcpConnection
{
    public TcpClient client;
    public NetworkStream stream;
    public byte[] buffer = new byte[1024];

    public int port;

    public void Start()
    {
        client = new TcpClient();
        client.Connect("127.0.0.1", port);
        stream = client.GetStream();

        // Rozpoczêcie odczytu danych z serwera w nowym w¹tku
        //new System.Threading.Thread(ReceiveData).Start();
    }

    public void SendData(string data)
    {
        // Wysy³anie danych do serwera
        // ...
    }

    private void ReceiveData()
    {
        while (true)
        {
            // Odczytanie danych z serwera
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                // Konwersja odczytanych danych na string
                string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // Przetworzenie odebranych danych
                // ...
            }
        }
    }

    void OnDestroy()
    {
        client.Close();
    }
}
