using Budget_App_MAUI.Models;
using Budget_App_MAUI.ViewModel;
//using Java.Time;

namespace Budget_App_MAUI
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MonthViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

        }


    }
}
