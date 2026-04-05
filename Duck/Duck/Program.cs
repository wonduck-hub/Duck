using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Text;

class Server
{
    static Queue<byte[]> frameQueue = new Queue<byte[]>();
    static object queueLock = new object();
    static bool running = true;

    static void Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("Server started... Waiting for client.");

        TcpClient client = listener.AcceptTcpClient();
        Console.WriteLine("Client connected!");

        NetworkStream stream = client.GetStream();

        // 수신 스레드
        Thread receiveThread = new Thread(() => ReceiveLoop(stream));
        receiveThread.Start();

        // 디코딩 스레드
        Thread decodeThread = new Thread(() => DecodeLoop());
        decodeThread.Start();

        Console.WriteLine("Press Enter to stop server...");
        Console.ReadLine();
        running = false;

        client.Close();
        listener.Stop();
    }

    static void ReceiveLoop(NetworkStream stream)
    {
        byte[] buffer = new byte[4096];
        while (running)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // 연결 종료

                byte[] data = new byte[bytesRead];
                Array.Copy(buffer, data, bytesRead);

                lock (queueLock)
                {
                    frameQueue.Enqueue(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Receive Error: " + ex.Message);
                break;
            }
        }
    }

    static void DecodeLoop()
    {
        StringBuilder frameCountStringBuilder = new StringBuilder(4096);
        using StreamWriter frameCountStreamWriter = new StreamWriter(
            new FileStream(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/server_frame_counts.csv",
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read));

        Stopwatch sw = Stopwatch.StartNew();
        int packetCount = 0;
        int expectedSequence = 1; // 송신 측에서 1부터 시작한다고 가정

        while (running)
        {
            byte[] frame = null;

            lock (queueLock)
            {
                if (frameQueue.Count > 0)
                {
                    frame = frameQueue.Dequeue();
                }
            }

            if (frame != null)
            {
                try
                {
                    string msg;
                    int frameCount = SimpleProtocol.ParseFrame(frame, out msg);

                    if (frameCount > 0)
                    {
                        // 메시지에 시퀀스 번호가 포함되어 있다고 가정
                        // 예: "SEQ:123|Hello Server!"
                        if (msg.StartsWith("SEQ:"))
                        {
                            int sepIndex = msg.IndexOf('|');
                            if (sepIndex > 0)
                            {
                                string seqStr = msg.Substring(4, sepIndex - 4);
                                if (int.TryParse(seqStr, out int seq))
                                {
                                    if (seq != expectedSequence)
                                    {
                                        Console.WriteLine($"누락 발생! 기대한 {expectedSequence}, 받은 {seq}");
                                        expectedSequence = seq; // 누락된 만큼 건너뛰기
                                    }
                                    expectedSequence++;
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Parsing failed!");
                    }

                    ++packetCount;
                    frameCountStringBuilder.AppendLine(packetCount.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Decode Error: " + ex.Message);
                }
            }
            else
            {
                Thread.Sleep(1); // CPU 점유 줄이기
            }

            if (sw.Elapsed.TotalMilliseconds >= 1000)
            {
                Console.WriteLine($"Decoded {packetCount} packets in the last second.");
                packetCount = 0;
                frameCountStreamWriter.Write(frameCountStringBuilder.ToString());
                frameCountStreamWriter.Flush();
                frameCountStringBuilder.Clear();
                sw.Restart();
            }
        }
    }
}