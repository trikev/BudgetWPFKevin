using BudgetWPFKevin.Data;
using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Data.Repository;
using BudgetWPFKevin.Mappers;
using BudgetWPFKevin.Services;
using BudgetWPFKevin.ViewModels;
using BudgetWPFKevin.ViewModels.Summary;
using BudgetWPFKevin.ViewModels.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetWPFKevin.Helpers
{


    public static class ServiceCollectionExtensions
    {


        // Registrerar databaskontext, AutoMapper, Services

        public static IServiceCollection AddBudgetServices(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddDbContext<BudgetContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddAutoMapper(typeof(TransactionMappingProfile));

            services.AddSingleton<IDialogService, DialogService>();
            services.AddScoped<IIncomeCalculationService, IncomeCalculationService>();
            services.AddScoped<ITransactionDialogService, TransactionDialogService>();

            return services;
        }


        // Registrerar repositories som hanterar data

        public static IServiceCollection AddRepositories(
            this IServiceCollection services)
        {
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IRecurringTransaction, RecurringTransactionRepository>();
            services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
            services.AddScoped<IAbsenceRepository, AbsenceRepository>();

            return services;
        }

        // Registrerar alla ViewModels som driver applikationens UI-logik
        public static IServiceCollection AddViewModels(
            this IServiceCollection services)
        {
            services.AddScoped<MainViewModel>();
            services.AddScoped<CategoryListVM>();
            services.AddScoped<TransactionCoordinatorVM>();
            services.AddScoped<IncomeListVM>();
            services.AddScoped<ExpenseListVM>();
            services.AddScoped<AbsenceSummaryVM>();
            services.AddScoped<MonthlySummaryVM>();
            services.AddScoped<RecurringTransactionListVM>();

            services.AddTransient<NewTransactionVM>();
            services.AddTransient<UserSettingsViewModel>();


            return services;
        }

        // Kör alla metoder samman för att registrera alla beroenden

        public static IServiceCollection AddAllBudgetDependencies(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddBudgetServices(connectionString);
            services.AddRepositories();
            services.AddViewModels();

            return services;
        }
    }
}