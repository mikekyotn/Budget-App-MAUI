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
            builder.Services.AddSingleton<MonthViewModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<DetailsViewModel>(); //transient because may be different each time
            builder.Services.AddTransient<DetailsPage>(); //transient may not need to call often
            
            builder.Services.AddDbContext<TransactionDataContext>(options =>
            {
                options.UseSqlite("Data Source=transaction.db");
            });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            
            

            //Add dataseeding here SEE MauiDemoNoMVVM example
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TransactionDataContext>();
                //context.Database.EnsureDeleted(); //clearing the database to refresh if changing data
                context.Database.EnsureCreated();
                if(!context.Transactions.Any())
                {
                    context.Transactions.AddRange(
                        new Transaction { Type="Income", DayOfMonthDue=2, Month=TransactMonth.January, Description="My Job", Category="Salary", Comments = "NA", IsPaid=true, PaymentEstimated=2000.00m, PaymentActual=1987.25m},
                        new Transaction { Type = "Expense", DayOfMonthDue = 4, Month = TransactMonth.January, Description = "Century21 Housing", Category = "Rent", Comments = "NA", IsPaid = false, PaymentEstimated=850.00m, PaymentActual=850.00m },
                        new Transaction { Type = "Income", DayOfMonthDue = 15, Month = TransactMonth.TEMPLATE, Description = "My Job", Category = "Salary", Comments = "NA", IsPaid = false, PaymentEstimated=2000.00m, PaymentActual=0 },
                        new Transaction { Type = "Expense", DayOfMonthDue = 6, Month = TransactMonth.January, Description = "Electric Company", Category = "Utilities", Comments = "NA", IsPaid = false, PaymentEstimated=125.50m, PaymentActual=0 }
                    );
                    context.SaveChanges();
                }
            }
            return app;
        }
    }
}
