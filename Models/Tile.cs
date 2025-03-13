namespace SlidingTiles.Models
{
    public class Tile
    {
        public int Number { get; set; }
        public bool IsEmpty { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }

        public Tile(int number, int row, int column)
        {
            Number = number;
            IsEmpty = number == 0;
            Row = row;
            Column = column;
        }
    }
}
