using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace SlidingTiles.Services
{
    public static class TileAnimationService
    {
        /// <summary>
        /// Animates a tile from its current position to a new position
        /// </summary>
        /// <param name="view">The tile view to animate</param>
        /// <param name="xOffset">Horizontal distance to move</param>
        /// <param name="yOffset">Vertical distance to move</param>
        /// <param name="duration">Animation duration in milliseconds</param>
        /// <returns>Task that completes when animation is finished</returns>
        public static async Task SlideTileAsync(View view, double xOffset, double yOffset, uint duration = 150)
        {
            // Store the original TranslationX and TranslationY
            double originalTranslationX = view.TranslationX;
            double originalTranslationY = view.TranslationY;

            // Set up the initial position for the animation
            view.TranslationX = originalTranslationX - xOffset;
            view.TranslationY = originalTranslationY - yOffset;

            // Make sure the view is visible during animation
            view.Opacity = 1;

            // Create and start the animation
            await view.TranslateTo(originalTranslationX, originalTranslationY, duration, Easing.CubicOut);
        }
        
        /// <summary>
        /// Animates a tile from its current position to a new position, then updates its layout position
        /// </summary>
        /// <param name="view">The tile view to animate</param>
        /// <param name="fromRow">Starting row</param>
        /// <param name="fromCol">Starting column</param>
        /// <param name="toRow">Target row</param>
        /// <param name="toCol">Target column</param>
        /// <param name="tileSize">Size of each tile</param>
        /// <returns>Task that completes when animation is finished</returns>
        public static async Task AnimateTileMovementAsync(View view, int fromRow, int fromCol, int toRow, int toCol, double tileSize)
        {
            // Calculate the distance to move based on grid positions
            double xOffset = (toCol - fromCol) * tileSize;
            double yOffset = (toRow - fromRow) * tileSize;
            
            if (Math.Abs(xOffset) > 0 || Math.Abs(yOffset) > 0)
            {
                // Animate the movement
                await view.TranslateTo(xOffset, yOffset, 150, Easing.CubicOut);
                
                // Reset translation and update the actual position
                view.TranslationX = 0;
                view.TranslationY = 0;
                
                // Now we'd typically update the grid position in the game logic
            }
        }
    }
}
