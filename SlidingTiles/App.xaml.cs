using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System;
using System.Diagnostics;

namespace SlidingTiles
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();

                // Remove the deprecated MainPage assignment
                // MainPage = new AppShell();

                Debug.WriteLine("App initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in App constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        // Override CreateWindow method instead of setting MainPage directly
        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                Debug.WriteLine("CreateWindow called");
                Window window = new Window(new AppShell());
                Debug.WriteLine("Window created with AppShell");

                // You can set additional window properties here if needed
                // window.Title = "Sliding Tiles";

                Debug.WriteLine("Window created and configured successfully");
                return window;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CreateWindow: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        protected override void OnStart()
        {
            Debug.WriteLine("App.OnStart called");
            base.OnStart();
        }

        protected override void OnResume()
        {
            Debug.WriteLine("App.OnResume called");
            base.OnResume();
        }

        protected override void OnSleep()
        {
            Debug.WriteLine("App.OnSleep called");
            base.OnSleep();
        }
    }
}