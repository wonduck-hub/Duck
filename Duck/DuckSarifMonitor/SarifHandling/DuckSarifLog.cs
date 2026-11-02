using Microsoft.CodeAnalysis.Sarif;

namespace DuckSarifMonitor.SarifHandling
{
    public enum ESarifProjectType
    {
        DUCK_CAHT,
        NONE
    }
    internal class DuckSarifLog
    {
        public SarifLog Data { get; private set; }
        public ESarifProjectType Project { get; private set; }

        public DuckSarifLog(SarifLog data, ESarifProjectType sarifType)
        {
            Data = data;
            Project = sarifType;
        }

        public DuckSarifLog(FileInfo sarifFile, ESarifProjectType sarifType)
        {
            Data = SarifLog.Load(sarifFile.FullName);
            Project = sarifType;
        }

        public DuckSarifLog(string sarifFilePath, ESarifProjectType sarifType)
        {
            Data = SarifLog.Load(sarifFilePath);
            Project = sarifType;
        }

        public static DuckSarifLog Load(FileInfo sarifFile, ESarifProjectType sarifType)
        {
            DuckSarifLog result = new DuckSarifLog(SarifLog.Load(sarifFile.FullName), sarifType);
            return result;
        }
    }
}
