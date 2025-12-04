using BudgetWPFKevin.Data;
using BudgetWPFKevin.Helpers;
using BudgetWPFKevin.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Threading;
using System.Windows;
namespace BudgetWPFKevin
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        private const string ConnectionString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BudgetDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30";

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // En rad - allt är registrerat! 🎉
            services.AddAllBudgetDependencies(ConnectionString);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ThemeManager.Instance.ApplyTheme();


            var culture = new CultureInfo("sv-SE");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Skapa DB om den saknas
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BudgetContext>();
                context.Database.EnsureCreated();
            }

            // Skapa scope för MainViewModel
            var scope2 = _serviceProvider.CreateScope();
            var mainViewModel = scope2.ServiceProvider.GetRequiredService<MainViewModel>();

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.Show();
            await mainViewModel.LoadAsync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ThemeManager.Instance.Dispose();

            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}