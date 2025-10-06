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

namespace Budget_App_MAUI.ViewModel
{
    public partial class MonthViewModel:BaseViewModel
    {
        //Create the context to use for accessing the db
        private TransactionDataContext _transactionDataContext;
        //Make an observable collection to put on frontend and is responsive to changes
        public ObservableCollection<Transaction> Transactions { get; set; } = new ObservableCollection<Transaction>();

        public MonthViewModel(TransactionDataContext dataContext)
        {
            Title = "Monthly Budget View";
            _transactionDataContext = dataContext;

            //NEED TO UPDATE THIS ARGUMENT TO RESPOND TO USER SELECTION OF MONTH
            LoadTransactionsByMonth(TransactMonth.January);            
        }

        //Get the filtered transaction data for the month from the db into a variable
        //Add each transaction into the Observable Collection-Transactions using foreach .Add
        private async void LoadTransactionsByMonth(TransactMonth month)
        {
            try
            {
                await _transactionDataContext.Database.EnsureCreatedAsync();
                
                //filtering only the month needed from the db into a list
                var thisMonthTransactions = await _transactionDataContext.Transactions.Where
                (t => t.Month == month).OrderBy(t => t.DayOfMonthDue).ToListAsync(); 
                
                foreach (var transaction in thisMonthTransactions)
                {
                    Transactions.Add(transaction);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error Loading Transactions", ex.Message, "OK");
            }

        }

        [RelayCommand]
        async Task GoToDetailsAsync(Transaction transaction)
        {
            if (transaction == null) { return; }
            //send the transactionId which is the Guid to the DetailsViewModel using query (?) property
            await Shell.Current.GoToAsync($"{nameof(DetailsPage)}?transactionId={transaction.Id}");
        }
    }
}
