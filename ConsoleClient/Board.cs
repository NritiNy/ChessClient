namespace ConsoleClient;

public static class Piece
{
    public const int None = 0;
    public const int Pawn = 1;
    public const int Knight = 2;
    public const int Bishop = 3;
    public const int Rook = 4;
    public const int Queen = 5;
    public const int King = 6;

    public const int White = 8;
    public const int Black = 16;

    private const int typeMask = 0b00111;
    private const int blackMask = 0b10000;
    private const int whiteMask = 0b01000;
    private const int colorMask = whiteMask | blackMask;

    public static bool IsColor(int piece, int color) => (piece & colorMask) == color;

    public static int Color(int piece) => piece & colorMask;

    public static int PieceType(int piece) => piece & typeMask;
}

public class Position
{
    #region Perft Positions
    public static Position Start => new ("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

    public static List<Position> PerftPositions => new List<Position>
    {
        new Position("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"),
        new Position("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"),
        new Position("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1"),
        new Position("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1"),
        new Position("r2q1rk1/pP1p2pp/Q4n2/bbp1p3/Np6/1B3NBn/pPPP1PPP/R3K2R b KQ - 0 1 "),
        new Position("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"),
        new Position("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 1")
    };

    public static List<List<ulong>> PerftResults => new List<List<ulong>>
    {
        new List<ulong> {20, 400, 8902, 197281, 4865609, 119060324, 3195901860, 84998978956, 2439530234167},
        new List<ulong> {48, 2039, 97862, 4085603, 193690690, 8031647685},
        new List<ulong> {14, 191, 2812, 43238, 674624, 11030083, 178633661, 3009794393},
        new List<ulong> {6, 264, 9467, 422333, 15833292, 706045033},
        new List<ulong> {6, 264, 9467, 422333, 15833292, 706045033},
        new List<ulong> {44, 1486, 62379, 2103487, 89941194},
        new List<ulong> {46, 2079, 89890, 3894594, 164075551, 6923051137, 287188994746, 11923589843526, 490154852788714}
    };
    #endregion

    private int[,] _board = new int[8, 8];
    public int[,] Board => this._board;
    public int ColorToMove { get; private set; } = Piece.White;
    public string CastlingAvailability { get; private set; } = "-";
    public string EnPassantTarget { get; private set; } = "-";
    public int HalfmoveClock { get; private set; } = 0;
    public int MoveCounter { get; private set; } = 1;
    

    public Position(string fenString)
    {
        this.ParseFENString(fenString);
    }

    private void ParseFENString(string fenString)
    {
        var segments = fenString.Split(" ");
        
        this.ColorToMove = segments[1] == "w" ? Piece.White : Piece.Black;
        this.CastlingAvailability = segments[2];
        this.EnPassantTarget = segments[3];
        this.HalfmoveClock = int.Parse(segments[4]);
        this.MoveCounter = int.Parse(segments[5]);

        var rows = segments[0].Split("/");
        for (int i = rows.Length - 1; i >= 0; i--)
        {
            var row = rows[i];
            var rowIdx = 0;
            for (var idx = 0; idx < row.Length; idx ++)
            {
                char c = row[idx];
                if (char.IsNumber(c))
                {
                    rowIdx += (int)char.GetNumericValue(c);
                }
                else
                {
                    var cl = char.ToLower(c);
                    
                    var color = char.IsLower(c) ? Piece.Black : Piece.White;
                    var piece = cl switch
                    {
                        'p' => Piece.Pawn,
                        'n' => Piece.Knight,
                        'b' => Piece.Bishop,
                        'r' => Piece.Rook,
                        'q' => Piece.Queen,
                        'k' => Piece.King,
                        _ => Piece.None
                    };
                    
                    this._board[i, rowIdx] = color | piece;
                    rowIdx++;
                }
            }
        }
    }
}