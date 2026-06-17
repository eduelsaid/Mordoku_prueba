using System.Collections.Generic;

namespace Murdoku
{
    public class MurdokuPuzzle
    {
        public int Size { get; }
        public MurdokuCell[,] Grid { get; }
        public List<MurdokuSuspect> Suspects { get; }
        public Dictionary<string, GridPosition> Solution { get; } = new();
        public string MurdererName { get; set; }
        public string VictimName { get; set; }

        public MurdokuPuzzle(int size, MurdokuCell[,] grid, List<MurdokuSuspect> suspects)
        {
            Size = size;
            Grid = grid;
            Suspects = suspects;
        }

        public MurdokuCell GetCell(int row, int col) => Grid[row, col];

        public MurdokuCell GetCell(GridPosition pos) => Grid[pos.Row, pos.Col];

        public MurdokuSuspect GetSuspectAt(GridPosition pos)
        {
            foreach (var suspect in Suspects)
            {
                if (suspect.PlacedPosition.HasValue && suspect.PlacedPosition.Value.Equals(pos))
                    return suspect;
            }

            return null;
        }

        public bool IsRowOccupied(int row, MurdokuSuspect except = null)
        {
            foreach (var suspect in Suspects)
            {
                if (suspect == except || !suspect.PlacedPosition.HasValue)
                    continue;
                if (suspect.PlacedPosition.Value.Row == row)
                    return true;
            }

            return false;
        }

        public bool IsColOccupied(int col, MurdokuSuspect except = null)
        {
            foreach (var suspect in Suspects)
            {
                if (suspect == except || !suspect.PlacedPosition.HasValue)
                    continue;
                if (suspect.PlacedPosition.Value.Col == col)
                    return true;
            }

            return false;
        }

        public int CountSuspectsInRoom(RoomType room)
        {
            var count = 0;
            foreach (var suspect in Suspects)
            {
                if (!suspect.PlacedPosition.HasValue)
                    continue;
                if (GetCell(suspect.PlacedPosition.Value).Room == room)
                    count++;
            }

            return count;
        }
    }
}
