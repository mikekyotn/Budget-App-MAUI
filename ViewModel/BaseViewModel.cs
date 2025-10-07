using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget_App_MAUI.Models;

namespace Budget_App_MAUI.ViewModel
{
    /// <summary>
    /// Create the base view from which all other pages will inherit
    /// </summary>
    public partial class BaseViewModel:ObservableObject
    {
        //Make an observable collection to put on frontend and is responsive to changes
        [ObservableProperty]
        private ObservableCollection<Payment> paymentList = new ObservableCollection<Payment>();
        
        [ObservableProperty]
        string title;
               
        public BaseViewModel()
        {

        }


    }
}
