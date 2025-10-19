using Budget_App_MAUI.Data;
using Budget_App_MAUI.Models;
using Budget_App_MAUI.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_App_MAUI.ViewModel
{
    public partial class MenuViewModel: BaseViewModel
    {
        public PaymentDataContext _dataContext;
        [ObservableProperty]
        ObservableCollection<int> availableYears;

        [ObservableProperty]
        int selectedYear;

        [ObservableProperty]
        ObservableCollection<PaymentMonth> availableMonths;

        [ObservableProperty]
        PaymentMonth selectedMonth;
        [ObservableProperty]
        private bool isCurrentMonthEnabled = false;
        [ObservableProperty]
        private bool isCopyFromTemplateEnabled = true; 

        int currentYear = DateTime.Now.Year;
        int currentMonth = DateTime.Now.Month;
        public MenuViewModel(PaymentDataContext dataContext)
        {
            Title = "Menu";
            _dataContext = dataContext;
            GetAvailableYearsAsync();
            
            if(MonthExistsInDb(currentMonth, currentYear))
            {
                IsCurrentMonthEnabled = true;
                SelectedYear = currentYear;
                SelectedMonth = (PaymentMonth)currentMonth;
            }
            int nextMonth = currentMonth + 1 > 12 ? 1 : currentMonth + 1;
            int nextMonthYear = currentMonth + 1 > 12 ? currentYear + 1 : currentYear;
            if (MonthExistsInDb(nextMonth, nextMonthYear))
            {
                IsCopyFromTemplateEnabled = false; //disable the button if next month already exists
            }

        }
        public async Task GetAvailableYearsAsync()
        {
            var years = await _dataContext.MonthIndices
                .Select(m => m.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
            
            years.Remove(0); //remove template year if exists
            AvailableYears = new ObservableCollection<int>(years);
            SelectedYear = AvailableYears.FirstOrDefault(); //default to most recent year
            await LoadMonthsForYearAsync(SelectedYear);

        }
        //This method loads the months for the selected year as it's selected/changed by user
        partial void OnSelectedYearChanged(int value)
        {
            LoadMonthsForYearAsync(value);
        }
        public async Task LoadMonthsForYearAsync(int year)
        {
            var months = await _dataContext.MonthIndices
                .Where(m => m.Year == year)
                .OrderByDescending(m => m.Month)
                .Select(m => m.Month)
                .ToListAsync();

            months.Remove(0); //remove template month if exists
            AvailableMonths = new ObservableCollection<PaymentMonth>(months);
            SelectedMonth = AvailableMonths.FirstOrDefault(); //default to most recent month
        }
        //Check if current month exists in  db
        private bool MonthExistsInDb(int month, int year)
        {
            PaymentMonth payMonth = (PaymentMonth)month;
            return _dataContext.MonthIndices.Any(m => m.Month == payMonth && m.Year == year);
        }

        [RelayCommand]
        async Task ViewCurrentMonthAsync()
        {            
            //send the selected month to the MonthViewModel using query (?) property
            await Shell.Current.GoToAsync($"{nameof(MainPage)}?month={currentMonth}");
            //Message the MonthViewModel to refresh the list
            WeakReferenceMessenger.Default.Send(new TransactionUpdatedMessage((PaymentMonth)currentMonth));
        }
        [RelayCommand]
        async Task ViewTemplateAsync()
        {            
            //send the selected month to the MonthViewModel using query (?) property
            await Shell.Current.GoToAsync($"{nameof(MainPage)}?month=0");
            //Message the MonthViewModel to refresh the list
            WeakReferenceMessenger.Default.Send(new TransactionUpdatedMessage(PaymentMonth.TEMPLATE));
        }
        [RelayCommand]
        async Task ViewSelectedMonthAsync()
        {            
            //send the selected month to the MonthViewModel using query (?) property
            await Shell.Current.GoToAsync($"{nameof(MainPage)}?month={SelectedMonth}");
            //Message the MonthViewModel to refresh the list
            WeakReferenceMessenger.Default.Send(new TransactionUpdatedMessage((PaymentMonth)SelectedMonth));
        }
        [RelayCommand]
        async Task CreateFromTemplate()
        {
            if(IsCurrentMonthEnabled)
            {
                //do some action to copy template with next month, this year/next if Dec
                CopyTemplateToMonth(_dataContext, currentMonth + 1 > 12 ? 1 : currentMonth + 1, currentMonth + 1 > 12 ? currentYear + 1 : currentYear);
                await Shell.Current.DisplayAlert("Template Copied", "Next month was created and populated from template. Please select year/month below to view.", "OK");
                await GetAvailableYearsAsync(); //refresh the years/months pickers
                IsCopyFromTemplateEnabled = false; //disable the button if next month now exists
            }
            else
            {
                //do action to copy template to current month
                CopyTemplateToMonth(_dataContext, currentMonth, currentYear);                
                await Shell.Current.DisplayAlert("Template Copied", "Current month did not exist so was created and populated from template. Please use the View/Update Current Month above.", "OK");
                await GetAvailableYearsAsync(); //refresh the years/months pickers
                IsCurrentMonthEnabled = true; //enable the button now that current month exists
            }
        }
        static void CopyTemplateToMonth(PaymentDataContext context, int month, int year)
        {
            var templatePayments = context.Payments.Where(p => p.Month == PaymentMonth.TEMPLATE).ToList();
            foreach (var template in templatePayments)
            {
                var newPayment = new Payment
                {
                    Id = Guid.NewGuid(),
                    Type = template.Type,
                    DayOfMonthDue = template.DayOfMonthDue,
                    Month = (PaymentMonth)month,
                    Year = year,
                    Description = template.Description,
                    Category = template.Category,
                    Comments = template.Comments,                                        
                    IsPaid = template.IsPaid,
                    AmountEstimated = template.AmountEstimated,
                    AmountActual = template.AmountActual
                    
                };
                context.Payments.Add(newPayment);
            }
            //add the new month to MonthIndices if it doesn't exist
            if (!context.MonthIndices.Any(m => m.Month == (PaymentMonth)month && m.Year == year))
            {
                var newMonthIndex = new MonthIndex
                {
                    Month = (PaymentMonth)month,
                    Year = year
                };
                context.MonthIndices.Add(newMonthIndex);
            }
            context.SaveChanges();
        }
    }
}
