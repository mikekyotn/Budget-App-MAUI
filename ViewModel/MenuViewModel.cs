using Budget_App_MAUI.Data;
using Budget_App_MAUI.Models;
using CommunityToolkit.Mvvm.ComponentModel;
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
        public MenuViewModel(PaymentDataContext dataContext)
        {
            Title = "Menu";
            _dataContext = dataContext;
            GetAvailableYearsAsync();
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

    }
}
