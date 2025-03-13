namespace SlidingTiles;

public partial class AppShell : Shell
{
    public AppShell()
    {
        try
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("AppShell initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing AppShell: {ex.Message}");
        }
    }
}
