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

namespace SlidingTiles
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private int gridSize = 4; // Default size
        private Button?[,] tiles;
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
            tiles = new Button?[gridSize, gridSize];
            
            // Set default picker selection to 4×4
            SizePicker.SelectedIndex = 1; // Index 1 corresponds to 4×4
            
            // Set the board's aging effects drawable
            BoardAgeEffects.Drawable = TileTextureGenerator.CreateAgedBoardDrawable();
            
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

        private void SizePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the new grid size from picker
            int newSize = SizePicker.SelectedIndex + 3; // 3×3, 4×4, 5×5, 6×6
            
            // If size has changed, reinitialize the game
            if (newSize != gridSize)
            {
                gridSize = newSize;
                tiles = new Button?[gridSize, gridSize];
                InitializeTiles();
                StatusMessage = $"New {gridSize}×{gridSize} game ready! Click 'Shuffle' to start.";
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
                            Button tile = CreateTile(tileNumber.ToString(), tileSize);
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

        private Button CreateTile(string text, int tileSize)
        {
            // Increase the minimum size for better visibility
            int effectiveTileSize = Math.Max(60, tileSize);
            
            // Calculate font size based on grid size
            double fontSize = Math.Max(16, 28 - (gridSize - 3) * 3);
            
            // Create the base tile with standard properties
            var tile = new Button
            {
                Text = text, // Keep the text property for win checking
                WidthRequest = effectiveTileSize,
                HeightRequest = effectiveTileSize,
                BackgroundColor = Color.FromArgb("#FFD700"), // Default gold color that will be varied
                FontSize = fontSize,
                TextColor = Color.FromArgb("#B22222"), // Default red text that will be varied
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(2),
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

            // Apply our aging effects using the TileTextureGenerator service
            TileTextureGenerator.ApplyAgingEffects(tile, text, gridSize);
            
            // Visual states for enhanced 3D appearance when pressed - using the actual current BackgroundColor
            // which has been modified by the TileTextureGenerator
            VisualStateManager.SetVisualStateGroups(tile, new VisualStateGroupList
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
                                new Setter { Property = Button.BackgroundColorProperty, Value = tile.BackgroundColor.WithLuminosity(0.9f) }, // Darker when pressed, using float
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
            
            // Register click event for the button
            tile.Clicked += Tile_Clicked;
            
            return tile;
        }

        private async void Tile_Clicked(object? sender, EventArgs e)
        {
            if (sender is not Button clickedTile)
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
                    Button tileButton = tiles[row, col]!;
                    
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
                    
                    // Check the text property directly, which we've kept for win checking
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
}
