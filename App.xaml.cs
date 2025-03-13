using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SlidingTiles
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Make sure we're using our implementation of MainPage
            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            Window window = base.CreateWindow(activationState);

            // You can adjust window size or other properties here
            window.Title = "Sliding Tiles Puzzle";

            return window;
        }
    }
}
