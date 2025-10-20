using Budget_App_MAUI.Data;
using Budget_App_MAUI.Messages;
using Budget_App_MAUI.Models;
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
    [QueryProperty(nameof(SelectedMonthInt), "month")]
    public partial class MonthViewModel:BaseViewModel
    {
        //Create the context to use for accessing the db
        private PaymentDataContext _paymentDataContext;
 
        [ObservableProperty]
        PaymentMonth selectedMonth;
        [ObservableProperty]
        decimal availableFunds;
        [ObservableProperty]
        decimal projectedFunds;
        [ObservableProperty]
        bool updateFundsButtonIsEnabled;


        public string SelectedMonthInt
        {
            set //parsing the string value into an int to match the PaymentMonth enum
            {
                if (int.TryParse(value, out int monthValue) &&
                    Enum.IsDefined(typeof(PaymentMonth), monthValue))
                {
                    SelectedMonth = (PaymentMonth)monthValue;
                    if (SelectedMonth == PaymentMonth.TEMPLATE)
                        UpdateFundsButtonIsEnabled = false;
                    else
                        UpdateFundsButtonIsEnabled = true;
                }
            }
        }
        //Page constructor
        public MonthViewModel(PaymentDataContext dataContext)
        {
            //This tells the main page which month to display
            Title = $"{SelectedMonth} Budget";
            _paymentDataContext = dataContext;

            //Initialize the Observable Collection
            LoadPaymentsByMonthAsync(SelectedMonth);
            //Initial calculation of projected funds
            CalculateProjectedFunds();

            //Register to receive messages when transactions are updated
            WeakReferenceMessenger.Default.Register<TransactionUpdatedMessage>(this, (recipient, message) =>
            {
                Title = $"{message.Value} Budget"; //to update the title
                LoadPaymentsByMonthAsync(message.Value);
                CalculateProjectedFunds();
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
                //Get the available funds for the month from the MonthIndices table
                AvailableFunds = await _paymentDataContext.MonthIndices.Where(m => m.Month == month)
                    .Select(m => m.AvailableFunds).FirstAsync(); //FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error Loading Payments", ex.Message, "OK");
            }
        }
        private void CalculateProjectedFunds()
        {
            ProjectedFunds = AvailableFunds
                + PaymentList.Where(p=> p.Type == PaymentType.Income && p.IsPaid==false).Sum(p => p.AmountEstimated)
                - PaymentList.Where(p=> (p.Type == PaymentType.Expense || p.Type==PaymentType.Transfer || p.Type==PaymentType.Investment) && p.IsPaid==false).Sum(p => p.AmountEstimated);
        }

        [RelayCommand]
        async Task GoToDetailsAsync(Payment payment)
        {
            if (payment == null) { return; }
            //send the paymentId which is the Guid to the DetailsViewModel using query (?) property
            await Shell.Current.GoToAsync($"{nameof(DetailsPage)}?payId={payment.Id}&month={(int)SelectedMonth}");

        }
        [RelayCommand]
        async Task AddNewPaymentAsync()
        {
            //Create a new Guid to send to the DetailsViewModel to reuse that page for adding a new payment
            Guid newPaymentId = Guid.NewGuid();          
            //Navigate to the DetailsPage to create a new payment
            await Shell.Current.GoToAsync($"{nameof(DetailsPage)}?payId={newPaymentId}&month={(int)SelectedMonth}");
            
        }
        [RelayCommand]
        async Task GoToMenuAsync()
        {
            await Shell.Current.GoToAsync(".."); 
        }
        [RelayCommand]
        async Task UpdateFunds()
        {
            string newFunds = await Shell.Current.DisplayPromptAsync("Update Available Funds",
                "Enter the new available funds amount:", "OK", "Cancel","Amount", 10,
                Keyboard.Numeric, AvailableFunds.ToString("C"));
            AvailableFunds = decimal.TryParse(newFunds, out decimal result) ? result : AvailableFunds;
            var currentMonthIndex = await _paymentDataContext.MonthIndices
                .FirstOrDefaultAsync(m => m.Month == SelectedMonth);
            _paymentDataContext.MonthIndices.Attach(currentMonthIndex);
            currentMonthIndex.AvailableFunds = AvailableFunds;
            //_paymentDataContext.MonthIndices.Update(new MonthIndex(SelectedMonth, AvailableFunds));
            await _paymentDataContext.SaveChangesAsync();
        }

        partial void OnAvailableFundsChanged(decimal value)
        {
            CalculateProjectedFunds();
        }
    }
}
