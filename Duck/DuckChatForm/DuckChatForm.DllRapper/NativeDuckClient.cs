using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace DuckChatForm.DllRapper
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
            Debug.Assert(mHandle != IntPtr.Zero);

            byte[] buffer = Encoding.UTF8.GetBytes(chat);
            //TODO: buffer에 채팅 저보 뿐만 아니라 자용자 정보, 전송 시간, 목표 채팅방 등을 포함시켜야 함

            Debug.Assert(buffer.Length <= RECEIVE_BUFFER_SIZE);

            return duckClientSendChat(mHandle, buffer, (nuint)buffer.Length) != 0;
        }

        public bool TryRecvChat(out byte[] outBuffer, out uint outChatSize)
        {
            Debug.Assert(mHandle != IntPtr.Zero);

            outBuffer = new byte[RECEIVE_BUFFER_SIZE];
            nuint chatLength = 0;
            bool bReceived = duckClientTryRecv(mHandle, outBuffer, (uint)outBuffer.Length, ref chatLength) != 0;

            Debug.Assert(chatLength <= (nuint)outBuffer.Length);

            outChatSize = (uint)chatLength;
            return bReceived;
        }
    }
}
