using System;
using System.Collections.Generic;
using System.Linq;

namespace Murdoku
{
    public static class ClueEvaluator
    {
        public static bool Matches(MurdokuPuzzle puzzle, MurdokuSuspect suspect, GridPosition pos)
        {
            if (suspect.Clue == null)
                return true;

            var cell = puzzle.GetCell(pos);
            var clue = suspect.Clue;

            switch (clue.Type)
            {
                case ClueType.EnHabitacion:
                    return clue.Room.HasValue && cell.Room == clue.Room.Value;

                case ClueType.SobreMueble:
                    return clue.Furniture.HasValue && cell.Furniture == clue.Furniture.Value;

                case ClueType.JuntoAMueble:
                    return clue.Furniture.HasValue && IsBesideFurniture(puzzle, pos, clue.Furniture.Value);

                case ClueType.DelanteDeVentana:
                    return cell.Furniture == FurnitureType.Ventana ||
                           IsBesideFurniture(puzzle, pos, FurnitureType.Ventana);

                case ClueType.SentadoEnSilla:
                    return cell.Furniture == FurnitureType.Silla;

                case ClueType.EnEsquina:
                    return cell.IsRoomCorner;

                case ClueType.NoJuntoAPared:
                    return !cell.IsBesideWall;

                case ClueType.MismaFilaQue:
                    return clue.Furniture.HasValue && HasFurnitureInRow(puzzle, pos.Row, clue.Furniture.Value);

                case ClueType.VictimaUltimaCasilla:
                    return true;

                default:
                    return true;
            }
        }

        public static bool IsBesideFurniture(MurdokuPuzzle puzzle, GridPosition pos, FurnitureType furniture)
        {
            foreach (var neighbor in GetOrthogonalNeighbors(puzzle, pos))
            {
                var neighborCell = puzzle.GetCell(neighbor);
                if (neighborCell.Room == puzzle.GetCell(pos).Room && neighborCell.Furniture == furniture)
                    return true;
            }

            return false;
        }

        public static bool HasFurnitureInRow(MurdokuPuzzle puzzle, int row, FurnitureType furniture)
        {
            for (var col = 0; col < puzzle.Size; col++)
            {
                if (puzzle.GetCell(row, col).Furniture == furniture)
                    return true;
            }

            return false;
        }

        public static IEnumerable<GridPosition> GetOrthogonalNeighbors(MurdokuPuzzle puzzle, GridPosition pos)
        {
            var deltas = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            foreach (var (dr, dc) in deltas)
            {
                var r = pos.Row + dr;
                var c = pos.Col + dc;
                if (r >= 0 && r < puzzle.Size && c >= 0 && c < puzzle.Size)
                    yield return new GridPosition(r, c);
            }
        }
    }
}
