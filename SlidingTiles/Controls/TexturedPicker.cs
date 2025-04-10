using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using SlidingTiles.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Handlers;

namespace SlidingTiles.Controls
{
    /// <summary>
    /// A ContentView that combines a Picker with a textured background for realistic gold leaf appearance
    /// </summary>
    public class TexturedPicker : ContentView
    {
        private readonly Microsoft.Maui.Controls.Picker _picker;
        private readonly GraphicsView _textureView;
        private readonly Border _border;
        
        // Expose necessary Picker properties
        public IList<string> Items 
        {
            get => _picker.ItemsSource?.Cast<string>().ToList() ?? new List<string>();
            set 
            {
                if (value != null)
                {
                    // Explicit cast to System.Collections.IList as required by the Picker's ItemsSource property
                    _picker.ItemsSource = (System.Collections.IList)value;
                }
            }
        }
        
        public int SelectedIndex
        {
            get => _picker.SelectedIndex;
            set => _picker.SelectedIndex = value;
        }
        
        public string SelectedItem
        {
            get => _picker.SelectedItem?.ToString() ?? string.Empty;
        }
        
        // Forward the SelectedIndexChanged event
        public event EventHandler SelectedIndexChanged
        {
            add => _picker.SelectedIndexChanged += value;
            remove => _picker.SelectedIndexChanged -= value;
        }
        
        public TexturedPicker()
        {
            // Create a Grid to host the texture and picker
            var grid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            
            // Add border with matching brown color
            _border = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4) },
                Stroke = Color.FromArgb("#5D4037"),
                StrokeThickness = 3,
                Padding = 0,
                Shadow = new Shadow
                {
                    Brush = SolidColorBrush.Black,
                    Offset = new Point(3, 3),
                    Radius = 5,
                    Opacity = 0.5f
                },
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            
            // Create the texture view with gold leaf drawable
            _textureView = new GraphicsView
            {
                Drawable = new GoldLeafPickerDrawable(),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            
            // Create the picker with transparent background
            _picker = new Microsoft.Maui.Controls.Picker
            {
                BackgroundColor = Colors.Transparent,
                TextColor = Color.FromArgb("#B22222"),  // Match TexturedTile text color
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            
            // Apply text effects similar to TexturedTile
            ApplyTextEffects();
            
            // Set custom property for platform-specific styling
            _picker.SetValue(CustomPickerProperties.ChevronColorProperty, Color.FromArgb("#5D4037"));
            
            // Assemble the components
            grid.Add(_textureView);
            grid.Add(_picker);
            
            _border.Content = grid;
            Content = _border;
        }
        
        private void ApplyTextEffects()
        {
            // Apply similar text effects as in TexturedTile and ShuffleButton
            Random rnd = new Random(42);
            
            // Vary text color like the tiles
            byte textR = (byte)Math.Clamp(178 + rnd.Next(-40, 20), 138, 198);
            byte textG = (byte)Math.Clamp(34 + rnd.Next(-20, 30), 14, 64);
            byte textB = (byte)Math.Clamp(34 + rnd.Next(-20, 10), 14, 44);
            
            _picker.TextColor = Color.FromRgba(textR, textG, textB, (byte)255);
            
            // Add shadow effects for carved appearance
            var shadowOffset = new Size(
                -1 + (rnd.NextDouble() * 0.4 - 0.2),
                -1 + (rnd.NextDouble() * 0.4 - 0.2));
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowColor(
                _picker, 
                Color.FromRgba((byte)139, (byte)0, (byte)0, (byte)rnd.Next(140, 200)));
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowOffset(
                _picker,
                shadowOffset);
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowRadius(
                _picker,
                (float)(rnd.NextDouble() * 0.5));
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowOpacity(
                _picker,
                (float)(0.6 + rnd.NextDouble() * 0.2));
        }
    }

    // Define custom property for chevron color
    public static class CustomPickerProperties
    {
        public static readonly BindableProperty ChevronColorProperty = 
            BindableProperty.CreateAttached("ChevronColor", typeof(Color), typeof(CustomPickerProperties), Colors.White);
            
        public static Color GetChevronColor(BindableObject view)
        {
            return (Color)view.GetValue(ChevronColorProperty);
        }
            
        public static void SetChevronColor(BindableObject view, Color value)
        {
            view.SetValue(ChevronColorProperty, value);
        }
    }

    /// <summary>
    /// Drawable for the Picker that renders a gold leaf appearance with wear patterns
    /// to match the tile aesthetic
    /// </summary>
    internal class GoldLeafPickerDrawable : IDrawable
    {
        private readonly Color _baseGold;
        private readonly Random _rnd;
        
        public GoldLeafPickerDrawable()
        {
            _baseGold = Color.FromRgba((byte)255, (byte)215, (byte)0, (byte)255); // Standard gold color
            _rnd = new Random(99); // Fixed seed for consistent appearance
        }
        
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Draw base gold background
            canvas.FillColor = _baseGold;
            canvas.FillRectangle(dirtyRect);
            
            // Calculate appropriate settings based on control size
            int speckCount = 30 + _rnd.Next(-10, 20); // Number of specks/dots
            float maxSpeckSize = Math.Max(1.5f, dirtyRect.Width / 30); // Maximum speck size
            
            // Draw underlying texture (subtle variation in gold)
            DrawGoldVariations(canvas, dirtyRect);
            
            // Draw small specks where gold leaf has rubbed off
            DrawWornSpecks(canvas, dirtyRect, speckCount, maxSpeckSize);
            
            // Draw fine crackle lines in the gold leaf
            DrawCrackleLines(canvas, dirtyRect);
            
            // Draw a few larger areas of wear at the edges/corners
            DrawEdgeWear(canvas, dirtyRect);
        }
        
        private void DrawGoldVariations(ICanvas canvas, RectF dirtyRect)
        {
            // Create subtle variations in the base gold tone
            for (int i = 0; i < 8; i++)
            {
                float x = (float)(_rnd.NextDouble() * dirtyRect.Width);
                float y = (float)(_rnd.NextDouble() * dirtyRect.Height);
                float size = (float)(dirtyRect.Width * 0.2 * _rnd.NextDouble()) + dirtyRect.Width * 0.1f;
                
                // Slightly darker or lighter gold
                byte r = (byte)Math.Clamp(_baseGold.Red * 255 + _rnd.Next(-20, 15), 200, 255);
                byte g = (byte)Math.Clamp(_baseGold.Green * 255 + _rnd.Next(-15, 10), 170, 215);
                byte b = (byte)Math.Clamp(_baseGold.Blue * 255 + _rnd.Next(-5, 10), 0, 30);
                byte a = (byte)_rnd.Next(30, 90); // Semi-transparent
                
                canvas.FillColor = Color.FromRgba(r, g, b, a);
                canvas.FillEllipse(x, y, size, size);
            }
        }
        
        private void DrawWornSpecks(ICanvas canvas, RectF dirtyRect, int count, float maxSize)
        {
            // Draw small specks where gold leaf has worn away to reveal base material
            for (int i = 0; i < count; i++)
            {
                float x = (float)(_rnd.NextDouble() * dirtyRect.Width);
                float y = (float)(_rnd.NextDouble() * dirtyRect.Height);
                
                // Most specks are tiny, a few are larger
                float size;
                if (_rnd.NextDouble() < 0.9)
                {
                    size = (float)(_rnd.NextDouble() * maxSize * 0.3) + 0.5f; // Tiny specks
                }
                else
                {
                    size = (float)(_rnd.NextDouble() * maxSize * 0.7) + maxSize * 0.3f; // Larger worn spots
                }
                
                // Base material colors peeking through (vary between browns and reds)
                byte r, g, b;
                
                // 70% of specks are brown (base material), 30% are darker red (oxidation/dirt)
                if (_rnd.NextDouble() < 0.7)
                {
                    // Brown base material
                    r = (byte)_rnd.Next(100, 140);
                    g = (byte)_rnd.Next(60, 100); 
                    b = (byte)_rnd.Next(20, 60);
                }
                else
                {
                    // Darker red/black (oxidation/dirt)
                    r = (byte)_rnd.Next(60, 100);
                    g = (byte)_rnd.Next(20, 60);
                    b = (byte)_rnd.Next(10, 40);
                }
                
                byte a = (byte)_rnd.Next(140, 255); // Opacity varies
                
                canvas.FillColor = Color.FromRgba(r, g, b, a);
                
                // Mix of circles and irregular shapes
                if (_rnd.NextDouble() < 0.7)
                {
                    // Simple circle specks
                    canvas.FillCircle(x, y, size);
                }
                else
                {
                    // Slightly irregular worn spots
                    float irregularity = (float)(_rnd.NextDouble() * 0.4 + 0.8);
                    canvas.FillEllipse(x, y, size, size * irregularity);
                }
            }
        }
        
        private void DrawCrackleLines(ICanvas canvas, RectF dirtyRect)
        {
            // Draw fine crackle lines in the gold leaf
            int lineCount = 4 + _rnd.Next(-2, 3); // Number of major crackle lines
            
            canvas.StrokeSize = 0.5f;
            
            for (int i = 0; i < lineCount; i++)
            {
                float startX, startY, endX, endY;
                
                // Lines often start from edges
                if (_rnd.NextDouble() < 0.7)
                {
                    // Start from an edge
                    int edge = _rnd.Next(4);
                    switch (edge)
                    {
                        case 0: // Top
                            startX = (float)(_rnd.NextDouble() * dirtyRect.Width);
                            startY = 0;
                            break;
                        case 1: // Right
                            startX = dirtyRect.Width;
                            startY = (float)(_rnd.NextDouble() * dirtyRect.Height);
                            break;
                        case 2: // Bottom
                            startX = (float)(_rnd.NextDouble() * dirtyRect.Width);
                            startY = dirtyRect.Height;
                            break;
                        default: // Left
                            startX = 0;
                            startY = (float)(_rnd.NextDouble() * dirtyRect.Height);
                            break;
                    }
                }
                else
                {
                    // Start from somewhere in the control
                    startX = (float)(_rnd.NextDouble() * dirtyRect.Width);
                    startY = (float)(_rnd.NextDouble() * dirtyRect.Height);
                }
                
                // End point - crackles tend to be shorter and branch-like
                float angle = (float)(_rnd.NextDouble() * Math.PI * 2);
                float length = (float)(_rnd.NextDouble() * dirtyRect.Width * 0.4 + dirtyRect.Width * 0.1);
                
                endX = startX + (float)(Math.Cos(angle) * length);
                endY = startY + (float)(Math.Sin(angle) * length);
                
                // Crackle color - dark with varying opacity
                byte a = (byte)_rnd.Next(30, 120); // Semi-transparent
                canvas.StrokeColor = Color.FromRgba((byte)30, (byte)20, (byte)10, a);
                
                // Draw the main crackle line
                PathF path = new PathF();
                path.MoveTo(startX, startY);
                
                // Add some slight curves to the path for a more natural crackle
                float midX = (startX + endX) / 2 + (float)(_rnd.NextDouble() * 10 - 5);
                float midY = (startY + endY) / 2 + (float)(_rnd.NextDouble() * 10 - 5);
                
                path.CurveTo(
                    (startX + midX) / 2, (startY + midY) / 2,
                    (midX + endX) / 2, (midY + endY) / 2,
                    endX, endY);
                    
                canvas.DrawPath(path);
                
                // Add some smaller branch crackles
                int branches = _rnd.Next(0, 2);
                for (int j = 0; j < branches; j++)
                {
                    // Pick a point along the main crackle
                    float t = (float)_rnd.NextDouble();
                    float branchX = startX + (midX - startX) * t;
                    float branchY = startY + (midY - startY) * t;
                    
                    // Branch at an angle
                    float branchAngle = angle + (float)(_rnd.NextDouble() * Math.PI / 2 - Math.PI / 4);
                    float branchLength = length * 0.3f * (float)_rnd.NextDouble();
                    
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
        
        private void DrawEdgeWear(ICanvas canvas, RectF dirtyRect)
        {
            // Edge wear is more pronounced at the corners
            int edgeWearCount = _rnd.Next(2, 4);
            
            for (int i = 0; i < edgeWearCount; i++)
            {
                float x, y, size;
                
                // Position along an edge or corner
                if (i == 0)
                {
                    // Force one corner to always have wear
                    int corner = _rnd.Next(4);
                    switch (corner)
                    {
                        case 0: // Top-left
                            x = (float)(_rnd.NextDouble() * dirtyRect.Width * 0.15);
                            y = (float)(_rnd.NextDouble() * dirtyRect.Height * 0.15);
                            break;
                        case 1: // Top-right
                            x = (float)(dirtyRect.Width - _rnd.NextDouble() * dirtyRect.Width * 0.15);
                            y = (float)(_rnd.NextDouble() * dirtyRect.Height * 0.15);
                            break;
                        case 2: // Bottom-right
                            x = (float)(dirtyRect.Width - _rnd.NextDouble() * dirtyRect.Width * 0.15);
                            y = (float)(dirtyRect.Height - _rnd.NextDouble() * dirtyRect.Height * 0.15);
                            break;
                        default: // Bottom-left
                            x = (float)(_rnd.NextDouble() * dirtyRect.Width * 0.15);
                            y = (float)(dirtyRect.Height - _rnd.NextDouble() * dirtyRect.Height * 0.15);
                            break;
                    }
                    size = (float)(_rnd.NextDouble() * dirtyRect.Width * 0.12) + dirtyRect.Width * 0.08f;
                }
                else
                {
                    // Random edge or corner wear
                    if (_rnd.NextDouble() < 0.6)
                    {
                        // Edge wear
                        int edge = _rnd.Next(4);
                        switch (edge)
                        {
                            case 0: // Top
                                x = (float)(_rnd.NextDouble() * dirtyRect.Width);
                                y = (float)(_rnd.NextDouble() * dirtyRect.Height * 0.1);
                                break;
                            case 1: // Right
                                x = (float)(dirtyRect.Width - _rnd.NextDouble() * dirtyRect.Width * 0.1);
                                y = (float)(_rnd.NextDouble() * dirtyRect.Height);
                                break;
                            case 2: // Bottom
                                x = (float)(_rnd.NextDouble() * dirtyRect.Width);
                                y = (float)(dirtyRect.Height - _rnd.NextDouble() * dirtyRect.Height * 0.1);
                                break;
                            default: // Left
                                x = (float)(_rnd.NextDouble() * dirtyRect.Width * 0.1);
                                y = (float)(_rnd.NextDouble() * dirtyRect.Height);
                                break;
                        }
                        size = (float)(_rnd.NextDouble() * dirtyRect.Width * 0.1) + dirtyRect.Width * 0.05f;
                    }
                    else
                    {
                        // Corner wear
                        int corner = _rnd.Next(4);
                        switch (corner)
                        {
                            case 0: // Top-left
                                x = (float)(_rnd.NextDouble() * dirtyRect.Width * 0.15);
                                y = (float)(_rnd.NextDouble() * dirtyRect.Height * 0.15);
                                break;
                            case 1: // Top-right
                                x = (float)(dirtyRect.Width - _rnd.NextDouble() * dirtyRect.Width * 0.15);
                                y = (float)(_rnd.NextDouble() * dirtyRect.Height * 0.15);
                                break;
                            case 2: // Bottom-right
                                x = (float)(dirtyRect.Width - _rnd.NextDouble() * dirtyRect.Width * 0.15);
                                y = (float)(dirtyRect.Height - _rnd.NextDouble() * dirtyRect.Height * 0.15);
                                break;
                            default: // Bottom-left
                                x = (float)(_rnd.NextDouble() * dirtyRect.Width * 0.15);
                                y = (float)(dirtyRect.Height - _rnd.NextDouble() * dirtyRect.Height * 0.15);
                                break;
                        }
                        size = (float)(_rnd.NextDouble() * dirtyRect.Width * 0.08) + dirtyRect.Width * 0.05f;
                    }
                }
                
                // Base material showing through
                byte r = (byte)_rnd.Next(100, 140);
                byte g = (byte)_rnd.Next(60, 100);
                byte b = (byte)_rnd.Next(20, 60);
                byte a = (byte)_rnd.Next(100, 220);
                
                canvas.FillColor = Color.FromRgba(r, g, b, a);
                
                // Create an irregular polygon for the wear patch
                PathF path = new PathF();
                int points = _rnd.Next(5, 8);
                float[] angles = new float[points];
                float[] distances = new float[points];
                
                // Generate random angles and sort them for a convex shape
                for (int j = 0; j < points; j++)
                {
                    angles[j] = (float)(_rnd.NextDouble() * Math.PI * 2);
                }
                Array.Sort(angles);
                
                // Generate random distances from center
                for (int j = 0; j < points; j++)
                {
                    distances[j] = (float)(0.7 + _rnd.NextDouble() * 0.6) * size;
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
            }
        }
    }
}