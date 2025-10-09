using Budget_App_MAUI.Data;
using Budget_App_MAUI.Models;
using Budget_App_MAUI.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Budget_App_MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            //Register services
            builder.Services.AddSingleton<MonthViewModel>(); //model for MainPage
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<DetailsViewModel>(); //transient because may be different each time
            builder.Services.AddTransient<DetailsPage>(); //transient may not need to call often
            builder.Services.AddSingleton<MenuViewModel>();
            builder.Services.AddSingleton<MenuPage>();

            //register the DataContext as a singleton so same instance used throughout app
            //instead of using AddDbContext which is scoped and creates new instance per request
            builder.Services.AddSingleton<PaymentDataContext>(sp =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<PaymentDataContext>();
                optionsBuilder.UseSqlite("Data Source=payments.db");
                return new PaymentDataContext(optionsBuilder.Options);
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif           

            //Add dataseeding here SEE MauiDemoNoMVVM example
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PaymentDataContext>();
                //context.Database.EnsureDeleted(); //clearing the database to refresh if changing data
                context.Database.EnsureCreated();
                if(!context.Payments.Any())
                {
                    context.Payments.AddRange(
                        new Payment { Type = PaymentType.Income, DayOfMonthDue=2, Month=PaymentMonth.January, Description="My Job", Category="Salary", Comments = "NA", IsPaid=true, AmountEstimated=1950.00m, AmountActual=1987.25m, Year = 2025},
                        new Payment { Type = PaymentType.Expense, DayOfMonthDue = 4, Month = PaymentMonth.January, Description = "Century21 Housing", Category = "Rent", Comments = "NA", IsPaid = false, AmountEstimated=850.00m, AmountActual=850.00m, Year = 2025 },
                        new Payment { Type = PaymentType.Income, DayOfMonthDue = 15, Month = PaymentMonth.TEMPLATE, Description = "My Job", Category = "Salary", Comments = "NA", IsPaid = false, AmountEstimated=2000.00m, AmountActual=0, Year = 0000 },
                        new Payment { Type = PaymentType.Expense, DayOfMonthDue = 6, Month = PaymentMonth.January, Description = "Electric Company", Category = "Utilities", Comments = "NA", IsPaid = false, AmountEstimated=125.50m, AmountActual=0, Year = 2025 },
                        new Payment { Type = PaymentType.Income, DayOfMonthDue = 2, Month = PaymentMonth.December, Description = "My Job", Category = "Salary", Comments = "NA", IsPaid = true, AmountEstimated = 1950.00m, AmountActual = 1987.25m, Year = 2024 },
                        new Payment { Type = PaymentType.Expense, DayOfMonthDue = 4, Month = PaymentMonth.December, Description = "Century21 Housing", Category = "Rent", Comments = "NA", IsPaid = true, AmountEstimated = 850.00m, AmountActual = 850.00m, Year = 2024 }
                    );
                    context.SaveChanges();
                }
                if (!context.MonthIndices.Any())
                {
                    context.MonthIndices.AddRange(
                        new MonthIndex { Year = 0000, Month = PaymentMonth.TEMPLATE },
                        new MonthIndex { Year = 2025, Month = PaymentMonth.January },
                        new MonthIndex { Year = 2024, Month = PaymentMonth.December }
                    );
                    context.SaveChanges();
                }
            }
            return app;
        }
    }
}
