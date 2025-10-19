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
    [QueryProperty(nameof(IdQuery), "payId")]
    
    public partial class DetailsViewModel:ObservableObject // BaseViewModel
    {
        public PaymentDataContext _dataContext;
        public List<PaymentType> PaymentTypes { get; set; } //For the PaymentType Picker control

        public List<int> DaysInMonth { get; } //For the DayOfMonthDue Picker control
        public DetailsViewModel(PaymentDataContext dataContext)
        {
            //Title = "Payment Details";
            _dataContext = dataContext;
            //for the picker control need to create a list of the enum values
            PaymentTypes = Enum.GetValues(typeof(PaymentType)).Cast<PaymentType>().ToList();
            DaysInMonth = Enumerable.Range(1, 31).ToList(); //List of days 1-31 for the DayOfMonthDue Picker
        }
        PaymentMonth SelectedMonth { get; set; } //to hold the Enum from MonthQuery passed from MonthViewModel
        public string MonthQuery
        {
            set //parsing the string value into an int to match the PaymentMonth enum
            {
                if (int.TryParse(value, out int monthValue) &&
                    Enum.IsDefined(typeof(PaymentMonth), monthValue))
                {
                    SelectedMonth = (PaymentMonth)monthValue;
                }
            }
        }

        [ObservableProperty]
        private Payment payment;
        private Payment originalPaymentHolder; //to hold original payment details in case of cancel

        public string IdQuery
        {
            set
            {
                if (Guid.TryParse(value, out var guid))
                {
                    // Load the transaction details based on the parsed GUID
                    var existingPayment = _dataContext.Payments.Find(guid);
                    if (existingPayment != null)
                    {
                        Payment = existingPayment;
                        originalPaymentHolder = new Payment
                        {
                            Id = Payment.Id,
                            Type = Payment.Type,
                            DayOfMonthDue = Payment.DayOfMonthDue,
                            Month = Payment.Month,
                            Year = Payment.Year,
                            Description = Payment.Description,
                            Category = Payment.Category,
                            Comments = Payment.Comments,
                            IsPaid = Payment.IsPaid,
                            AmountEstimated = Payment.AmountEstimated,
                            AmountActual = Payment.AmountActual
                        };
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
        }
        
        [RelayCommand]
        async Task CancelEditAsync()
        {
            //return to previous page by clearing the navigation stack and using absolute route
            //reset the Payment to original values if user made changes and then cancelled
            await _dataContext.Entry(Payment).ReloadAsync(); //reload from db to discard changes           
            await Shell.Current.GoToAsync("//MenuPage/MainPage");            
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
                    _dataContext.Update(Payment); //marks as modified

                await _dataContext.SaveChangesAsync(); //commits changes to db
                //Message the MonthViewModel to refresh the list
                WeakReferenceMessenger.Default.Send(new TransactionUpdatedMessage(Payment.Month));
                
                //return to previous page by clearing the navigation stack and using absolute route
                await Shell.Current.GoToAsync($"//MenuPage/MainPage?month={Payment.Month}");

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error Saving Transaction, Check All Fields", ex.Message, "OK");
            }
        }

    }
}
