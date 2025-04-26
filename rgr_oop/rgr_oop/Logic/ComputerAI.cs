using System;
using System.Collections.Generic;
using System.Linq;

namespace rgr_oop
{
    public class ComputerAI
    {
        private GameLogic _game;
        // Припускаємо, що комп’ютер грає за Cross, а гравець – за Nought.
        private readonly CellState computerMark = CellState.Cross;
        private readonly CellState opponentMark = CellState.Nought;
        // Діапазон для першого ходу та запасний варіант, що відповідає відображуваній сітці.
        private readonly int minX = 0;
        private readonly int maxX = 29; // для 30 клітинок по горизонталі
        private readonly int minY = 0;
        private readonly int maxY = 9;  // для 10 клітинок по вертикалі

        public ComputerAI(GameLogic game)
        {
            _game = game;
        }

        /// <summary>
        /// Обчислює оптимальний хід комп’ютера і повертає координати (не фіксуючи їх – це робить код форми).
        /// </summary>
        public BoardPoint GetComputerMove()
        {
            if (_game.CurrentPlayer != computerMark)
                throw new InvalidOperationException("Не хід комп’ютера!");

            BoardPoint chosenMove;
            // Якщо поле порожнє – перший хід: вибираємо довільну точку у межах відображуваної сітки.
            if (_game.Board.Count == 0)
            {
                Random random = new Random();
                int x = random.Next(minX, maxX + 1);
                int y = random.Next(minY, maxY + 1);
                chosenMove = new BoardPoint(x, y);
                return chosenMove;
            }

            HashSet<BoardPoint> candidates = GetCandidateMoves();
            // Якщо кандидатів немає (на випадок малоймовірного), повертаємо випадкову точку, не зайняту на полі.
            if (candidates.Count == 0)
            {
                Random random = new Random();
                do
                {
                    int x = random.Next(minX, maxX + 1);
                    int y = random.Next(minY, maxY + 1);
                    chosenMove = new BoardPoint(x, y);
                }
                while (_game.Board.ContainsKey(chosenMove));
                return chosenMove;
            }

            // 1. Якщо є хід, який веде до негайної перемоги – повертаємо його
            foreach (var move in candidates)
            {
                if (TrySimulateMove(move, computerMark))
                {
                    chosenMove = move;
                    return chosenMove;
                }
            }

            // 2. Інакше, якщо можна заблокувати виграш суперника – повертаємо цей хід
            foreach (var move in candidates)
            {
                if (TrySimulateMove(move, opponentMark))
                {
                    chosenMove = move;
                    return chosenMove;
                }
            }

            // 3. Якщо жоден з попередніх не підходить – обираємо хід з максимальною оцінкою
            chosenMove = candidates.First();
            int bestScore = int.MinValue;
            foreach (var move in candidates)
            {
                int score = EvaluateMove(move, computerMark);
                if (score > bestScore)
                {
                    bestScore = score;
                    chosenMove = move;
                }
            }
            return chosenMove;
        }

        /// <summary>
        /// Симулює хід (тимчасово ставить позначку, перевіряє виграш, а потім видаляє її).
        /// </summary>
        private bool TrySimulateMove(BoardPoint move, CellState state)
        {
            _game.Board[move] = state;
            bool isWinning = _game.CheckWin(move);
            _game.Board.Remove(move);
            return isWinning;
        }

        /// <summary>
        /// Збирає потенційних кандидатів, тобто сусідів уже зайнятих клітин.
        /// </summary>
        private HashSet<BoardPoint> GetCandidateMoves()
        {
            HashSet<BoardPoint> candidates = new HashSet<BoardPoint>();
            foreach (var kvp in _game.Board)
            {
                BoardPoint p = kvp.Key;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;
                        BoardPoint neighbor = new BoardPoint(p.X + dx, p.Y + dy);
                        // Тільки якщо клітинка в межах відображуваної сітки і не зайнята.
                        if (neighbor.X >= minX && neighbor.X <= maxX &&
                            neighbor.Y >= minY && neighbor.Y <= maxY &&
                            !_game.Board.ContainsKey(neighbor))
                        {
                            candidates.Add(neighbor);
                        }
                    }
                }
            }
            return candidates;
        }

        /// <summary>
        /// Оцінює якість ходу, аналізуючи кількість послідовних позначок у чотирьох напрямках.
        /// </summary>
        private int EvaluateMove(BoardPoint move, CellState state)
        {
            int score = 0;
            foreach (var direction in new (int dx, int dy)[] { (1, 0), (0, 1), (1, 1), (1, -1) })
            {
                int count = CountAdjacent(move, direction.dx, direction.dy, state) +
                            CountAdjacent(move, -direction.dx, -direction.dy, state);
                score = Math.Max(score, count + 1); // +1 враховуємо сам хід.
            }
            return score;
        }

        /// <summary>
        /// Рахує кількість суміжних клітин із однаковою позначкою в заданому напрямку.
        /// </summary>
        private int CountAdjacent(BoardPoint start, int dx, int dy, CellState state)
        {
            int count = 0;
            BoardPoint current = new BoardPoint(start.X + dx, start.Y + dy);
            while (_game.Board.TryGetValue(current, out CellState s) && s == state)
            {
                count++;
                current = new BoardPoint(current.X + dx, current.Y + dy);
            }
            return count;
        }
    }
}
