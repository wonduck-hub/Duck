using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DuckChat.DllRapper
{
    internal class NativeDuckClient : IDisposable
    {
        [DllImport("DuckChatServer.dll",
            EntryPoint = "DuckClientConnect",
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        private static extern IntPtr DuckClientConnect(
            [MarshalAs(UnmanagedType.LPStr)] string address,
            [MarshalAs(UnmanagedType.LPStr)] string port
            );

        [DllImport("DuckChatServer.dll",
            EntryPoint = "DuckClientDisconnect",
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        private static extern void DuckClientDisconnect(IntPtr client);

        [DllImport("DuckChatServer.dll",
            EntryPoint = "DuckClientSendChat",
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        private static extern bool DuckClientSendChat(
            IntPtr client,
            byte[] chat,
            uint chatSize);

        [DllImport("DuckChatServer.dll",
            EntryPoint = "DuckClientTryRecv",
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        private static extern bool DuckClientTryRecv(
            IntPtr client,
            byte[] outChatBuffer,
            uint bufferCapacity,
            uint outChatLength);

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

        public bool SendChat(byte[] buffer)
        {
            Debug.Assert(Handle != IntPtr.Zero);

            bool result = DuckClientSendChat(Handle, buffer, (uint)buffer.Length);

            return result;
        }

        public bool TryRecvChat(ref byte[] outBuffer, ref uint outChatSize)
        {
            outBuffer = new byte[1024];
            return DuckClientTryRecv(Handle, outBuffer, (uint)outBuffer.Length, outChatSize);
        }

        public IntPtr Handle { get; private set; }
        public string Address { get; private set; }
        public string Port { get; private set; }
    }
}
