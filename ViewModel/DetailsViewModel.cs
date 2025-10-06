using Budget_App_MAUI.Data;
using Budget_App_MAUI.Models;
using Budget_App_MAUI.ViewModel;
using Budget_App_MAUI.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;


//using Java.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Budget_App_MAUI.ViewModel
{
    //This is receiveing the transactionId from the MonthViewModel when a transaction is selected
    [QueryProperty(nameof(TransactionId), "transactionId")]
    public partial class DetailsViewModel : BaseViewModel
    {
        private TransactionDataContext _dataContext;
        public DetailsViewModel(TransactionDataContext dataContext)
        {
            Title = "Transaction Details";
            _dataContext = dataContext;
        }

        [ObservableProperty]
        string transactionId;
        [ObservableProperty]
        Models.Transaction transaction;

        partial void OnTransactionIdChanged(string value)
        {
            if (Guid.TryParse(value, out var guid))
            {
                // Load the transaction details based on the parsed GUID
                Transaction = _dataContext.Transactions.Find(guid);
            }
            else
            {
                Shell.Current.DisplayAlert("Invalid ID", "The provided transaction ID is not valid.", "OK");
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
                _dataContext.Transactions.Update(transaction);
                await _dataContext.SaveChangesAsync(); 
                WeakReferenceMessenger.Default.Send(new TransactionUpdatedMessage());
                await Shell.Current.GoToAsync("..");
                
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error Saving Transaction, Check All Fields", ex.Message, "OK");
            }
        }
    }
}
