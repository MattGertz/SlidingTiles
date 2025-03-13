namespace SlidingTiles.Models
{
    public class Tile
    {
        public int Number { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsEmpty => Number == 0;

        public Tile(int number, int row, int column)
        {
            Number = number;
            Row = row;
            Column = column;
        }
    }
}
