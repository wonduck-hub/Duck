using System.Runtime.InteropServices;
using System.Text;

namespace DuckChat.DllRapper
{
    internal sealed class NativeDuckClient : IDisposable
    {
        private const int RECEIVE_BUFFER_SIZE = 1024;
        private IntPtr mHandle;

        [DllImport("DuckChatCommon.dll", EntryPoint = "DuckClientConnect", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr duckClientConnect(
            [MarshalAs(UnmanagedType.LPStr)] string address,
            [MarshalAs(UnmanagedType.LPStr)] string port);

        [DllImport("DuckChatCommon.dll", EntryPoint = "DuckClientDisconnect", CallingConvention = CallingConvention.Cdecl)]
        private static extern void duckClientDisconnect(IntPtr client);

        [DllImport("DuckChatCommon.dll", EntryPoint = "DuckClientSendChat", CallingConvention = CallingConvention.Cdecl)]
        private static extern int duckClientSendChat(IntPtr client, byte[] chat, nuint chatSize);

        [DllImport("DuckChatCommon.dll", EntryPoint = "DuckClientTryRecv", CallingConvention = CallingConvention.Cdecl)]
        private static extern int duckClientTryRecv(
            IntPtr client,
            [Out] byte[] outChatBuffer,
            uint bufferCapacity,
            ref nuint outChatLength);

        public NativeDuckClient(string address, string port)
        {
            mHandle = duckClientConnect(address, port);
            if (mHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("서버에 연결할 수 없습니다: " + address + ":" + port);
            }
        }

        public void Dispose()
        {
            if (mHandle != IntPtr.Zero)
            {
                duckClientDisconnect(mHandle);
                mHandle = IntPtr.Zero;
            }
        }

        public bool SendChat(string chat)
        {
            throwIfDisposed();
            byte[] buffer = Encoding.UTF8.GetBytes(chat);
            if (buffer.Length > RECEIVE_BUFFER_SIZE)
            {
                throw new ArgumentException("메시지는 UTF-8 기준 1024바이트 이하여야 합니다.", nameof(chat));
            }

            return duckClientSendChat(mHandle, buffer, (nuint)buffer.Length) != 0;
        }

        public bool TryRecvChat(out byte[] outBuffer, out uint outChatSize)
        {
            throwIfDisposed();
            outBuffer = new byte[RECEIVE_BUFFER_SIZE];
            nuint chatLength = 0;
            bool bReceived = duckClientTryRecv(mHandle, outBuffer, (uint)outBuffer.Length, ref chatLength) != 0;
            if (chatLength > (nuint)outBuffer.Length)
            {
                throw new InvalidDataException("수신한 메시지가 버퍼 크기를 초과합니다.");
            }

            outChatSize = (uint)chatLength;
            return bReceived;
        }

        private void throwIfDisposed()
        {
            ObjectDisposedException.ThrowIf(mHandle == IntPtr.Zero, this);
        }
    }
}
