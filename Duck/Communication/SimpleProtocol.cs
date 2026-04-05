using System;
using System.Text;

public class SimpleProtocol
{
    private const byte HEADER = 0xAA;
    private const byte TAIL = 0x55;
    private static ushort mFrameCounter = 0; // 패킷 횟수 관리
    public static ushort FrameCounter { get { return mFrameCounter; } }

    public static byte[] BuildFrame(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        // 패킷 보낼 때마다 증가
        mFrameCounter++;

        byte checksum = 0;
        foreach (byte b in data)
            checksum += b;

        // 프레임 구성: Header + FrameCount(2) + Data + Checksum + Tail
        byte[] frame = new byte[1 + 2 + data.Length + 1 + 1];
        int index = 0;

        frame[index++] = HEADER;
        frame[index++] = (byte)(mFrameCounter >> 8);   // FrameCount 상위 바이트
        frame[index++] = (byte)(mFrameCounter & 0xFF); // FrameCount 하위 바이트
        Array.Copy(data, 0, frame, index, data.Length);
        index += data.Length;
        frame[index++] = checksum;
        frame[index++] = TAIL;

        return frame;
    }

    public static int ParseFrame(byte[] frame, out string message)
    {
        message = string.Empty;
        if (frame[0] != HEADER || frame[frame.Length - 1] != TAIL)
            return -1;

        int frameCount = (int)((frame[1] << 8) | frame[2]);
        int dataLength = frame.Length - 1 - 2 - 1 - 1; // 전체 - Header - FrameCount - Checksum - Tail

        byte[] data = new byte[dataLength];
        Array.Copy(frame, 3, data, 0, dataLength);

        byte checksum = frame[3 + dataLength];
        byte calcChecksum = 0;
        foreach (byte b in data)
            calcChecksum += b;

        if (checksum != calcChecksum)
            frameCount = -1;

        message = Encoding.UTF8.GetString(data);

        return frameCount;
    }
}