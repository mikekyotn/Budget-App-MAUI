using Budget_App_MAUI.Data;
using Budget_App_MAUI.Models;
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
                context.Database.EnsureCreated();
                if(!context.Transactions.Any())
                {
                    context.Transactions.AddRange(
                        new Transaction { Type="Income", DayOfMonthDue=2, Month=TransactMonth.January, Description="My Job", Category="Salary", Comments = "NA", IsPaid=true},
                        new Transaction { Type = "Expense", DayOfMonthDue = 4, Month = TransactMonth.January, Description = "Century21 Housing", Category = "Rent", Comments = "NA", IsPaid = false },
                        new Transaction { Type = "Income", DayOfMonthDue = 2, Month = TransactMonth.TEMPLATE, Description = "My Job", Category = "Salary", Comments = "NA", IsPaid = false },
                        new Transaction { Type = "Expense", DayOfMonthDue = 6, Month = TransactMonth.January, Description = "Electric Company", Category = "Utilities", Comments = "NA", IsPaid = false }
                    );
                    context.SaveChanges();
                }
            }
            return app;
        }
    }
}
