using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

namespace DLogger
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
    public class BasicLogger : IDisposable
    {
        public bool WriteToFile { get; set; } = true;
        public bool WriteToDebug { get; set; } = true;

        private readonly DirectoryInfo? mLogsDirectoryInfoOrNull;
        private readonly StreamWriter? mLogWriterOrNull;
        private readonly object mLoggerLock = new object();

        public BasicLogger(DirectoryInfo? directoryInfoOrNull = null, bool writeToFile = true, bool writeToDebug = true)
        {
            mLogsDirectoryInfoOrNull = directoryInfoOrNull;
            WriteToFile = mLogsDirectoryInfoOrNull != null && writeToFile;
            WriteToDebug = writeToDebug;

            if (WriteToFile)
            {
                try
                {
                    mLogsDirectoryInfoOrNull!.Create();

                    string fileName =
                        $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.log";

                    string filePath = Path.Combine(
                        mLogsDirectoryInfoOrNull.FullName,
                        fileName);

                    mLogWriterOrNull = new StreamWriter(
                        filePath,
                        append: true,
                        encoding: new UTF8Encoding(
                            encoderShouldEmitUTF8Identifier: false))
                    {
                        AutoFlush = true
                    };
                }
                catch (Exception ex)
                {
                    WriteToFile = false;
                    Debug.WriteLine($"Failed to create log file: {ex}");
                }
            }
        }

        public void Info(string message)
        {
            Write(LogLevel.Info, message);
        }

        public void Warning(string message)
        {
            Write(LogLevel.Warning, message);
        }

        public void Error(string message, Exception exception)
        {
            Write(LogLevel.Error, message, exception);
        }

        private void Write(
            LogLevel level,
            string message,
            Exception? exception = null)
        {
            string formattedMessage = Format(level, message, exception!);

            if (WriteToFile)
                WriteFile(formattedMessage);

            if (WriteToDebug)
                Debug.WriteLine(formattedMessage);
        }

        private string Format(
            LogLevel level,
            string message,
            Exception? exception = null)
        {
            string result =
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} " +
                $"[{level}] {message}";

            if (exception != null)
                result += Environment.NewLine + exception;

            return result;
        }

        private void WriteFile  (string message)
        {
            Debug.Assert(WriteToFile && mLogWriterOrNull != null, "WriteToFile is true but mLogWriterOrNull is null.");
            if (WriteToFile && mLogWriterOrNull != null)
            {
                lock (mLoggerLock)
                {
                    mLogWriterOrNull.WriteLine(message);
                }
            }
        }

        public void Dispose()
        {
            lock (mLoggerLock)
            {
                mLogWriterOrNull?.Dispose();
            }
        }
    }
}


/*
 * 추후 해당 로거를 수정하거나 새로 만들 때 ILogger를 사용하고 DI를 적용하는 구조를 갖는다면 특정 서비스에 대한 로그만 따로 확인할 구분할 수도 있을 것이다. 
 * 하지만 현재는 단순히 로그를 남기기 위한 용도로만 사용되므로 ILogger를 구현하지 않고 단순한 BasicLogger 클래스로 구현하였다.
 */