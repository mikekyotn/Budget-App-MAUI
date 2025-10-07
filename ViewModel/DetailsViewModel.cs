using Budget_App_MAUI.Data;
using Budget_App_MAUI.Messages;
using Budget_App_MAUI.Models;
using Budget_App_MAUI.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
//using Java.Time;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Budget_App_MAUI.ViewModel
{
    //This is receiveing the PaymentId from the MonthViewModel when a transaction is selected
    [QueryProperty(nameof(PaymentId), "paymentId")]
    public partial class DetailsViewModel:BaseViewModel
    {
        private PaymentDataContext _dataContext;
        public DetailsViewModel(PaymentDataContext dataContext)
        {
            Title = "Payment Details";
            _dataContext = dataContext;
        }

        [ObservableProperty]
        string paymentId;
        [ObservableProperty]
        Payment payment;

        partial void OnPaymentIdChanged(string value)
        {
            if (Guid.TryParse(value, out var guid))
            {
                // Load the transaction details based on the parsed GUID
                Payment = _dataContext.Payments.Find(guid);
            }
            else
            {
                Shell.Current.DisplayAlert("Invalid ID", "The provided Payment ID is not valid.", "OK");
                return;
            }
        }

        [RelayCommand]
        async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
        [RelayCommand]
        async Task SaveAsync()
        {
            try
            {
                _dataContext.Payments.Update(payment);
                await _dataContext.SaveChangesAsync();
                
                //filtering only the month needed from the db into a list
                var thisMonthPayments = await _dataContext.Payments.Where
                (p => p.Month == Payment.Month).OrderBy(p => p.DayOfMonthDue).ToListAsync();
                PaymentList.Clear();
                
                foreach (var p in thisMonthPayments)
                {
                    PaymentList.Add(p);
                }
                //await Shell.Current.GoToAsync("..");
                await Shell.Current.GoToAsync("..");

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error Saving Transaction, Check All Fields", ex.Message, "OK");
            }
        }
    }
}
