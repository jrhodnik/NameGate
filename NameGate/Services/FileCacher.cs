namespace NameGate.Services
{
    public abstract class BaseFileCacher
    {
        private readonly string _filePath;
        private readonly TimeSpan _expiry;

        public BaseFileCacher(string filePath, TimeSpan expiry)
        {
            _filePath = filePath;
            _expiry = expiry;
        }

        public bool RequiresRefresh()
        {
            var fi = GetFileInfo();
            return !fi.Exists || fi.LastWriteTimeUtc.Add(_expiry) < DateTime.UtcNow;
        }

        public async Task<bool> Refresh(bool force = false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            if (RequiresRefresh() || force)
            {
                var fi = GetFileInfo();
                if (fi.Exists)
                {
                    fi.Delete();
                    fi.Refresh();
                }

                using var s = await Loader();
                using var newFileStream = fi.Open(FileMode.CreateNew);
                await s.CopyToAsync(newFileStream);
                fi.Refresh();
                return true;
            }

            return false;
        }

        public Task<Stream> GetStream() => Task.FromResult((Stream)GetFileInfo().OpenRead());

        protected abstract Task<Stream> Loader();

        private FileInfo GetFileInfo() => new FileInfo(_filePath);
    }

    public class FileCacher : BaseFileCacher
    {
        private readonly Func<Task<Stream>> _loaderFunc;

        public FileCacher(string filePath, TimeSpan expiry, Func<Task<Stream>> loaderFunc) : base(filePath, expiry)
        {
            _loaderFunc = loaderFunc;
        }

        protected override Task<Stream> Loader() => _loaderFunc();
    }

}
