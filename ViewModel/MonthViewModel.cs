using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget_App_MAUI.Models;
using Budget_App_MAUI.Data;
using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Budget_App_MAUI.Messages;

namespace Budget_App_MAUI.ViewModel
{
    [QueryProperty(nameof(SelectedMonthInt), "month")]
    public partial class MonthViewModel:BaseViewModel
    {
        //Create the context to use for accessing the db
        private PaymentDataContext _paymentDataContext;

        //This tells the main page which month to display
        [ObservableProperty]
        PaymentMonth selectedMonth;
        public string SelectedMonthInt
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
        public MonthViewModel(PaymentDataContext dataContext)
        {
            Title = $"{selectedMonth} Budget";
            _paymentDataContext = dataContext;
                       
            LoadPaymentsByMonthAsync(SelectedMonth);

            WeakReferenceMessenger.Default.Register<TransactionUpdatedMessage>(this, (recipient, message) =>
            {                
                LoadPaymentsByMonthAsync(message.Value);
                Title = $"{message.Value} Budget"; //to update the title
            });
        }


        //Get the filtered transaction data for the month from the db into a variable
        //Add each transaction into the Observable Collection-PaymentList using foreach .Add
        [RelayCommand]
        public async Task LoadPaymentsByMonthAsync(PaymentMonth month)
        {
            try
            {
                PaymentList.Clear();
                await _paymentDataContext.Database.EnsureCreatedAsync();                
                _paymentDataContext.ChangeTracker.Clear(); //clear the tracking to avoid stale data
                //filtering only the month needed from the db into a list
                var thisMonthPayments = await _paymentDataContext.Payments.Where
                (t => t.Month == month).OrderBy(t => t.DayOfMonthDue).ToListAsync();
                
                foreach (var payment in thisMonthPayments)
                {
                    PaymentList.Add(payment);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error Loading Payments", ex.Message, "OK");
            }
        }

        [RelayCommand]
        async Task GoToDetailsAsync(Payment payment)
        {
            if (payment == null) { return; }
            //send the paymentId which is the Guid to the DetailsViewModel using query (?) property
            await Shell.Current.GoToAsync($"{nameof(DetailsPage)}?payId={payment.Id}&month={(int)selectedMonth}");

        }
        [RelayCommand]
        async Task AddNewPaymentAsync()
        {
            //Create a new Guid to send to the DetailsViewModel to reuse that page for adding a new payment
            Guid newPaymentId = Guid.NewGuid();            
            //Navigate to the DetailsPage to create a new payment
            await Shell.Current.GoToAsync($"{nameof(DetailsPage)}?payId={newPaymentId}&month={(int)selectedMonth}");
        }
        [RelayCommand]
        async Task GoToMenuAsync()
        {
            await Shell.Current.GoToAsync(".."); 
        }
    }
}
