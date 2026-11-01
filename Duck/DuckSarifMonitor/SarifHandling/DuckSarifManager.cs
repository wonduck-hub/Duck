namespace DuckSarifMonitor.SarifHandling
{
    internal static class DuckSarifManager
    {
        public static List<DuckSarifLog> Cache { get; private set; } = new List<DuckSarifLog>();

        public static void AddSarifLog(DuckSarifLog log)
        {
            Cache.Add(log);
        }
        public static bool AddSarifLogByPath(FileInfo sarifFile)
        {
            DuckSarifLog temp;

            try
            {
                temp = DuckSarifLog.Load(sarifFile, ESarifProjectType.NONE);
            }
            catch
            {
                return false;
            }

            Cache.Add(temp);

            return true;
        }
    }
}
