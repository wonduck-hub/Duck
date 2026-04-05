using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Communication
{
    public class TcpServer
    {
        private TcpListener listener;

        public TcpServer(string ip, int port)
        {
            listener = new TcpListener(IPAddress.Parse(ip), port);
        }

        public void Start()
        {
            listener.Start();
            Console.WriteLine("Server started...");
        }

        public void AcceptClient()
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected!");

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received: " + received);

            string response = "Hello Client!";
            byte[] data = Encoding.UTF8.GetBytes(response);
            stream.Write(data, 0, data.Length);

            client.Close();
        }

        public void Stop()
        {
            listener.Stop();
        }
    }
}
