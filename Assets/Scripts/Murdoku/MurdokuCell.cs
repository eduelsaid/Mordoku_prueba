namespace Murdoku
{
    public class MurdokuCell
    {
        public int Row { get; }
        public int Col { get; }
        public RoomType Room { get; set; }
        public FurnitureType Furniture { get; set; } = FurnitureType.Suelo;

        public bool IsRoomCorner { get; set; }
        public bool IsBesideWall { get; set; }

        public MurdokuCell(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public GridPosition Position => new(Row, Col);
    }
}
