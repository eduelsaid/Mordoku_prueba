namespace Murdoku
{
    public class MurdokuSuspect
    {
        public string Name { get; }
        public bool IsVictim { get; set; }
        public MurdokuClue Clue { get; set; }
        public GridPosition? PlacedPosition { get; set; }

        public MurdokuSuspect(string name)
        {
            Name = name;
        }
    }
}
