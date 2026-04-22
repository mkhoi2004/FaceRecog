using System.Threading.Tasks;

namespace FaceIDApp.Data
{
    /// <summary>
    /// Singleton entry point cho Database + Repository.
    /// Gọi InitializeAsync() 1 lần duy nhất khi khởi động ứng dụng.
    /// </summary>
    internal static class AppDatabase
    {
        private static DatabaseConfig _config;

        public static Repository Repository { get; private set; }

        public static DatabaseConfig Config => _config;

        public static async Task InitializeAsync()
        {
            _config = DatabaseConfig.LoadFromConfiguration();

            var bootstrapper = new DatabaseBootstrapper(_config);
            await bootstrapper.InitializeAsync();

            Repository = new Repository(_config);
        }
    }
}
