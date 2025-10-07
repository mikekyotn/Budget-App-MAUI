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
    public partial class MonthViewModel:BaseViewModel
    {
        //Create the context to use for accessing the db
        private PaymentDataContext _paymentDataContext;

        public MonthViewModel(PaymentDataContext dataContext)
        {
            Title = "Monthly Budget View";
            _paymentDataContext = dataContext;
                       
            //NEED TO UPDATE THIS ARGUMENT TO RESPOND TO USER SELECTION OF MONTH
            LoadPaymentsByMonthAsync(TransactMonth.January);

            WeakReferenceMessenger.Default.Register<TransactionUpdatedMessage>(this, (recipient, message) =>
            {
                PaymentList.Clear();
                LoadPaymentsByMonthAsync(message.Value);
            });

        }

        //Get the filtered transaction data for the month from the db into a variable
        //Add each transaction into the Observable Collection-PaymentList using foreach .Add
        [RelayCommand]
        public async Task LoadPaymentsByMonthAsync(TransactMonth month)
        {
            try
            {
                await _paymentDataContext.Database.EnsureCreatedAsync();
                //filtering only the month needed from the db into a list
                var thisMonthPayments = await _paymentDataContext.Payments.Where
                (t => t.Month == month).OrderBy(t => t.DayOfMonthDue).ToListAsync();
                PaymentList.Clear();
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
            await Shell.Current.GoToAsync($"{nameof(DetailsPage)}?paymentId={payment.Id}");
        }
    }
}
