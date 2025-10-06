using Budget_App_MAUI.ViewModel;

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
