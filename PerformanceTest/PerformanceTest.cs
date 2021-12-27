using ChessEngine.BoardRepresentation;

namespace PerformanceTest;

public static class PerformanceTest
{
    private static Position[] Positions => new []
    {
        new Position("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"),
        new Position("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"),
        new Position("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1"),
        new Position("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1"),
        new Position("r2q1rk1/pP1p2pp/Q4n2/bbp1p3/Np6/1B3NBn/pPPP1PPP/R3K2R b KQ - 0 1 "),
        new Position("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"),
        new Position("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 1")
    };

    private static List<ulong>[] Results => new []
    {
        new List<ulong> {20, 400, 8902, 197281, 4865609, 119060324, 3195901860, 84998978956, 2439530234167},
        new List<ulong> {48, 2039, 97862, 4085603, 193690690, 8031647685},
        new List<ulong> {14, 191, 2812, 43238, 674624, 11030083, 178633661, 3009794393},
        new List<ulong> {6, 264, 9467, 422333, 15833292, 706045033},
        new List<ulong> {6, 264, 9467, 422333, 15833292, 706045033},
        new List<ulong> {44, 1486, 62379, 2103487, 89941194},
        new List<ulong> {46, 2079, 89890, 3894594, 164075551, 6923051137, 287188994746, 11923589843526, 490154852788714}
    };

    private static void Run(int position, int depth) {
        if (position < 1 || position >= Positions.Length)
            throw new ArgumentException("The position index has to be valid.");

        if (depth < 1 || depth >= Results[position - 1].Count)
            throw new ArgumentException("The depth has to be within the calculated depths.");
        
        ulong Perft(Position pos, int d) {
            ulong nodes = 0;

            var nMoves = pos.GenerateLegalMoves(out var moves);

            if (d == 1)
                return (ulong) nMoves;

            for (var i = 0; i < nMoves; i++) {
                pos.MakeMove(moves[i]);
                nodes += Perft(pos, d - 1);
                pos.UnmakeMove(moves[i]);
            }

            return nodes;
        }

        var calculated = Perft(Positions[position-1], depth);
        var actual = Results[position-1][depth - 1];
        
        Console.WriteLine($"Number of calculated positions: {calculated.ToString(), 20}");
        Console.WriteLine($"Number of actual positions:     {actual.ToString(),20}");
        
        Console.WriteLine("#============#");
        Console.WriteLine($"#   {(actual == calculated ? "PASSED" : "FAILED")}   #");
        Console.WriteLine("#============#");
    }

    public static void Main(string[] args) {
        Console.WriteLine("Running the following position: ");
        Console.WriteLine(Positions[0].FenString());
        PrintBoard(Positions[0]);
        Console.WriteLine("\n");
        Run(1, 3);
    }
    
    public static void PrintBoard(Position position, int colorToPlay = Color.White)
    {
        Console.Write("      ");
        for (int i = 0; i < 8; i++)
        {
            Console.Write($"  {(char)(colorToPlay == Color.White ? i + 97 : 104 - i)}   ");
        }
        Console.Write("\n");
        Console.WriteLine("     +-----+-----+-----+-----+-----+-----+-----+-----+");
        
        for (int r = 7; r >= 0; r--)
        {
            Console.Write($"  {(colorToPlay == Color.White ? r + 1 : 8 - r)}  |");
            for (int j = 0; j < 8; j++)
            {
                var row = colorToPlay == Color.White ? r : 7 - r;
                var col = colorToPlay == Color.White ? j : 7 - j;
                
                char c = Piece.PieceType(position.Board[row * 8 + col]) switch
                {
                    Piece.Pawn => 'p',
                    Piece.Knight => 'n',
                    Piece.Bishop => 'b',
                    Piece.Rook => 'r',
                    Piece.Queen => 'q',
                    Piece.King => 'k',
                    _ => ' '
                };
                c = Piece.Color(position.Board[row * 8 + col]) == Color.White ? Char.ToUpper(c) : Char.ToLower(c);
                Console.Write($"  {c}  |");
            }
            Console.Write($"  {(colorToPlay == Piece.White ? r + 1 : 8 - r)}\n");
            Console.WriteLine("     +-----+-----+-----+-----+-----+-----+-----+-----+");
        }
        
        Console.Write("      ");
        for (int i = 0; i < 8; i++)
        {
            Console.Write($"  {(char)(colorToPlay == Color.White ? i + 97 : 104 - i)}   ");
        }
        Console.Write("\n");
        
        
    }
}