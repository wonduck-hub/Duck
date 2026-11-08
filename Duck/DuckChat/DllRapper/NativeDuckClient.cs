using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DuckChatForm.DllRapper
{
    internal class NativeDuckClient : IDisposable
    {
        [DllImport("DuckChatCommon.dll",
            EntryPoint = "DuckClientConnect",
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        private static extern IntPtr DuckClientConnect(
            [MarshalAs(UnmanagedType.LPStr)] string address,
            [MarshalAs(UnmanagedType.LPStr)] string port
            );

        [DllImport("DuckChatCommon.dll",
            EntryPoint = "DuckClientDisconnect",
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        private static extern void DuckClientDisconnect(IntPtr client);

        [DllImport("DuckChatCommon.dll",
            EntryPoint = "DuckClientSendChat",
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        private static extern bool DuckClientSendChat(
            IntPtr client,
            byte[] chat,
            uint chatSize);

        [DllImport("DuckChatCommon.dll",
            EntryPoint = "DuckClientTryRecv",
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        private static extern bool DuckClientTryRecv(
            IntPtr client,
            byte[] outChatBuffer,
            uint bufferCapacity,
            ref uint outChatLength);

        public NativeDuckClient(string address, string port)
        {
            Address = address;
            Port = port;
            Handle = DuckClientConnect(address, port);
        }

        public void Dispose()
        {
            DuckClientDisconnect(Handle);

            Handle = IntPtr.Zero;
            Address = string.Empty;
            Port = string.Empty;
        }

        public bool SendChat(string chat)
        {
            Debug.Assert(Handle != IntPtr.Zero);

            byte[] buffer = stringToByteArray(chat);

            bool result = DuckClientSendChat(Handle, buffer, (uint)buffer.Length);

            return result;
        }

        public bool TryRecvChat(ref byte[] outBuffer, ref uint outChatSize)
        {
            outBuffer = new byte[1024];
            return DuckClientTryRecv(Handle, outBuffer, (uint)outBuffer.Length, ref outChatSize);
        }

        private byte[] stringToByteArray(string str)
        {
            byte[] result = new byte[str.Length];

            for (int index = 0; index < str.Length; ++index)
            {
                result[index] = (byte)str[index];
            }

            Debug.Assert(result.Length == str.Length);

            return result;
        }

        public IntPtr Handle { get; private set; }
        public string Address { get; private set; }
        public string Port { get; private set; }
    }
}
