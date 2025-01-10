using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Windows;
using HollingerBox.Data;

namespace HollingerBox
{
    public partial class App : Application
    {
        // We'll keep a static reference to the ServiceProvider so we can use it throughout the app
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Build configuration
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // 2. Setup DI
            var services = new ServiceCollection();

            // 3. Add DbContext with the SQLite connection string from appsettings
            services.AddDbContext<EthicsContext>(options =>
            {
                var connString = config.GetConnectionString("SqliteDb");
                options.UseSqlite(connString);
            });

            // 4. Register other services or ViewModels as needed
            // services.AddTransient<MainWindowViewModel>();

            // 5. Build the service provider
            ServiceProvider = services.BuildServiceProvider();

            // Optionally, if you want to apply migrations on startup:
            using (var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EthicsContext>();
                // This ensures the SQLite DB is created and migrations are applied if not up to date.
               // db.Database.Migrate();
            }

            // 6. Show the MainWindow (RESOLVED from DI if needed)
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
