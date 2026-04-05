using System;
using System.Net.Sockets;
using System.Text;

public class DuckTcpClient
{
    private TcpClient client;

    public DuckTcpClient(string ip, int port)
    {
        client = new TcpClient(ip, port);
    }

    public void Send(string message)
    {
        NetworkStream stream = client.GetStream();
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);

        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine("Received: " + response);

        client.Close();
    }
}