using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading.Tasks;
using SlidingTiles.Models;
using SlidingTiles.Extensions;
using SlidingTiles.Services;
using SlidingTiles.Controls;
using Microsoft.Maui.Graphics;

namespace SlidingTiles
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private int gridSize = 4; // Default size
        private TexturedTile?[,] tiles;
        private int emptyRow, emptyCol;
        private Random random = new Random();
        private string statusMessage = "Start a new game!";
        private const int MinTileSize = 40; // Minimum tile size in pixels

        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                if (statusMessage != value)
                {
                    statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            
            // Initialize with default grid size
            tiles = new TexturedTile?[gridSize, gridSize];
            
            // Initialize the custom TexturedPicker with grid size options
            SizePicker.Items = new List<string> { "3×3", "4×4", "5×5", "6×6" };
            SizePicker.SelectedIndex = 1; // Index 1 corresponds to 4×4
            SizePicker.SelectedIndexChanged += SizePicker_SelectedIndexChanged;
            
            // Set the board's aging effects drawable
            BoardAgeEffects.Drawable = TileTextureGenerator.CreateAgedBoardDrawable();
            
            // Apply gold leaf texture to the Shuffle button
            ShuffleButtonTexture.Drawable = new ShuffleButtonDrawable();
            
            // Apply text effects to the Shuffle button to match the tile style
            ApplyShuffleButtonEffects();
            
            InitializeTiles();
            
            // Automatically shuffle the board on startup using fire-and-forget pattern
            // Since we can't use async directly in constructors
            _ = Task.Run(async () => 
            {
                // Small delay to ensure UI is fully loaded before animation starts
                await Task.Delay(100);
                await Dispatcher.DispatchAsync(async () => await ShuffleBoard());
            });
        }

        /// <summary>
        /// Applies text effects to the Shuffle button to match the tile aesthetic
        /// </summary>
        private void ApplyShuffleButtonEffects()
        {
            // Create consistent randomness for the button effects
            Random rnd = new Random(42); // Fixed seed for consistent appearance
            
            // Apply pressed-state visual effects to the button
            VisualStateManager.SetVisualStateGroups(ShuffleButton, new VisualStateGroupList
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

            // Vary text color to simulate faded or oxidized engraving (like in TexturedTile)
            byte textR = (byte)Math.Clamp(178 + rnd.Next(-40, 20), 138, 198);
            byte textG = (byte)Math.Clamp(34 + rnd.Next(-20, 30), 14, 64);
            byte textB = (byte)Math.Clamp(34 + rnd.Next(-20, 10), 14, 44);
            
            ShuffleButton.TextColor = Color.FromRgba(textR, textG, textB, (byte)255);

            // Add a more pronounced text shadow effect for better visibility
            ShuffleButton.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromRgba(0, 0, 0, 180)),
                Offset = new Point(1.5, 1.5),
                Radius = 3,
                Opacity = 0.7f
            };

            // Add subtle shadow offset for carved text appearance
            var shadowOffset = new Size(
                -1 + (rnd.NextDouble() * 0.4 - 0.2), 
                -1 + (rnd.NextDouble() * 0.4 - 0.2));

            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowColor(
                ShuffleButton, 
                Color.FromRgba((byte)139, (byte)0, (byte)0, (byte)rnd.Next(140, 200)));
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowOffset(
                ShuffleButton, 
                shadowOffset);
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowRadius(
                ShuffleButton, 
                (float)(rnd.NextDouble() * 0.5));
                
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.SetShadowOpacity(
                ShuffleButton, 
                (float)(0.6 + rnd.NextDouble() * 0.2));
        }

        private async void SizePicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Get the new grid size from picker
            int newSize = SizePicker.SelectedIndex + 3; // 3×3, 4×4, 5×5, 6×6
            
            // If size has changed, reinitialize the game
            if (newSize != gridSize)
            {
                gridSize = newSize;
                tiles = new TexturedTile?[gridSize, gridSize];
                InitializeTiles();
                StatusMessage = $"New {gridSize}×{gridSize} game. Shuffling tiles...";
                
                // Automatically shuffle the board when size changes
                await ShuffleBoard();
            }
        }

        private void InitializeTiles()
        {
            try
            {
                if (GameGrid == null)
                {
                    Debug.WriteLine("ERROR: GameGrid is null in InitializeTiles");
                    return;
                }

                // Clear any existing tiles
                GameGrid.Clear();
                
                // Reset grid definitions
                GameGrid.RowDefinitions.Clear();
                GameGrid.ColumnDefinitions.Clear();
                
                // Create new row and column definitions based on gridSize
                for (int i = 0; i < gridSize; i++)
                {
                    GameGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                    GameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                }

                // Calculate tile size based on grid size - increased base size
                int tileSize = Math.Max(60, 240 / gridSize);

                // Create new tiles
                for (int row = 0; row < gridSize; row++)
                {
                    for (int col = 0; col < gridSize; col++)
                    {
                        int tileNumber = row * gridSize + col + 1;
                        if (tileNumber < gridSize * gridSize)
                        {
                            TexturedTile tile = CreateTile(tileNumber.ToString(), tileSize);
                            tiles[row, col] = tile;
                            GameGrid.Add(tile, col, row);
                        }
                        else
                        {
                            // This is the empty spot
                            tiles[row, col] = null;
                            emptyRow = row;
                            emptyCol = col;
                        }
                    }
                }

                StatusMessage = $"Game ready! Click 'Shuffle' to start.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in InitializeTiles: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private TexturedTile CreateTile(string text, int tileSize)
        {
            // Increase the minimum size for better visibility
            int effectiveTileSize = Math.Max(60, tileSize);
            
            // Create our custom TexturedTile that includes the gold leaf texture with wear patterns
            var tile = new TexturedTile(text, effectiveTileSize, gridSize);
            
            // Register click event for the tile
            tile.Clicked += Tile_Clicked;
            
            return tile;
        }

        private async void Tile_Clicked(object? sender, EventArgs e)
        {
            if (sender is not TexturedTile clickedTile)
                return;

            int row = -1, col = -1;

            // Find the clicked tile position
            for (int r = 0; r < gridSize; r++)
            {
                for (int c = 0; c < gridSize; c++)
                {
                    if (tiles[r, c] == clickedTile)
                    {
                        row = r;
                        col = c;
                        break;
                    }
                }
                if (row != -1) break;
            }

            // Check if the clicked tile is adjacent to the empty space
            if (IsAdjacent(row, col, emptyRow, emptyCol))
            {
                // Use the animated version for user interactions
                await MoveTileAsync(row, col);
                CheckWin();
            }
        }

        private bool IsAdjacent(int row1, int col1, int row2, int col2)
        {
            return (Math.Abs(row1 - row2) == 1 && col1 == col2) || 
                   (Math.Abs(col1 - col2) == 1 && row1 == row2);
        }

        private async Task MoveTileAsync(int row, int col)
        {
            try 
            {
                // Move the tile to empty position
                if (tiles[row, col] != null && GameGrid != null)
                {
                    TexturedTile tileButton = tiles[row, col]!;
                    
                    // Calculate tile size to use for animation
                    double tileSize = tileButton.Width > 0 ? tileButton.Width : Math.Max(60, 240 / gridSize);
                    
                    // Animate the tile movement
                    await tileButton.SlideToPositionAsync(emptyRow, emptyCol, tileSize);
                    
                    // Update the tiles array after animation completes
                    tiles[emptyRow, emptyCol] = tiles[row, col];
                    tiles[row, col] = null;

                    // Update the empty position
                    emptyRow = row;
                    emptyCol = col;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MoveTileAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void MoveTile(int row, int col)
        {
            try 
            {
                // Move the tile to empty position without animation (used for shuffling)
                if (tiles[row, col] != null && GameGrid != null)
                {
                    GameGrid.Remove(tiles[row, col]);
                    GameGrid.Add(tiles[row, col], emptyCol, emptyRow);

                    // Update the tiles array
                    tiles[emptyRow, emptyCol] = tiles[row, col];
                    tiles[row, col] = null;

                    // Update the empty position
                    emptyRow = row;
                    emptyCol = col;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MoveTile: {ex.Message}");
            }
        }

        private void CheckWin()
        {
            bool win = true;
            
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    int expectedNumber = row * gridSize + col + 1;
                    
                    // Skip the empty space
                    if (expectedNumber == gridSize * gridSize)
                        continue;
                    
                    // Check if tile matches expected number
                    if (tiles[row, col] == null)
                    {
                        win = false;
                        break;
                    }
                    
                    // Check the text property
                    if (tiles[row, col]!.Text != expectedNumber.ToString())
                    {
                        win = false;
                        break;
                    }
                }
                if (!win) break;
            }

            if (win)
            {
                StatusMessage = "Congratulations! You win!";
                DisplayAlert("Victory!", $"You solved the {gridSize}×{gridSize} puzzle!", "OK");
            }
        }

        private async void NewGame_Clicked(object sender, EventArgs e)
        {
            InitializeTiles();
            await ShuffleBoard();
        }

        private async Task ShuffleBoard()
        {
            // Adjust number of moves based on grid size for better shuffling
            int moves = gridSize * gridSize * 5;
            
            StatusMessage = "Shuffling...";
            
            // Perform random valid moves to shuffle
            for (int i = 0; i < moves; i++)
            {
                // Get possible moves
                List<(int row, int col)> possibleMoves = new();
                
                // Check all four directions
                int[,] directions = { {-1, 0}, {1, 0}, {0, -1}, {0, 1} };
                
                for (int d = 0; d < 4; d++)
                {
                    int newRow = emptyRow + directions[d, 0];
                    int newCol = emptyCol + directions[d, 1];
                    
                    if (newRow >= 0 && newRow < gridSize && newCol >= 0 && newCol < gridSize)
                    {
                        possibleMoves.Add((newRow, newCol));
                    }
                }
                
                // Pick a random valid move
                if (possibleMoves.Count > 0)
                {
                    var move = possibleMoves[random.Next(possibleMoves.Count)];
                    
                    // Use animation only for the last move to give visual feedback
                    if (i == moves - 1)
                    {
                        await MoveTileAsync(move.row, move.col);
                    }
                    else
                    {
                        // Use the fast non-animated version for the rest
                        MoveTile(move.row, move.col);
                    }
                }
            }

            StatusMessage = "Puzzle shuffled! Make your moves.";
        }

        private async void Shuffle_Clicked(object sender, EventArgs e)
        {
            // Disable the button while shuffling to prevent multiple clicks
            Button? shuffleButton = null;
            if (sender is Button button)
            {
                shuffleButton = button;
                shuffleButton.IsEnabled = false;
            }
                
            await ShuffleBoard();
            
            // Re-enable the button
            if (shuffleButton != null)
            {
                shuffleButton.IsEnabled = true;
            }
        }

        // INotifyPropertyChanged implementation
        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Drawable for the Shuffle button that renders a gold leaf appearance with wear patterns
    /// to match the tile aesthetic
    /// </summary>
    internal class ShuffleButtonDrawable : IDrawable
    {
        private readonly Color _baseGold;
        private readonly Random _rnd;
        
        public ShuffleButtonDrawable()
        {
            _baseGold = Color.FromRgba((byte)255, (byte)215, (byte)0, (byte)255); // Standard gold color
            _rnd = new Random(99); // Fixed seed for consistent appearance
        }
        
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Draw base gold background
            canvas.FillColor = _baseGold;
            canvas.FillRectangle(dirtyRect);
            
            // Calculate appropriate settings based on button size
            int speckCount = 40 + _rnd.Next(-10, 20); // Number of specks/dots
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
            int lineCount = 5 + _rnd.Next(-2, 3); // Number of major crackle lines
            
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
                    // Start from somewhere in the button
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
