using System;
using System.Collections.Generic;
using System.Linq;

namespace Murdoku
{
    /// <summary>
    /// Genera particiones aleatorias del tablero en habitaciones conectadas.
    /// Cada nueva partida obtiene una distribución distinta.
    /// </summary>
    public class RoomLayoutGenerator
    {
        private readonly Random _rng;

        public RoomLayoutGenerator(Random rng)
        {
            _rng = rng;
        }

        public RoomType[] Generate(int size, int roomCount)
        {
            if (roomCount < 1 || roomCount > size * size)
                throw new ArgumentException("Número de habitaciones inválido.");

            var cells = new List<(int row, int col)>();
            for (var row = 0; row < size; row++)
            for (var col = 0; col < size; col++)
                cells.Add((row, col));

            var partitions = PartitionCells(cells, roomCount);
            var roomTypes = PickRoomTypes(roomCount);

            var layout = new RoomType[size * size];
            for (var i = 0; i < partitions.Count; i++)
            {
                foreach (var (row, col) in partitions[i])
                    layout[row * size + col] = roomTypes[i];
            }

            return layout;
        }

        private List<List<(int row, int col)>> PartitionCells(List<(int row, int col)> cells, int parts)
        {
            if (parts == 1)
                return new List<List<(int, int)>> { cells };

            if (!TrySplit(cells, out var left, out var right))
                return SplitByBreadth(cells, parts);

            var leftCount = parts / 2;
            var rightCount = parts - leftCount;
            var result = new List<List<(int, int)>>();
            result.AddRange(PartitionCells(left, leftCount));
            result.AddRange(PartitionCells(right, rightCount));
            return result;
        }

        private bool TrySplit(List<(int row, int col)> cells, out List<(int row, int col)> left, out List<(int row, int col)> right)
        {
            left = new List<(int, int)>();
            right = new List<(int, int)>();

            if (cells.Count < 2)
                return false;

            var minRow = cells.Min(c => c.row);
            var maxRow = cells.Max(c => c.row);
            var minCol = cells.Min(c => c.col);
            var maxCol = cells.Max(c => c.col);

            var canSplitRow = maxRow > minRow;
            var canSplitCol = maxCol > minCol;
            if (!canSplitRow && !canSplitCol)
                return false;

            var splitHorizontally = canSplitRow && canSplitCol ? _rng.Next(2) == 0 : canSplitRow;

            if (splitHorizontally)
            {
                var pivot = _rng.Next(minRow, maxRow);
                foreach (var cell in cells)
                {
                    if (cell.row <= pivot)
                        left.Add(cell);
                    else
                        right.Add(cell);
                }
            }
            else
            {
                var pivot = _rng.Next(minCol, maxCol);
                foreach (var cell in cells)
                {
                    if (cell.col <= pivot)
                        left.Add(cell);
                    else
                        right.Add(cell);
                }
            }

            if (left.Count == 0 || right.Count == 0)
                return false;

            return true;
        }

        private List<List<(int row, int col)>> SplitByBreadth(List<(int row, int col)> cells, int parts)
        {
            var result = Enumerable.Range(0, parts).Select(_ => new List<(int, int)>()).ToList();
            var queue = new Queue<(int row, int col)>(cells.OrderBy(_ => _rng.Next()));
            var index = 0;

            while (queue.Count > 0)
            {
                result[index % parts].Add(queue.Dequeue());
                index++;
            }

            return result;
        }

        private RoomType[] PickRoomTypes(int count)
        {
            var pool = ((RoomType[])Enum.GetValues(typeof(RoomType))).ToList();
            Shuffle(pool);

            var result = new RoomType[count];
            for (var i = 0; i < count; i++)
                result[i] = pool[i % pool.Count];

            Shuffle(result);
            return result;
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
