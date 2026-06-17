namespace Murdoku
{
    public class MurdokuClue
    {
        public ClueType Type { get; }
        public RoomType? Room { get; }
        public FurnitureType? Furniture { get; }
        public string Text { get; }

        public MurdokuClue(ClueType type, string text, RoomType? room = null, FurnitureType? furniture = null)
        {
            Type = type;
            Text = text;
            Room = room;
            Furniture = furniture;
        }
    }
}
