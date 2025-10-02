using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_App_MAUI.ViewModel
{
    /// <summary>
    /// Create the base view from which all other pages will inherit
    /// </summary>
    public partial class BaseViewModel:ObservableObject
    {
        [ObservableProperty]
        string title;

        
        public BaseViewModel()
        {

        }

    }
}
