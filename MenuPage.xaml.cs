using Budget_App_MAUI.ViewModel;

namespace Budget_App_MAUI;

public partial class MenuPage : ContentPage
{
	public MenuPage(MenuViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }
}