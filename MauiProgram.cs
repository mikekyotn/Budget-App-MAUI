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
            builder.Services.AddTransient<MonthViewModel>(); //model for MainPage
            builder.Services.AddTransient<MainPage>();
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

            //Add initial template example data if database is empty
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PaymentDataContext>();
                //context.Database.EnsureDeleted(); //clearing the database to refresh if changing data
                context.Database.EnsureCreated();
                if(!context.Payments.Any())
                {
                    context.Payments.AddRange(
                    new Payment { Type = PaymentType.Income, DayOfMonthDue = 1, Month = PaymentMonth.TEMPLATE, Description = "Who pays/credits", Category = "Add Category", Comments = "Add comments here", IsPaid = false, AmountEstimated=0.00m, AmountActual=0, Year = 0 }
                    );
                    context.SaveChanges();
                }
                if (!context.MonthIndices.Any())
                {
                    context.MonthIndices.AddRange(                        
                        new MonthIndex { Year = 0000, Month = PaymentMonth.TEMPLATE, AvailableFunds = 0.00m }
                    );
                    context.SaveChanges();
                }
            }
            return app;
        }
    }
}
