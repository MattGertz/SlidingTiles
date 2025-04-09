using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace SlidingTiles.Extensions
{
    public static class ViewExtensions
    {
        /// <summary>
        /// Extension method to slide a view with animation
        /// </summary>
        public static async Task SlideToAsync(this View view, double x, double y, uint duration = 150)
        {
            await view.TranslateTo(x, y, duration, Easing.CubicOut);
        }
        
        /// <summary>
        /// Extension method to smoothly reposition a view within a grid
        /// </summary>
        public static async Task SlideToPositionAsync(this View view, int row, int column, double tileSize, uint duration = 150)
        {
            // Get the current grid position
            int currentRow = Grid.GetRow(view);
            int currentColumn = Grid.GetColumn(view);
            
            if (currentRow == row && currentColumn == column)
                return;
                
            // Calculate offsets
            double xOffset = (column - currentColumn) * tileSize;
            double yOffset = (row - currentRow) * tileSize;
            
            // Animate to new position
            await view.TranslateTo(xOffset, yOffset, duration, Easing.CubicOut);
            
            // Update grid position
            Grid.SetRow(view, row);
            Grid.SetColumn(view, column);
            
            // Reset translation values
            view.TranslationX = 0;
            view.TranslationY = 0;
        }
    }
}
