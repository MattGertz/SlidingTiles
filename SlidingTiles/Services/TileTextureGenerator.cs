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
        /// Applies aging effects to a button representing a tile
        /// </summary>
        public static void ApplyAgingEffects(Button tile, string tileNumber, int gridSize)
        {
            // Create consistent but varied randomness for each tile
            Random rnd = new Random(int.Parse(tileNumber) + gridSize * 100);
            
            // Generate slight color variations for aging effect
            double satVariation = 0.7 + rnd.NextDouble() * 0.3; // 70-100% saturation (some wear)
            double brightVariation = 0.9 + rnd.NextDouble() * 0.1; // 90-100% brightness
            
            // Calculate color components explicitly as bytes
            byte r = (byte)Math.Clamp((255 * brightVariation) + rnd.Next(-10, 10), 0, 255);
            byte g = (byte)Math.Clamp((215 * brightVariation * satVariation) + rnd.Next(-15, 5), 0, 255);
            byte b = (byte)Math.Clamp((0 * satVariation) + rnd.Next(0, 15), 0, 255);
            byte a = 255;
            
            // Base gold color with variations - simulate worn/preserved areas
            Color tileBaseColor = Color.FromRgba(r, g, b, a);
            
            // Border color with age variations - slightly darker than the tile
            byte borderR = (byte)Math.Clamp(184 + rnd.Next(-40, 20), 144, 204);
            byte borderG = (byte)Math.Clamp(134 + rnd.Next(-30, 20), 104, 154);
            byte borderB = (byte)Math.Clamp(11 + rnd.Next(-5, 15), 6, 26);
            byte borderA = 255;
            Color borderColor = Color.FromRgba(borderR, borderG, borderB, borderA);
            
            // Text color with age variations - simulate faded or oxidized engraving
            byte textR = (byte)Math.Clamp(178 + rnd.Next(-40, 20), 138, 198);
            byte textG = (byte)Math.Clamp(34 + rnd.Next(-20, 30), 14, 64);
            byte textB = (byte)Math.Clamp(34 + rnd.Next(-20, 10), 14, 44);
            byte textA = 255;
            Color textColor = Color.FromRgba(textR, textG, textB, textA);
            
            // Apply the colors to the tile
            tile.BackgroundColor = tileBaseColor;
            tile.BorderColor = borderColor;
            tile.TextColor = textColor;
            
            // Simulate crackles and uneven wear in the tile's finish
            if (rnd.NextDouble() < 0.8) // 80% of tiles have some aging effect
            {
                // Generate procedural aging pattern using GraphicsView for each tile
                ApplyTextShadowEffects(tile, rnd);
                
                // Apply a procedural graphic as the background of the button
                ApplyBackgroundPattern(tile, rnd);
            }
        }
        
        /// <summary>
        /// Applies varied text shadow effects to create an aged carved appearance
        /// </summary>
        private static void ApplyTextShadowEffects(Button tile, Random rnd)
        {
            // Add subtle shadow offset variations for carved text
            // Creates an irregular/imperfect carved effect like hand carving
            var shadowOffset = new Size(
                -1 + (rnd.NextDouble() * 0.4 - 0.2), 
                -1 + (rnd.NextDouble() * 0.4 - 0.2));
                
            byte r = 139;
            byte g = 0;
            byte b = 0;
            byte a = (byte)rnd.Next(140, 200);
            
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowColor(
                tile, 
                Color.FromRgba(r, g, b, a)); // Darker red shadow with slight opacity variance
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowOffset(
                tile, 
                shadowOffset); // Slightly varied offset for aged carved look
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowRadius(
                tile, 
                (float)(rnd.NextDouble() * 0.5)); // Varied shadow blur - some sharper, some more worn
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowOpacity(
                tile, 
                (float)(0.6 + rnd.NextDouble() * 0.2)); // Varied opacity
        }
        
        /// <summary>
        /// Applies a subtle background pattern to create an aged appearance
        /// </summary>
        private static void ApplyBackgroundPattern(Button tile, Random rnd)
        {
            // Use a unique ClassId to help identify this tile for any future styling
            tile.ClassId = $"aged-tile-{rnd.Next(1000)}";
            
            // Add a subtle border shader effect varying with each tile
            Border border = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(tile.CornerRadius) },
                Stroke = tile.BorderColor,
                StrokeThickness = tile.BorderWidth,
                BackgroundColor = Colors.Transparent,
                Opacity = 0.4,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            // This could be extended in the future to add more complex graphical aging patterns
        }
        
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