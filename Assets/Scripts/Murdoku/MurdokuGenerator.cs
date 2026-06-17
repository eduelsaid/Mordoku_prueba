using System;
using System.Collections.Generic;
using System.Linq;

namespace Murdoku
{
    public class MurdokuGenerator
    {
        private readonly Random _rng;
        private readonly string[] _namePool =
        {
            "Axel", "Bella", "Cora", "Ella", "Vincent", "Arianna", "Brycen", "Colleen",
            "Dan", "Evan", "Alexander", "Briggs", "Carissa", "Diana", "Elsa", "Antonio",
            "Ben", "Chelsea", "Dahlia", "Emmy", "Haylee", "Bianca", "Elijah"
        };

        public MurdokuGenerator(int? seed = null)
        {
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public MurdokuPuzzle Generate(int size = 4, RoomType[] roomLayout = null)
        {
            if (size < 3 || size > 8)
                throw new ArgumentException("El tamaño del tablero debe estar entre 3 y 8.");

            for (var attempt = 0; attempt < 50; attempt++)
            {
                var rooms = roomLayout ?? new RoomLayoutGenerator(_rng).Generate(size, size);
                if (rooms.Length != size * size)
                    continue;

                var puzzle = TryBuildPuzzle(size, rooms);
                if (puzzle != null)
                    return puzzle;
            }

            throw new InvalidOperationException($"No se pudo generar un caso válido de {size}x{size}.");
        }

        private MurdokuPuzzle TryBuildPuzzle(int size, RoomType[] rooms)
        {
            var grid = BuildGrid(size, rooms);
            PlaceFurniture(grid, size);
            ComputeWallFlags(grid, size);

            var suspects = CreateSuspects(size);
            var puzzle = new MurdokuPuzzle(size, grid, suspects);

            if (!TryAssignSolution(puzzle, out var murderer, out var victim))
                return null;

            puzzle.MurdererName = murderer.Name;
            puzzle.VictimName = victim.Name;
            victim.IsVictim = true;

            foreach (var suspect in suspects)
            {
                var pos = suspect.PlacedPosition!.Value;
                puzzle.Solution[suspect.Name] = pos;
                suspect.Clue = BuildClue(puzzle, suspect, pos);
                suspect.PlacedPosition = null;
            }

            return puzzle;
        }

        private MurdokuCell[,] BuildGrid(int size, RoomType[] rooms)
        {
            var grid = new MurdokuCell[size, size];
            var index = 0;
            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    var cell = new MurdokuCell(row, col) { Room = rooms[index++] };
                    grid[row, col] = cell;
                }
            }

            return grid;
        }

        private void PlaceFurniture(MurdokuCell[,] grid, int size)
        {
            var roomCells = new Dictionary<RoomType, List<MurdokuCell>>();
            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    var cell = grid[row, col];
                    if (!roomCells.ContainsKey(cell.Room))
                        roomCells[cell.Room] = new List<MurdokuCell>();
                    roomCells[cell.Room].Add(cell);
                }
            }

            foreach (var pair in roomCells)
            {
                var room = pair.Key;
                var cells = pair.Value;
                var allowed = RoomCatalog.AllowedFurniture[room]
                    .Where(f => f != FurnitureType.Suelo)
                    .ToList();

                Shuffle(cells);
                var furnitureCount = Math.Min(allowed.Count, Math.Max(1, cells.Count / 2));

                for (var i = 0; i < furnitureCount; i++)
                {
                    var furniture = allowed[i % allowed.Count];
                    cells[i].Furniture = furniture;
                }
            }
        }

        private void ComputeWallFlags(MurdokuCell[,] grid, int size)
        {
            var roomBounds = new Dictionary<RoomType, (int minR, int maxR, int minC, int maxC)>();
            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    var room = grid[row, col].Room;
                    if (!roomBounds.ContainsKey(room))
                        roomBounds[room] = (row, row, col, col);
                    else
                    {
                        var b = roomBounds[room];
                        roomBounds[room] = (
                            Math.Min(b.minR, row), Math.Max(b.maxR, row),
                            Math.Min(b.minC, col), Math.Max(b.maxC, col));
                    }
                }
            }

            foreach (var pair in roomBounds)
            {
                var room = pair.Key;
                var (minR, maxR, minC, maxC) = pair.Value;
                for (var row = minR; row <= maxR; row++)
                {
                    for (var col = minC; col <= maxC; col++)
                    {
                        if (grid[row, col].Room != room)
                            continue;

                        var cell = grid[row, col];
                        var onTop = row == minR;
                        var onBottom = row == maxR;
                        var onLeft = col == minC;
                        var onRight = col == maxC;
                        cell.IsBesideWall = onTop || onBottom || onLeft || onRight;
                        cell.IsRoomCorner = (onTop || onBottom) && (onLeft || onRight);
                    }
                }
            }
        }

        private List<MurdokuSuspect> CreateSuspects(int count)
        {
            var names = _namePool.OrderBy(_ => _rng.Next()).Take(count).ToList();
            return names.Select(n => new MurdokuSuspect(n)).ToList();
        }

        private bool TryAssignSolution(MurdokuPuzzle puzzle, out MurdokuSuspect murderer, out MurdokuSuspect victim)
        {
            murderer = null;
            victim = null;

            for (var attempt = 0; attempt < 200; attempt++)
            {
                var columns = Enumerable.Range(0, puzzle.Size).ToArray();
                Shuffle(columns);

                for (var i = 0; i < puzzle.Suspects.Count; i++)
                    puzzle.Suspects[i].PlacedPosition = new GridPosition(i, columns[i]);

                var victimCandidate = puzzle.Suspects[_rng.Next(puzzle.Suspects.Count)];
                var victimRoom = puzzle.GetCell(victimCandidate.PlacedPosition!.Value).Room;

                var alonePartners = puzzle.Suspects
                    .Where(s => s != victimCandidate)
                    .Where(s => puzzle.GetCell(s.PlacedPosition!.Value).Room == victimRoom)
                    .Where(s => puzzle.CountSuspectsInRoom(victimRoom) == 2)
                    .ToList();

                if (alonePartners.Count != 1)
                    continue;

                murderer = alonePartners[0];
                victim = victimCandidate;
                return true;
            }

            return false;
        }

        private MurdokuClue BuildClue(MurdokuPuzzle puzzle, MurdokuSuspect suspect, GridPosition pos)
        {
            if (suspect.IsVictim)
                return new MurdokuClue(ClueType.VictimaUltimaCasilla, "La víctima.\nEstaba en la última casilla libre.");

            var cell = puzzle.GetCell(pos);
            var options = new List<Func<MurdokuClue>>();

            options.Add(() => new MurdokuClue(
                ClueType.EnHabitacion,
                $"Estaba en el {RoomCatalog.RoomNames[cell.Room]}.",
                cell.Room));

            if (cell.Furniture != FurnitureType.Suelo)
            {
                var label = RoomCatalog.FurnitureLabels[cell.Furniture].ToLower();
                if (cell.Furniture == FurnitureType.Silla)
                    options.Add(() => new MurdokuClue(ClueType.SentadoEnSilla, "Estaba sentado en la silla."));
                else if (cell.Furniture == FurnitureType.Ventana)
                    options.Add(() => new MurdokuClue(ClueType.DelanteDeVentana, "Estaba delante de una ventana."));
                else if (cell.Furniture == FurnitureType.Alfombra)
                    options.Add(() => new MurdokuClue(ClueType.SobreMueble, "Estaba sobre la alfombra.", furniture: FurnitureType.Alfombra));
                else if (cell.Furniture == FurnitureType.Cama)
                    options.Add(() => new MurdokuClue(ClueType.SobreMueble, "Estaba sobre la cama.", furniture: FurnitureType.Cama));
                else
                    options.Add(() => new MurdokuClue(ClueType.SobreMueble, $"Estaba sobre {label}.", furniture: cell.Furniture));
            }

            foreach (FurnitureType furniture in Enum.GetValues(typeof(FurnitureType)))
            {
                if (furniture == FurnitureType.Suelo || furniture == cell.Furniture)
                    continue;
                if (!ClueEvaluator.IsBesideFurniture(puzzle, pos, furniture))
                    continue;

                var label = RoomCatalog.FurnitureLabels[furniture].ToLower();
                options.Add(() => new MurdokuClue(ClueType.JuntoAMueble, $"Estaba junto a {label}.", furniture: furniture));
            }

            if (cell.IsRoomCorner)
                options.Add(() => new MurdokuClue(ClueType.EnEsquina, "Estaba en una esquina."));

            if (!cell.IsBesideWall)
                options.Add(() => new MurdokuClue(ClueType.NoJuntoAPared, "No estaba junto a una pared."));

            foreach (FurnitureType furniture in Enum.GetValues(typeof(FurnitureType)))
            {
                if (furniture == FurnitureType.Suelo)
                    continue;
                if (!ClueEvaluator.HasFurnitureInRow(puzzle, pos.Row, furniture))
                    continue;
                if (cell.Furniture == furniture)
                    continue;

                var label = RoomCatalog.FurnitureLabels[furniture];
                options.Add(() => new MurdokuClue(ClueType.MismaFilaQue, $"Estaba en la misma fila que {label}.", furniture: furniture));
            }

            return options[_rng.Next(options.Count)]();
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
