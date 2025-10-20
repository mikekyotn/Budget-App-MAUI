namespace Budget_App_MAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Set the app theme to Light mode
            //Application.Current.UserAppTheme = AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            const int newWidth = 800;
            const int newHeight = 700;

            var window = new Window(new AppShell())
            {
                Width = newWidth,
                Height = newHeight
            };
            return window;
        }

        

    }
}