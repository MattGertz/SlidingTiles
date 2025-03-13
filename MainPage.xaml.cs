using SlidingTiles.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlidingTiles
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private GameBoard gameBoard;
        private Dictionary<int, Button> tileButtons;
        private string statusMessage = "Tap tiles to move them";
        
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                statusMessage = value;
                OnPropertyChanged();
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            
            tileButtons = new Dictionary<int, Button>();
            gameBoard = new GameBoard();
            
            CreateTileButtons();
            RefreshGameBoard();
        }

        private void CreateTileButtons()
        {
            GameGrid.Children.Clear();
            tileButtons.Clear();

            for (int row = 0; row < gameBoard.Size; row++)
            {
                for (int col = 0; col < gameBoard.Size; col++)
                {
                    var button = new Button
                    {
                        WidthRequest = 80,
                        HeightRequest = 80,
                        CornerRadius = 0,
                        FontSize = 24,
                        FontAttributes = FontAttributes.Bold
                    };
                    
                    button.Clicked += TileButton_Clicked;
                    
                    GameGrid.Add(button, col, row);
                    tileButtons.Add(row * gameBoard.Size + col, button);
                }
            }
        }

        private void RefreshGameBoard()
        {
            foreach (var tile in gameBoard.Tiles)
            {
                int gridPosition = tile.Row * gameBoard.Size + tile.Column;
                
                if (tileButtons.TryGetValue(gridPosition, out Button button))
                {
                    if (tile.IsEmpty)
                    {
                        button.Text = string.Empty;
                        button.IsVisible = false;
                    }
                    else
                    {
                        button.Text = tile.Number.ToString();
                        button.IsVisible = true;
                    }
                    
                    // Store the tile number as CommandParameter for easy retrieval
                    button.CommandParameter = tile.Number;
                }
            }
            
            if (gameBoard.IsSolved())
            {
                StatusMessage = "Puzzle Solved! Congratulations!";
            }
        }

        private void TileButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int number)
            {
                var tile = gameBoard.Tiles.FirstOrDefault(t => t.Number == number);
                
                if (tile != null)
                {
                    if (gameBoard.MoveTile(tile))
                    {
                        RefreshGameBoard();
                    }
                }
            }
        }

        private void NewGame_Clicked(object sender, EventArgs e)
        {
            gameBoard = new GameBoard();
            gameBoard.ShuffleTiles();
            RefreshGameBoard();
            StatusMessage = "New game started";
        }

        private void Shuffle_Clicked(object sender, EventArgs e)
        {
            gameBoard.ShuffleTiles();
            RefreshGameBoard();
            StatusMessage = "Puzzle shuffled";
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
