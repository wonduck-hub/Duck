using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace TcpClientTest
{
    public class Program
    {
        static void Main()
        {
            TcpClient client = new TcpClient("127.0.0.1", 5000);
            NetworkStream stream = client.GetStream();

            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            StringBuilder frameCountStringBuilder = new StringBuilder(4096);
            StreamWriter frameCountStreamWriter = new StreamWriter(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/client_frame_counts.csv", FileMode.Create, FileAccess.Write, FileShare.Read));

            uint count = 0;
            while (true)
            {
                // Hz 출력
                if (sw.Elapsed.TotalMilliseconds >= 1000)
                {
                    Console.WriteLine($"Hz: {count}");
                    count = 0;
                    frameCountStreamWriter.Write(frameCountStringBuilder.ToString());
                    frameCountStreamWriter.Flush();
                    frameCountStringBuilder.Clear();
                    sw.Restart();
                }

                // 첫 번째 메시지
                string message1 = "Hello Server!";
                byte[] frame1 = SimpleProtocol.BuildFrame(message1);
                stream.Write(frame1, 0, frame1.Length);

                ++count;
                frameCountStringBuilder.AppendLine(count.ToString());

                Delay800Hz();
            }

            client.Close();
        }

        // 800Hz 딜레이 (약 1.25ms)
        public static void Delay800Hz()
        {
            double targetMilliseconds = 1000.0 / 850.0; // 1.25ms
            Stopwatch sw = Stopwatch.StartNew();

            while (sw.Elapsed.TotalMilliseconds < targetMilliseconds)
            {
                // BusyWait: 아무 작업도 하지 않고 루프만 돎
            }
        }

    }
}
