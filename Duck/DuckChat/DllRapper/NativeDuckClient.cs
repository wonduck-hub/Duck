using System;
using System.Runtime.InteropServices;

namespace DuckChat.DllRapper
{
    internal class NativeDuckClient
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
    }
}
