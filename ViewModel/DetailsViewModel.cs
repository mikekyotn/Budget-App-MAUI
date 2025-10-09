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
//using Windows.UI;


namespace Budget_App_MAUI.ViewModel
{
    //This is receiveing the PaymentId and Month as strings from the MonthViewModel when a transaction is selected
    //ORDER MATTERS for the QueryProperty attributes and determines which property is set first
    //Need month to be set first so the Payment can be created with the correct month if a new Payment is being added
    [QueryProperty(nameof(MonthQuery), "month")]
    [QueryProperty(nameof(PaymentId), "paymentId")]
    
    public partial class DetailsViewModel:BaseViewModel
    {
        public PaymentDataContext _dataContext;
        public List<PaymentType> PaymentTypes { get; set; } //For the PaymentType Picker control

        public List<int> DaysInMonth { get; } //For the DayOfMonthDue Picker control
        public DetailsViewModel(PaymentDataContext dataContext)
        {
            Title = "Payment Details";
            _dataContext = dataContext;
            //for the picker control need to create a list of the enum values
            PaymentTypes = Enum.GetValues(typeof(PaymentType)).Cast<PaymentType>().ToList();
            DaysInMonth = Enumerable.Range(1, 31).ToList(); //List of days 1-31 for the DayOfMonthDue Picker
        }
        PaymentMonth SelectedMonth { get; set; } //to hold the Enum from MonthQuery passed from MonthViewModel
        public string MonthQuery
        {
            set //parsing the string value into an int to match the TransactMonth enum
            {
                if (int.TryParse(value, out int monthValue) &&
                    Enum.IsDefined(typeof(PaymentMonth), monthValue))
                {
                    SelectedMonth = (PaymentMonth)monthValue;
                }
            }
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
                if (_dataContext.Payments.Find(guid) != null)
                {
                    Payment = _dataContext.Payments.Find(guid);
                }
                else //create a new Payment with the SelectedMonth
                {
                    int year;
                    if (SelectedMonth == PaymentMonth.TEMPLATE)
                        year = 0000; //template payments have year 0000
                    else
                        year = DateTime.Now.Year; //current year for new payments
                    Payment = new Payment(Guid.NewGuid(), SelectedMonth, year);
                    //Do not add the new Payment to the db until the user adds details and saves
                    
                }
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
                //Add if new Payment otherwise update existing
                var existing = await _dataContext.Payments.FindAsync(Payment.Id);
                if (existing == null)
                    _dataContext.Add(Payment);
                else
                    _dataContext.Update(Payment);

                //check if need to update the MonthIndex table
                bool yrMonthExists = await _dataContext.MonthIndices
                    .AnyAsync(m => m.Year == Payment.Year && m.Month == Payment.Month);

                if (!yrMonthExists)
                {
                    _dataContext.MonthIndices.Add(new MonthIndex
                    {
                        Year = Payment.Year,
                        Month = Payment.Month
                    });
                }
                await _dataContext.SaveChangesAsync();

                //Message the MonthViewModel to refresh the list
                WeakReferenceMessenger.Default.Send(new TransactionUpdatedMessage(payment.Month));
                //retutn to previous page
                await Shell.Current.GoToAsync("..");

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error Saving Transaction, Check All Fields", ex.Message, "OK");
            }
        }
    }
}
