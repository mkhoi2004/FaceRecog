using System.Threading.Tasks;

namespace FaceRecognitionDotNet.WinForms.Data
{
    internal static class AppDatabase
    {
        private static PostgresOptions _options;

        public static PostgresRepository Repository { get; private set; }

        public static async Task InitializeAsync()
        {
            _options = PostgresOptions.LoadFromConfiguration();

            var bootstrapper = new PostgresBootstrapper(_options);
            await bootstrapper.InitializeAsync().ConfigureAwait(false);

            Repository = new PostgresRepository(_options);
        }
    }
}
