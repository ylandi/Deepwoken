using System;
using System.Collections.Generic;

class Program
{
    static readonly string FILES = "abcdefgh";
    static readonly string RANKS = "12345678";
    static char[,] board = new char[8, 8];

    static void Main()
    {
        InitBoard();

        bool whiteTurn = true;
        int moveNumber = 1;

        Console.WriteLine("Простые шахматы (C#). Ходы формата e2e4. Выход: quit.");

        while (true)
        {
            PrintBoard();

            if (InCheck(whiteTurn))
                Console.WriteLine("Шах! " + (whiteTurn ? "Белым" : "Чёрным"));

            if (!HasAnyLegalMoves(whiteTurn))
            {
                if (InCheck(whiteTurn))
                    Console.WriteLine("Мат! Победили " + (whiteTurn ? "Чёрные" : "Белые"));
                else
                    Console.WriteLine("Пат! Ничья.");
                break;
            }

            Console.Write($"{moveNumber}. {(whiteTurn ? "Белые" : "Чёрные")} ход: ");
            string input = Console.ReadLine();
            if (input == null) continue;
            input = input.Trim().ToLower();

            if (input == "quit" || input == "exit") break;

            if (input.Length != 4)
            {
                Console.WriteLine("Неверный формат. Пример: e2e4");
                continue;
            }

            var (r1, c1) = AlgebraicToIndex(input.Substring(0, 2));
            var (r2, c2) = AlgebraicToIndex(input.Substring(2, 2));

            if (!OnBoard(r1, c1) || !OnBoard(r2, c2))
            {
                Console.WriteLine("Координаты вне доски.");
                continue;
            }

            char piece = board[r1, c1];
            if (piece == '.')
            {
                Console.WriteLine("Там нет фигуры.");
                continue;
            }
            if (whiteTurn != char.IsUpper(piece))
            {
                Console.WriteLine("Это не ваша фигура.");
                continue;
            }

            var moves = GenerateMovesForPiece(r1, c1);
            bool legal = false;
            foreach (var (mr, mc) in moves)
            {
                if (mr == r2 && mc == c2)
                {
                    var copy = (char[,])board.Clone();
                    MakeMove(copy, r1, c1, r2, c2);
                    if (!InCheck(copy, whiteTurn))
                    {
                        board = copy;
                        legal = true;
                    }
                }
            }

            if (!legal)
            {
                Console.WriteLine("Нельзя так ходить.");
                continue;
            }

            whiteTurn = !whiteTurn;
            moveNumber++;
        }
    }

    static void InitBoard()
    {
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                board[r, c] = '.';

        string back = "RNBQKBNR";
        for (int c = 0; c < 8; c++)
        {
            board[7, c] = back[c];      // белые фигуры
            board[6, c] = 'P';
            board[0, c] = char.ToLower(back[c]); // чёрные фигуры
            board[1, c] = 'p';
        }
    }

    static void PrintBoard()
    {
        Console.WriteLine();
        for (int r = 0; r < 8; r++)
        {
            Console.Write(8 - r + " ");
            for (int c = 0; c < 8; c++)
                Console.Write(board[r, c] + " ");
            Console.WriteLine();
        }
        Console.WriteLine("  a b c d e f g h\n");
    }

    static (int, int) AlgebraicToIndex(string sq)
    {
        int file = FILES.IndexOf(sq[0]);
        int rank = int.Parse(sq[1].ToString());
        int r = 8 - rank;
        return (r, file);
    }

    static bool OnBoard(int r, int c) => r >= 0 && r < 8 && c >= 0 && c < 8;

    static void MakeMove(char[,] b, int r1, int c1, int r2, int c2)
    {
        char p = b[r1, c1];
        b[r1, c1] = '.';
        // превращение пешки
        if (char.ToLower(p) == 'p' && (r2 == 0 || r2 == 7))
            p = char.IsUpper(p) ? 'Q' : 'q';
        b[r2, c2] = p;
    }

    static List<(int, int)> GenerateMovesForPiece(int r, int c)
    {
        List<(int, int)> moves = new List<(int, int)>();
        char p = board[r, c];
        if (p == '.') return moves;
        bool white = char.IsUpper(p);
        int dir = white ? -1 : 1;

        switch (char.ToLower(p))
        {
            case 'p':
                if (OnBoard(r + dir, c) && board[r + dir, c] == '.')
                {
                    moves.Add((r + dir, c));
                    int start = white ? 6 : 1;
                    if (r == start && board[r + 2 * dir, c] == '.')
                        moves.Add((r + 2 * dir, c));
                }
                foreach (int dc in new int[] { -1, 1 })
                {
                    int rr = r + dir, cc = c + dc;
                    if (OnBoard(rr, cc) && board[rr, cc] != '.' && !SameColor(board[rr, cc], p))
                        moves.Add((rr, cc));
                }
                break;

            case 'n':
                int[,] knights = { { 2, 1 }, { 1, 2 }, { -1, 2 }, { -2, 1 }, { -2, -1 }, { -1, -2 }, { 1, -2 }, { 2, -1 } };
                for (int i = 0; i < 8; i++)
                {
                    int rr = r + knights[i, 0], cc = c + knights[i, 1];
                    if (OnBoard(rr, cc) && !SameColor(board[rr, cc], p))
                        moves.Add((rr, cc));
                }
                break;

            case 'b': case 'r': case 'q':
                int[,] dirs = {
                    { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 },
                    { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 }
                };
                for (int i = 0; i < 8; i++)
                {
                    if ((char.ToLower(p) == 'b' && i >= 4) ||
                        (char.ToLower(p) == 'r' && i < 4))
                        continue;

                    int rr = r + dirs[i, 0], cc = c + dirs[i, 1];
                    while (OnBoard(rr, cc))
                    {
                        if (board[rr, cc] == '.')
                            moves.Add((rr, cc));
                        else
                        {
                            if (!SameColor(board[rr, cc], p))
                                moves.Add((rr, cc));
                            break;
                        }
                        rr += dirs[i, 0]; cc += dirs[i, 1];
                    }
                }
                break;

            case 'k':
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0) continue;
                        int rr = r + dr, cc = c + dc;
                        if (OnBoard(rr, cc) && !SameColor(board[rr, cc], p))
                            moves.Add((rr, cc));
                    }
                break;
        }

        return moves;
    }

    static bool SameColor(char p1, char p2)
    {
        if (p1 == '.' || p2 == '.') return false;
        return (char.IsUpper(p1) && char.IsUpper(p2)) || (char.IsLower(p1) && char.IsLower(p2));
    }

    static bool InCheck(bool white)
    {
        var king = FindKing(board, white);
        if (king == (-1, -1)) return false;
        return SquareAttacked(board, king.Item1, king.Item2, !white);
    }

    static bool InCheck(char[,] b, bool white)
    {
        var king = FindKing(b, white);
        if (king == (-1, -1)) return false;
        return SquareAttacked(b, king.Item1, king.Item2, !white);
    }

    static (int, int) FindKing(char[,] b, bool white)
    {
        char target = white ? 'K' : 'k';
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                if (b[r, c] == target)
                    return (r, c);
        return (-1, -1);
    }

    static bool SquareAttacked(char[,] b, int r, int c, bool byWhite)
    {
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
            {
                char p = b[i, j];
                if (p == '.') continue;
                if (byWhite != char.IsUpper(p)) continue;

                var moves = GenerateMovesForPieceTemp(b, i, j);
                foreach (var (rr, cc) in moves)
                    if (rr == r && cc == c) return true;
            }
        return false;
    }

    // копия генератора для проверки атак (не использует основной board)
    static List<(int, int)> GenerateMovesForPieceTemp(char[,] b, int r, int c)
    {
        List<(int, int)> moves = new List<(int, int)>();
        char p = b[r, c];
        if (p == '.') return moves;
        bool white = char.IsUpper(p);
        int dir = white ? -1 : 1;

        switch (char.ToLower(p))
        {
            case 'p':
                foreach (int dc in new int[] { -1, 1 })
                {
                    int rr = r + dir, cc = c + dc;
                    if (OnBoard(rr, cc)) moves.Add((rr, cc));
                }
                break;

            case 'n':
                int[,] knights = { { 2, 1 }, { 1, 2 }, { -1, 2 }, { -2, 1 }, { -2, -1 }, { -1, -2 }, { 1, -2 }, { 2, -1 } };
                for (int i = 0; i < 8; i++)
                {
                    int rr = r + knights[i, 0], cc = c + knights[i, 1];
                    if (OnBoard(rr, cc)) moves.Add((rr, cc));
                }
                break;

            case 'b': case 'r': case 'q':
                int[,] dirs = {
                    { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 },
                    { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 }
                };
                for (int i = 0; i < 8; i++)
                {
                    if ((char.ToLower(p) == 'b' && i >= 4) ||
                        (char.ToLower(p) == 'r' && i < 4))
                        continue;

                    int rr = r + dirs[i, 0], cc = c + dirs[i, 1];
                    while (OnBoard(rr, cc))
                    {
                        moves.Add((rr, cc));
                        if (b[rr, cc] != '.') break;
                        rr += dirs[i, 0]; cc += dirs[i, 1];
                    }
                }
                break;

            case 'k':
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0) continue;
                        int rr = r + dr, cc = c + dc;
                        if (OnBoard(rr, cc)) moves.Add((rr, cc));
                    }
                break;
        }

        return moves;
    }

    static bool HasAnyLegalMoves(bool white)
    {
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
            {
                char p = board[r, c];
                if (p == '.') continue;
                if (white != char.IsUpper(p)) continue;
                foreach (var (rr, cc) in GenerateMovesForPiece(r, c))
                {
                    var copy = (char[,])board.Clone();
                    MakeMove(copy, r, c, rr, cc);
                    if (!InCheck(copy, white)) return true;
                }
            }
        return false;
    }
}
