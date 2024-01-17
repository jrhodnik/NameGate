using System.Runtime.InteropServices;

namespace NameGate.Services
{
    public class DirectoryManager(GlobalConfig _config)
    {
        public string DataDirectory => _config.DataDirectory ?? GetPlatformDefaultDataDirectory();
        public string CacheDirectory => _config.CacheDirectory ?? Path.Combine(Path.GetTempPath(), "NameGate");

        public string MainDatabasePath => Path.Combine(DataDirectory, "main.db");

        private string GetPlatformDefaultDataDirectory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NameGate");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "/var/lib/NameGate";

            throw new NotImplementedException($"Unsupported platform.");
        }

        public string GetCacheFilePath(string name) => Path.Combine(CacheDirectory, name);
    }
}
