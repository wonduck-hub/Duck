using Microsoft.CodeAnalysis.Sarif;

namespace DuckSarifMonitor.SarifHandling
{
    public enum ESarifProjectID
    {
        DUCK_CHAT,
        NONE
    }
    internal class DuckSarifLog
    {
        public SarifLog Data { get; private set; }
        public ESarifProjectID Project { get; private set; }

        public DuckSarifLog(SarifLog data, ESarifProjectID sarifType)
        {
            Data = data;
            Project = sarifType;
        }

        public DuckSarifLog(FileInfo sarifFile, ESarifProjectID sarifType)
        {
            Data = SarifLog.Load(sarifFile.FullName);
            Project = sarifType;
        }

        public DuckSarifLog(string sarifFilePath, ESarifProjectID sarifType)
        {
            Data = SarifLog.Load(sarifFilePath);
            Project = sarifType;
        }

        public static DuckSarifLog Load(FileInfo sarifFile, ESarifProjectID sarifType)
        {
            DuckSarifLog result = new DuckSarifLog(SarifLog.Load(sarifFile.FullName), sarifType);
            return result;
        }
    }
}
