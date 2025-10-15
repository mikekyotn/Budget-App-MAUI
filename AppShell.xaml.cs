namespace Budget_App_MAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(DetailsPage), typeof(DetailsPage));
            
        }
    }
}
