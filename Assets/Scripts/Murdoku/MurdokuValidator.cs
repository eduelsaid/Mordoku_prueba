using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Murdoku
{
    public static class MurdokuValidator
    {
        public static bool TryValidatePlacement(MurdokuPuzzle puzzle, MurdokuSuspect suspect, GridPosition pos, out string error)
        {
            if (puzzle.IsRowOccupied(pos.Row, suspect))
            {
                error = "Ya hay un sospechoso en esa fila.";
                return false;
            }

            if (puzzle.IsColOccupied(pos.Col, suspect))
            {
                error = "Ya hay un sospechoso en esa columna.";
                return false;
            }

            if (!ClueEvaluator.Matches(puzzle, suspect, pos))
            {
                error = $"La pista de {suspect.Name} no coincide con esa casilla.";
                return false;
            }

            error = null;
            return true;
        }

        public static bool IsComplete(MurdokuPuzzle puzzle)
        {
            return puzzle.Suspects.All(s => s.PlacedPosition.HasValue);
        }

        public static bool IsSolved(MurdokuPuzzle puzzle, string accusedMurderer, out string message)
        {
            if (!IsComplete(puzzle))
            {
                message = "Coloca a todos los sospechosos primero.";
                return false;
            }

            foreach (var suspect in puzzle.Suspects)
            {
                if (!puzzle.Solution.TryGetValue(suspect.Name, out var expected))
                    continue;

                if (!suspect.PlacedPosition.HasValue || !suspect.PlacedPosition.Value.Equals(expected))
                {
                    message = "Las posiciones no coinciden con la solución.";
                    return false;
                }

                if (!ClueEvaluator.Matches(puzzle, suspect, suspect.PlacedPosition.Value))
                {
                    message = $"La pista de {suspect.Name} no se cumple.";
                    return false;
                }
            }

            if (string.IsNullOrEmpty(accusedMurderer))
            {
                message = "Selecciona al asesino.";
                return false;
            }

            if (accusedMurderer != puzzle.MurdererName)
            {
                message = $"Incorrecto. El asesino era {puzzle.MurdererName}.";
                return false;
            }

            message = $"¡Caso resuelto! {puzzle.MurdererName} era el asesino.";
            return true;
        }

        public static string BuildHint(MurdokuPuzzle puzzle)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Reglas Murdoku:");
            sb.AppendLine("• Un sospechoso por fila y por columna.");
            sb.AppendLine("• La víctima queda en la última casilla libre.");
            sb.AppendLine("• El asesino estaba a solas con la víctima en la misma habitación.");
            sb.AppendLine();
            sb.AppendLine($"Víctima: {puzzle.VictimName}");
            return sb.ToString();
        }
    }
}
