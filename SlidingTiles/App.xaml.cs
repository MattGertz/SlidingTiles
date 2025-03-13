using Microsoft.Maui;
using Microsoft.Maui.Controls;
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
                
                // Set MainPage directly in constructor for compatibility with .NET MAUI preview
                MainPage = new AppShell();
                
                Debug.WriteLine("App initialized successfully with MainPage set to AppShell");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in App constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        // Still implement CreateWindow for future compatibility
        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                Debug.WriteLine("CreateWindow called");
                Window window = base.CreateWindow(activationState);
                Debug.WriteLine("Base window created");
                
                // Make sure window has proper settings
                window.Title = "Sliding Tiles Puzzle";
                window.MinimumWidth = 400;
                window.MinimumHeight = 600;

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