using System;
using System.Collections.Generic;

namespace SlidingTiles.Models
{
    public class GameBoard
    {
        private readonly int size;
        private readonly Tile[,] tiles;
        private int emptyTileRow;
        private int emptyTileColumn;
        private readonly Random random = new();

        public GameBoard(int size = 4)
        {
            this.size = size;
            tiles = new Tile[size, size];
            InitializeBoard();
        }

        public Tile[,] Tiles => tiles;
        public int Size => size;

        private void InitializeBoard()
        {
            int tileNumber = 1;
            
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (row == size - 1 && col == size - 1)
                    {
                        // Last position is empty (0)
                        tiles[row, col] = new Tile(0, row, col);
                        emptyTileRow = row;
                        emptyTileColumn = col;
                    }
                    else
                    {
                        tiles[row, col] = new Tile(tileNumber++, row, col);
                    }
                }
            }
        }

        public bool MoveTile(int row, int column)
        {
            if (IsAdjacent(row, column, emptyTileRow, emptyTileColumn))
            {
                // Swap the tiles
                var clickedTile = tiles[row, column];
                tiles[row, column] = tiles[emptyTileRow, emptyTileColumn];
                tiles[emptyTileRow, emptyTileColumn] = clickedTile;

                // Update positions
                clickedTile.Row = emptyTileRow;
                clickedTile.Column = emptyTileColumn;
                tiles[row, column].Row = row;
                tiles[row, column].Column = column;

                // Update empty position
                emptyTileRow = row;
                emptyTileColumn = column;

                return true;
            }
            return false;
        }

        private bool IsAdjacent(int row1, int col1, int row2, int col2)
        {
            return (Math.Abs(row1 - row2) == 1 && col1 == col2) || 
                   (Math.Abs(col1 - col2) == 1 && row1 == row2);
        }

        public void Shuffle(int moves = 100)
        {
            for (int i = 0; i < moves; i++)
            {
                // Get possible moves
                List<(int row, int col)> possibleMoves = new();
                
                // Check all four directions
                int[,] directions = { {-1, 0}, {1, 0}, {0, -1}, {0, 1} };
                
                for (int d = 0; d < 4; d++)
                {
                    int newRow = emptyTileRow + directions[d, 0];
                    int newCol = emptyTileColumn + directions[d, 1];
                    
                    if (newRow >= 0 && newRow < size && newCol >= 0 && newCol < size)
                    {
                        possibleMoves.Add((newRow, newCol));
                    }
                }
                
                // Pick a random valid move
                if (possibleMoves.Count > 0)
                {
                    var move = possibleMoves[random.Next(possibleMoves.Count)];
                    MoveTile(move.row, move.col);
                }
            }
        }

        public bool CheckWin()
        {
            int expectedNumber = 1;
            
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    // The last tile should be empty (0)
                    if (row == size - 1 && col == size - 1)
                    {
                        if (tiles[row, col].Number != 0)
                            return false;
                    }
                    else if (tiles[row, col].Number != expectedNumber++)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
    }
}