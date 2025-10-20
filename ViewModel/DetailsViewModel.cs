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

    public partial class DetailsViewModel : BaseViewModel
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
            set //parsing the string value into an int to match the PaymentMonth enum
            {
                if (int.TryParse(value, out int monthValue) &&
                    Enum.IsDefined(typeof(PaymentMonth), monthValue))
                {
                    SelectedMonth = (PaymentMonth)monthValue;
                    if (SelectedMonth == PaymentMonth.TEMPLATE)
                        IsMonthlyEdit = false;
                    else
                        IsMonthlyEdit = true;
                }
            }
        }

        [ObservableProperty]
        bool canDelete;
        [ObservableProperty] //obserbable for options in the DetailsPage
        bool isMonthlyEdit;
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
                        CanDelete = true; //enable delete button and IsPaid functions for existing payments
                    }
                    else //create a new Payment with the SelectedMonth
                    {
                        int year;
                        if(!IsMonthlyEdit) //means it's a template edit
                            year = 0000; //template payments have year 0000
                        else
                            year = DateTime.Now.Year; //current year for new payments

                        Payment = new Payment(Guid.NewGuid(), SelectedMonth, year);
                        CanDelete = false; //disable delete button and IsPaid functions for new payments

                        //We do not Save to db until the user adds details and saves in case they cancel
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
                await Shell.Current.DisplayAlert("Error Saving Transaction", ex.Message, "OK");
            }
        }
        [RelayCommand]
        async Task DeleteAsync()
        {
            bool confirmDelete = await Shell.Current.DisplayAlert("Confirm Delete", "Are you sure you want to delete this payment?", "Yes", "No");
            if (confirmDelete)
            {
                try
                {
                    _dataContext.Payments.Remove(Payment);
                    await _dataContext.SaveChangesAsync(); //commits changes to db
                    //Message the MonthViewModel to refresh the list
                    WeakReferenceMessenger.Default.Send(new TransactionUpdatedMessage(Payment.Month));
                    //return to previous page by clearing the navigation stack and using absolute route
                    await Shell.Current.GoToAsync($"//MenuPage/MainPage?month={Payment.Month}");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error Deleting Payment", ex.Message, "OK");
                }
            }

        }
    }
}
