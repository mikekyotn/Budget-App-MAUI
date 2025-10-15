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
        bool isCurrentMonthEnabled = false;
        int currentYear = DateTime.Now.Year;
        int currentMonth = DateTime.Now.Month;
        public MenuViewModel(PaymentDataContext dataContext)
        {
            Title = "Menu";
            _dataContext = dataContext;
            GetAvailableYearsAsync();
            
            if(MonthExistsInDb(currentMonth, currentYear))
            {
                isCurrentMonthEnabled = true;
                SelectedYear = currentYear;
                SelectedMonth = (PaymentMonth)currentMonth;
            }
            
        }

        public async Task GetAvailableYearsAsync()
        {
            var years = await _dataContext.MonthIndices
                .Select(m => m.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
            
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
            //if (SelectedMonth == 0 || SelectedYear == 0) { return; } //ensure valid selection
            //send the selected month to the MonthViewModel using query (?) property
            await Shell.Current.GoToAsync($"{nameof(MainPage)}?month={currentMonth}");
            //Message the MonthViewModel to refresh the list
            WeakReferenceMessenger.Default.Send(new TransactionUpdatedMessage((PaymentMonth)currentMonth));
        }
        [RelayCommand]
        async Task ViewTemplateAsync()
        {
            //if (SelectedMonth == 0 || SelectedYear == 0) { return; } //ensure valid selection
            //send the selected month to the MonthViewModel using query (?) property
            await Shell.Current.GoToAsync($"{nameof(MainPage)}?month=0");
            //Message the MonthViewModel to refresh the list
            WeakReferenceMessenger.Default.Send(new TransactionUpdatedMessage(PaymentMonth.TEMPLATE));
        }
    }
}
