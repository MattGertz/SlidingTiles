using Microsoft.Maui.Graphics;
using System;
using Microsoft.Maui.Controls.Shapes;

namespace SlidingTiles.Services
{
    /// <summary>
    /// Generates procedural aging textures and effects for tiles and game board
    /// </summary>
    public static class TileTextureGenerator
    {
        private static readonly Random random = new Random();
        
        /// <summary>
        /// Creates a GraphicsView drawable that renders an aged board background
        /// </summary>
        public static IDrawable CreateAgedBoardDrawable()
        {
            return new AgedBoardDrawable();
        }
        
        /// <summary>
        /// Drawable that renders an aged wood appearance for the game board
        /// </summary>
        private class AgedBoardDrawable : IDrawable
        {
            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                // Fill background with a base wood color
                canvas.FillColor = Color.FromRgb(121, 85, 72); // Brown
                canvas.FillRectangle(dirtyRect);
                
                // Draw subtle wood grain lines
                canvas.StrokeColor = Color.FromRgba(0, 0, 0, 32); // Very translucent black
                canvas.StrokeSize = 0.5f;
                
                Random rand = new Random(42); // Fixed seed for consistency
                
                // Draw horizontal wood grain lines
                for (float y = 0; y < dirtyRect.Height; y += rand.Next(5, 15))
                {
                    canvas.DrawLine(0, y, dirtyRect.Width, y + rand.Next(-3, 4));
                }
                
                // Draw some irregular darker patches to simulate aging and water stains
                for (int i = 0; i < 20; i++)
                {
                    float x = (float)(rand.NextDouble() * dirtyRect.Width);
                    float y = (float)(rand.NextDouble() * dirtyRect.Height);
                    float size = rand.Next(5, 30);
                    
                    canvas.FillColor = Color.FromRgba(0, 0, 0, rand.Next(10, 40));
                    canvas.FillEllipse(x, y, size, size * rand.Next(1, 3) * 0.5f);
                }
                
                // Draw some small "rust" spots
                for (int i = 0; i < 30; i++)
                {
                    float x = (float)(rand.NextDouble() * dirtyRect.Width);
                    float y = (float)(rand.NextDouble() * dirtyRect.Height);
                    float size = rand.Next(1, 5);
                    
                    canvas.FillColor = Color.FromRgba(165, 42, 42, rand.Next(50, 150));
                    canvas.FillCircle(x, y, size);
                }
            }
        }
    }
}