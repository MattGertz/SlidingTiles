using Microsoft.Maui.Graphics;
using SlidingTiles.Services;
using System;

namespace SlidingTiles.Controls
{
    /// <summary>
    /// A ContentView that combines a Button with a textured background for realistic gold leaf appearance
    /// </summary>
    public class TexturedTile : ContentView
    {
        private readonly Button _button;
        private readonly GraphicsView _textureView;
        private readonly string _tileNumber;
        private readonly int _gridSize;

        // Make event nullable to satisfy compiler requirements
        public event EventHandler? Clicked;

        public string Text 
        { 
            get => _button.Text; 
            set => _button.Text = value; 
        }

        public TexturedTile(string tileNumber, int tileSize, int gridSize)
        {
            _tileNumber = tileNumber;
            _gridSize = gridSize;
            
            // Create a Grid to host both the texture and the button
            var grid = new Grid
            {
                WidthRequest = tileSize,
                HeightRequest = tileSize
            };

            // Calculate font size based on grid size
            double fontSize = Math.Max(16, 28 - (gridSize - 3) * 3);

            // Create the texture view
            _textureView = new GraphicsView
            {
                WidthRequest = tileSize,
                HeightRequest = tileSize,
                Drawable = new GoldLeafDrawable(tileNumber, gridSize),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            // Create the button for interaction and text display
            _button = new Button
            {
                Text = tileNumber,
                FontSize = fontSize,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#B22222"), // Default red text that will be varied
                BackgroundColor = Colors.Transparent, // Transparent to let texture show through
                WidthRequest = tileSize,
                HeightRequest = tileSize,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                BorderColor = Color.FromArgb("#B8860B"), // Default border color that will be varied
                BorderWidth = 2,
                CornerRadius = 4,
                Shadow = new Shadow
                {
                    Brush = SolidColorBrush.Black,
                    Offset = new Point(3, 3),
                    Radius = 5,
                    Opacity = 0.5f
                }
            };

            // Handle button clicks
            _button.Clicked += (sender, e) => Clicked?.Invoke(this, EventArgs.Empty);

            // Set up text shadow effects for carved appearance
            ApplyTextEffects();

            // Add elements to the grid (order matters for z-index)
            grid.Add(_textureView);
            grid.Add(_button);

            // Apply visual states for pressed effect
            SetupVisualStates();

            // Set the content of this ContentView
            Content = grid;
        }

        /// <summary>
        /// Set up visual states for interaction feedback
        /// </summary>
        private void SetupVisualStates()
        {
            VisualStateManager.SetVisualStateGroups(_button, new VisualStateGroupList
            {
                new VisualStateGroup
                {
                    Name = "CommonStates",
                    States =
                    {
                        new VisualState { Name = "Normal" },
                        new VisualState
                        {
                            Name = "Pressed",
                            Setters =
                            {
                                new Setter { Property = Button.TranslationYProperty, Value = 1.0 },
                                new Setter { Property = Button.ShadowProperty, 
                                    Value = new Shadow
                                    {
                                        Brush = SolidColorBrush.Black,
                                        Offset = new Point(1, 1),
                                        Radius = 2,
                                        Opacity = 0.3f
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Apply text shadow effects for carved text appearance
        /// </summary>
        private void ApplyTextEffects()
        {
            // Create consistent but varied randomness for each tile
            Random rnd = new Random(int.Parse(_tileNumber) + _gridSize * 100);
            
            // Vary text color to simulate faded or oxidized engraving
            byte textR = (byte)Math.Clamp(178 + rnd.Next(-40, 20), 138, 198);
            byte textG = (byte)Math.Clamp(34 + rnd.Next(-20, 30), 14, 64);
            byte textB = (byte)Math.Clamp(34 + rnd.Next(-20, 10), 14, 44);
            byte textA = 255;
            
            _button.TextColor = Color.FromRgba(textR, textG, textB, textA);

            // Add subtle shadow offset variations for carved text
            var shadowOffset = new Size(
                -1 + (rnd.NextDouble() * 0.4 - 0.2), 
                -1 + (rnd.NextDouble() * 0.4 - 0.2));
                
            byte r = 139;
            byte g = 0;
            byte b = 0;
            byte a = (byte)rnd.Next(140, 200);
            
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowColor(
                _button, 
                Color.FromRgba(r, g, b, a));
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowOffset(
                _button, 
                shadowOffset);
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowRadius(
                _button, 
                (float)(rnd.NextDouble() * 0.5));
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowOpacity(
                _button, 
                (float)(0.6 + rnd.NextDouble() * 0.2));
        }
    }

    /// <summary>
    /// Drawable that renders a worn gold leaf appearance with specks and dots of discoloration
    /// </summary>
    internal class GoldLeafDrawable : IDrawable
    {
        private readonly int _seed;
        private readonly int _gridSize;
        private readonly Color _baseGold;
        
        public GoldLeafDrawable(string tileNumber, int gridSize)
        {
            _seed = int.Parse(tileNumber) + gridSize * 100; // Consistent seed for this tile
            _gridSize = gridSize;
            _baseGold = Color.FromRgba((byte)255, (byte)215, (byte)0, (byte)255); // Standard gold color
        }
        
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Use a seeded random to ensure consistent appearance for each tile
            Random rnd = new Random(_seed);
            
            // Draw base gold background
            canvas.FillColor = _baseGold;
            canvas.FillRectangle(dirtyRect);
            
            // Calculate appropriate settings based on tile size
            int speckCount = 80 + rnd.Next(-30, 30); // Number of specks/dots
            float maxSpeckSize = Math.Max(1.5f, dirtyRect.Width / 30); // Maximum speck size
            
            // Draw underlying texture (subtle variation in gold)
            DrawGoldVariations(canvas, dirtyRect, rnd);
            
            // Draw small specks where gold leaf has rubbed off
            DrawWornSpecks(canvas, dirtyRect, rnd, speckCount, maxSpeckSize);
            
            // Draw fine crackle lines in the gold leaf
            DrawCrackleLines(canvas, dirtyRect, rnd);
            
            // Draw a few larger areas of wear at the edges/corners
            DrawEdgeWear(canvas, dirtyRect, rnd);
        }
        
        private void DrawGoldVariations(ICanvas canvas, RectF dirtyRect, Random rnd)
        {
            // Create subtle variations in the base gold tone
            for (int i = 0; i < 10; i++)
            {
                float x = (float)(rnd.NextDouble() * dirtyRect.Width);
                float y = (float)(rnd.NextDouble() * dirtyRect.Height);
                float size = (float)(dirtyRect.Width * 0.2 * rnd.NextDouble()) + dirtyRect.Width * 0.1f;
                
                // Slightly darker or lighter gold
                byte r = (byte)Math.Clamp(_baseGold.Red * 255 + rnd.Next(-20, 15), 200, 255);
                byte g = (byte)Math.Clamp(_baseGold.Green * 255 + rnd.Next(-15, 10), 170, 215);
                byte b = (byte)Math.Clamp(_baseGold.Blue * 255 + rnd.Next(-5, 10), 0, 30);
                byte a = (byte)rnd.Next(30, 90); // Semi-transparent
                
                canvas.FillColor = Color.FromRgba(r, g, b, a);
                canvas.FillEllipse(x, y, size, size);
            }
        }
        
        private void DrawWornSpecks(ICanvas canvas, RectF dirtyRect, Random rnd, int count, float maxSize)
        {
            // Draw small specks where gold leaf has worn away to reveal base material
            for (int i = 0; i < count; i++)
            {
                float x = (float)(rnd.NextDouble() * dirtyRect.Width);
                float y = (float)(rnd.NextDouble() * dirtyRect.Height);
                
                // Most specks are tiny, a few are larger
                float size;
                if (rnd.NextDouble() < 0.9)
                {
                    size = (float)(rnd.NextDouble() * maxSize * 0.3) + 0.5f; // Tiny specks
                }
                else
                {
                    size = (float)(rnd.NextDouble() * maxSize * 0.7) + maxSize * 0.3f; // Larger worn spots
                }
                
                // Base material colors peeking through (vary between browns and reds)
                byte r, g, b;
                
                // 70% of specks are brown (base material), 30% are darker red (oxidation/dirt)
                if (rnd.NextDouble() < 0.7)
                {
                    // Brown base material
                    r = (byte)rnd.Next(100, 140);
                    g = (byte)rnd.Next(60, 100); 
                    b = (byte)rnd.Next(20, 60);
                }
                else
                {
                    // Darker red/black (oxidation/dirt)
                    r = (byte)rnd.Next(60, 100);
                    g = (byte)rnd.Next(20, 60);
                    b = (byte)rnd.Next(10, 40);
                }
                
                byte a = (byte)rnd.Next(140, 255); // Opacity varies
                
                canvas.FillColor = Color.FromRgba(r, g, b, a);
                
                // Mix of circles and irregular shapes
                if (rnd.NextDouble() < 0.7)
                {
                    // Simple circle specks
                    canvas.FillCircle(x, y, size);
                }
                else
                {
                    // Slightly irregular worn spots
                    float irregularity = (float)(rnd.NextDouble() * 0.4 + 0.8);
                    canvas.FillEllipse(x, y, size, size * irregularity);
                }
            }
        }
        
        private void DrawCrackleLines(ICanvas canvas, RectF dirtyRect, Random rnd)
        {
            // Draw fine crackle lines in the gold leaf
            int lineCount = 8 + rnd.Next(-3, 4); // Number of major crackle lines
            
            canvas.StrokeSize = 0.5f;
            
            for (int i = 0; i < lineCount; i++)
            {
                float startX, startY, endX, endY;
                
                // Lines often start from edges
                if (rnd.NextDouble() < 0.7)
                {
                    // Start from an edge
                    int edge = rnd.Next(4);
                    switch (edge)
                    {
                        case 0: // Top
                            startX = (float)(rnd.NextDouble() * dirtyRect.Width);
                            startY = 0;
                            break;
                        case 1: // Right
                            startX = dirtyRect.Width;
                            startY = (float)(rnd.NextDouble() * dirtyRect.Height);
                            break;
                        case 2: // Bottom
                            startX = (float)(rnd.NextDouble() * dirtyRect.Width);
                            startY = dirtyRect.Height;
                            break;
                        default: // Left
                            startX = 0;
                            startY = (float)(rnd.NextDouble() * dirtyRect.Height);
                            break;
                    }
                }
                else
                {
                    // Start from somewhere in the tile
                    startX = (float)(rnd.NextDouble() * dirtyRect.Width);
                    startY = (float)(rnd.NextDouble() * dirtyRect.Height);
                }
                
                // End point - crackles tend to be shorter and branch-like
                float angle = (float)(rnd.NextDouble() * Math.PI * 2);
                float length = (float)(rnd.NextDouble() * dirtyRect.Width * 0.4 + dirtyRect.Width * 0.1);
                
                endX = startX + (float)(Math.Cos(angle) * length);
                endY = startY + (float)(Math.Sin(angle) * length);
                
                // Crackle color - dark with varying opacity
                byte a = (byte)rnd.Next(30, 120); // Semi-transparent
                canvas.StrokeColor = Color.FromRgba((byte)30, (byte)20, (byte)10, a);
                
                // Draw the main crackle line
                PathF path = new PathF();
                path.MoveTo(startX, startY);
                
                // Add some slight curves to the path for a more natural crackle
                float midX = (startX + endX) / 2 + (float)(rnd.NextDouble() * 10 - 5);
                float midY = (startY + endY) / 2 + (float)(rnd.NextDouble() * 10 - 5);
                
                path.CurveTo(
                    (startX + midX) / 2, (startY + midY) / 2,
                    (midX + endX) / 2, (midY + endY) / 2,
                    endX, endY);
                    
                canvas.DrawPath(path);
                
                // Add some smaller branch crackles
                int branches = rnd.Next(0, 3);
                for (int j = 0; j < branches; j++)
                {
                    // Pick a point along the main crackle
                    float t = (float)rnd.NextDouble();
                    float branchX = startX + (midX - startX) * t;
                    float branchY = startY + (midY - startY) * t;
                    
                    // Branch at an angle
                    float branchAngle = angle + (float)(rnd.NextDouble() * Math.PI / 2 - Math.PI / 4);
                    float branchLength = length * 0.3f * (float)rnd.NextDouble();
                    
                    float branchEndX = branchX + (float)(Math.Cos(branchAngle) * branchLength);
                    float branchEndY = branchY + (float)(Math.Sin(branchAngle) * branchLength);
                    
                    // Thinner and more transparent for branches
                    canvas.StrokeSize = 0.3f;
                    // Convert to bytes to avoid ambiguous method call
                    byte branchOpacity = (byte)(a * 0.7);
                    canvas.StrokeColor = Color.FromRgba((byte)30, (byte)20, (byte)10, branchOpacity);
                    
                    canvas.DrawLine(branchX, branchY, branchEndX, branchEndY);
                }
            }
        }
        
        private void DrawEdgeWear(ICanvas canvas, RectF dirtyRect, Random rnd)
        {
            // More significant wear often occurs at edges and corners
            int edgeWearCount = rnd.Next(2, 5);
            
            for (int i = 0; i < edgeWearCount; i++)
            {
                float x, y, size;
                
                // Position along an edge or corner
                if (rnd.NextDouble() < 0.6)
                {
                    // Edge wear
                    int edge = rnd.Next(4);
                    switch (edge)
                    {
                        case 0: // Top
                            x = (float)(rnd.NextDouble() * dirtyRect.Width);
                            y = (float)(rnd.NextDouble() * dirtyRect.Height * 0.1);
                            break;
                        case 1: // Right
                            x = (float)(dirtyRect.Width - rnd.NextDouble() * dirtyRect.Width * 0.1);
                            y = (float)(rnd.NextDouble() * dirtyRect.Height);
                            break;
                        case 2: // Bottom
                            x = (float)(rnd.NextDouble() * dirtyRect.Width);
                            y = (float)(dirtyRect.Height - rnd.NextDouble() * dirtyRect.Height * 0.1);
                            break;
                        default: // Left
                            x = (float)(rnd.NextDouble() * dirtyRect.Width * 0.1);
                            y = (float)(rnd.NextDouble() * dirtyRect.Height);
                            break;
                    }
                    size = (float)(rnd.NextDouble() * dirtyRect.Width * 0.15) + dirtyRect.Width * 0.05f;
                }
                else
                {
                    // Corner wear
                    int corner = rnd.Next(4);
                    switch (corner)
                    {
                        case 0: // Top-left
                            x = (float)(rnd.NextDouble() * dirtyRect.Width * 0.15);
                            y = (float)(rnd.NextDouble() * dirtyRect.Height * 0.15);
                            break;
                        case 1: // Top-right
                            x = (float)(dirtyRect.Width - rnd.NextDouble() * dirtyRect.Width * 0.15);
                            y = (float)(rnd.NextDouble() * dirtyRect.Height * 0.15);
                            break;
                        case 2: // Bottom-right
                            x = (float)(dirtyRect.Width - rnd.NextDouble() * dirtyRect.Width * 0.15);
                            y = (float)(dirtyRect.Height - rnd.NextDouble() * dirtyRect.Height * 0.15);
                            break;
                        default: // Bottom-left
                            x = (float)(rnd.NextDouble() * dirtyRect.Width * 0.15);
                            y = (float)(dirtyRect.Height - rnd.NextDouble() * dirtyRect.Height * 0.15);
                            break;
                    }
                    size = (float)(rnd.NextDouble() * dirtyRect.Width * 0.12) + dirtyRect.Width * 0.08f;
                }
                
                // Base material showing through
                byte r = (byte)rnd.Next(100, 140);
                byte g = (byte)rnd.Next(60, 100);
                byte b = (byte)rnd.Next(20, 60);
                byte a = (byte)rnd.Next(100, 220);
                
                canvas.FillColor = Color.FromRgba(r, g, b, a);
                
                // Draw irregular wear patch - more natural than perfect circles
                PathF path = new PathF();
                
                // Create an irregular polygon for the wear patch
                int points = rnd.Next(5, 9);
                float[] angles = new float[points];
                float[] distances = new float[points];
                
                // Generate random angles and sort them for a convex shape
                for (int j = 0; j < points; j++)
                {
                    angles[j] = (float)(rnd.NextDouble() * Math.PI * 2);
                }
                Array.Sort(angles);
                
                // Generate random distances from center
                for (int j = 0; j < points; j++)
                {
                    distances[j] = (float)(0.7 + rnd.NextDouble() * 0.6) * size;
                }
                
                // Create the path
                for (int j = 0; j < points; j++)
                {
                    float px = x + (float)(Math.Cos(angles[j]) * distances[j]);
                    float py = y + (float)(Math.Sin(angles[j]) * distances[j]);
                    
                    if (j == 0)
                        path.MoveTo(px, py);
                    else
                        path.LineTo(px, py);
                }
                
                path.Close();
                canvas.FillPath(path);
                
                // Add some smaller wear spots around the main wear
                int satelliteCount = rnd.Next(3, 8);
                for (int j = 0; j < satelliteCount; j++)
                {
                    float angle = (float)(rnd.NextDouble() * Math.PI * 2);
                    float distance = size * 0.6f * (float)rnd.NextDouble() + size * 0.7f;
                    
                    float sx = x + (float)(Math.Cos(angle) * distance);
                    float sy = y + (float)(Math.Sin(angle) * distance);
                    float ssize = size * 0.2f * (float)rnd.NextDouble() + size * 0.1f;
                    
                    // Slightly different color
                    a = (byte)rnd.Next(80, 180); // More transparent
                    canvas.FillColor = Color.FromRgba(r, g, b, a);
                    
                    canvas.FillCircle(sx, sy, ssize);
                }
            }
        }
    }
}