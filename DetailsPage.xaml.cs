using Budget_App_MAUI.ViewModel;

namespace Budget_App_MAUI;

public partial class DetailsPage : ContentPage
{
	public DetailsPage(DetailsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		
    }

}