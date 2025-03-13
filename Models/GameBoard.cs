using System.Collections.ObjectModel;

namespace SlidingTiles.Models
{
    public class GameBoard
    {
        public ObservableCollection<Tile> Tiles { get; private set; }
        public int Size { get; private set; }
        private Tile emptyTile;

        public GameBoard(int size = 4)
        {
            Size = size;
            Tiles = new ObservableCollection<Tile>();
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            Tiles.Clear();
            
            // Create tiles with proper numbers
            int counter = 1;
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    // Last tile is empty (0)
                    int number = (row == Size - 1 && col == Size - 1) ? 0 : counter++;
                    var tile = new Tile(number, row, col);
                    Tiles.Add(tile);
                    
                    if (tile.IsEmpty)
                        emptyTile = tile;
                }
            }
        }

        public void ShuffleTiles(int moves = 100)
        {
            Random random = new Random();
            
            // Make random valid moves to shuffle the board
            for (int i = 0; i < moves; i++)
            {
                var adjacentTiles = GetAdjacentTiles(emptyTile);
                if (adjacentTiles.Count > 0)
                {
                    int randomIndex = random.Next(adjacentTiles.Count);
                    MoveTile(adjacentTiles[randomIndex]);
                }
            }
        }

        public bool MoveTile(Tile tile)
        {
            // Check if this tile is adjacent to the empty one
            if (IsAdjacent(tile, emptyTile))
            {
                // Swap positions
                int tempRow = tile.Row;
                int tempCol = tile.Column;
                
                tile.Row = emptyTile.Row;
                tile.Column = emptyTile.Column;
                
                emptyTile.Row = tempRow;
                emptyTile.Column = tempCol;
                
                // Update tiles in collection for UI binding
                var index1 = Tiles.IndexOf(tile);
                var index2 = Tiles.IndexOf(emptyTile);
                
                Tiles.Move(index1, index2);
                
                return true;
            }
            
            return false;
        }

        private bool IsAdjacent(Tile tile1, Tile tile2)
        {
            // Check if tiles are adjacent (horizontally or vertically)
            return (Math.Abs(tile1.Row - tile2.Row) == 1 && tile1.Column == tile2.Column) || 
                   (Math.Abs(tile1.Column - tile2.Column) == 1 && tile1.Row == tile2.Row);
        }

        private List<Tile> GetAdjacentTiles(Tile tile)
        {
            List<Tile> adjacentTiles = new List<Tile>();
            
            foreach (var t in Tiles)
            {
                if (IsAdjacent(t, tile))
                    adjacentTiles.Add(t);
            }
            
            return adjacentTiles;
        }

        public bool IsSolved()
        {
            int counter = 1;
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    Tile tile = Tiles.FirstOrDefault(t => t.Row == row && t.Column == col);
                    
                    // Last position should be empty
                    if (row == Size - 1 && col == Size - 1)
                    {
                        if (!tile.IsEmpty)
                            return false;
                    }
                    // Otherwise check for correct number
                    else if (tile.Number != counter++)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
    }
}
