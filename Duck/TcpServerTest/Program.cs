using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    const int FrameSize = 18;
    const int QueueCapacity = 10000;

    static BlockingCollection<byte[]> frameQueue =
        new BlockingCollection<byte[]>(new ConcurrentQueue<byte[]>(), QueueCapacity);

    static volatile bool running = true;

    static void Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("Server started... Waiting for client.");

        TcpClient client = listener.AcceptTcpClient();
        Console.WriteLine("Client connected!");

        client.NoDelay = true;
        client.ReceiveBufferSize = 1024 * 1024;
        client.SendBufferSize = 1024 * 1024;

        NetworkStream stream = client.GetStream();

        Thread receiveThread = new Thread(() => ReceiveLoop(stream));
        Thread decodeThread = new Thread(DecodeLoop);

        receiveThread.Start();
        decodeThread.Start();

        Console.WriteLine("Press Enter to stop server...");
        Console.ReadLine();

        running = false;
        frameQueue.CompleteAdding();

        try { stream.Close(); } catch { }
        try { client.Close(); } catch { }
        try { listener.Stop(); } catch { }

        receiveThread.Join();
        decodeThread.Join();
    }

    static void ReceiveLoop(NetworkStream stream)
    {
        byte[] readBuffer = new byte[4096];
        byte[] assemblyBuffer = new byte[8192];
        int assemblyCount = 0;

        try
        {
            while (running)
            {
                int bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                if (bytesRead == 0)
                {
                    Console.WriteLine("Client disconnected.");
                    break;
                }

                EnsureCapacity(ref assemblyBuffer, assemblyCount + bytesRead);
                Buffer.BlockCopy(readBuffer, 0, assemblyBuffer, assemblyCount, bytesRead);
                assemblyCount += bytesRead;

                int offset = 0;

                while (assemblyCount - offset >= FrameSize)
                {
                    byte[] frame = new byte[FrameSize];
                    Buffer.BlockCopy(assemblyBuffer, offset, frame, 0, FrameSize);
                    offset += FrameSize;

                    // 큐가 꽉 차면 여기서 block
                    // => 프레임을 버리는 대신 송신 측 흐름을 늦춤
                    frameQueue.Add(frame);
                }

                int remaining = assemblyCount - offset;
                if (remaining > 0)
                {
                    Buffer.BlockCopy(assemblyBuffer, offset, assemblyBuffer, 0, remaining);
                }

                assemblyCount = remaining;
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine("Receive Error: " + ex.Message);
        }
        finally
        {
            frameQueue.CompleteAdding();
        }
    }

    static void DecodeLoop()
    {
        Stopwatch sw = Stopwatch.StartNew();
        int packetCount = 0;
        int expectedSequence = 1;
        long totalMissing = 0;
        long totalParsed = 0;
        long totalParseFail = 0;

        StringBuilder frameCountStringBuilder = new StringBuilder(4096);
        StreamWriter frameCountStreamWriter = new StreamWriter(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/server_frame_counts.csv", FileMode.Create, FileAccess.Write, FileShare.Read));

        foreach (byte[] frame in frameQueue.GetConsumingEnumerable())
        {
            try
            {
                string msg;
                int seq = SimpleProtocol.ParseFrame(frame, out msg);

                if (seq < 0)
                {
                    totalParseFail++;
                    Console.WriteLine("Parsing failed!");
                    continue;
                }

                if (seq != expectedSequence)
                {
                    if (seq > expectedSequence)
                    {
                        int missed = seq - expectedSequence;
                        totalMissing += missed;
                        Console.WriteLine($"누락 감지: 기대={expectedSequence}, 수신={seq}, 누락={missed}");
                    }
                    else
                    {
                        Console.WriteLine($"역순/중복/카운터 초기화 의심: 기대={expectedSequence}, 수신={seq}");
                    }

                    expectedSequence = seq;
                }

                frameCountStringBuilder.AppendLine(((ushort)seq).ToString());
                expectedSequence++;
                packetCount++;
                totalParsed++;

                // 필요하면 메시지 확인
                // Console.WriteLine($"SEQ={seq}, MSG={msg}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Decode Error: " + ex.Message);
            }

            if (sw.ElapsedMilliseconds >= 1000)
            {
                Console.WriteLine(
                    $"Decoded={packetCount}/s, Queue={frameQueue.Count}, TotalParsed={totalParsed}, ParseFail={totalParseFail}, Missing={totalMissing}");
                frameCountStreamWriter.Write(frameCountStringBuilder.ToString());
                frameCountStreamWriter.Flush();
                frameCountStringBuilder.Clear();
                packetCount = 0;
                sw.Restart();
            }
        }
    }

    static void EnsureCapacity(ref byte[] buffer, int required)
    {
        if (buffer.Length >= required)
            return;

        int newSize = buffer.Length;
        while (newSize < required)
            newSize *= 2;

        Array.Resize(ref buffer, newSize);
    }
}