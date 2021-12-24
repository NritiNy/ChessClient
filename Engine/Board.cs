namespace ChessEngine;

public class Position {
    public static Position InitialPosition => new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
    

    public int[] Board { get; } = new int[64];
    public int ColorToMove { get; private set; }
    public string CastlingAvailability { get; private set; } = "-";
    public int EnPassantTarget { get; private set; } = -1;
    public int HalfmoveClock { get; private set; }
    public int MoveCounter { get; private set; }


    private readonly List<string> _reachedPositions = new ();


    public Position(string fenString) {
        ParseFenString(fenString);
        
        _reachedPositions.Add(FenString());
    }


    public bool IsLegalMove(Move move) => MoveGenerator.Instance.IsLegalMove(this, ColorToMove, move);

    public int GenerateLegalMoves(out List<Move> moves) {
        moves = MoveGenerator.Instance.GenerateLegalMoves(this, ColorToMove);
        return moves.Count;
    }


    public void MakeMove(Move move) {
        Board[move.Target] = Board[move.Start];
        Board[move.Start] = Piece.None;

        if (ColorToMove == Color.Black) {
            MoveCounter++;
        }
        ColorToMove = ColorToMove == Color.White ? Color.Black : Color.White;
        
        _reachedPositions.Add(FenString());
    }

    public void UnmakeMove(int nMoves = 1) {
        for (int i = 0; i < nMoves && i < _reachedPositions.Count; i++) {
            var fenString = _reachedPositions[^1];
            _reachedPositions.RemoveAt(_reachedPositions.Count - 1);
            
            ParseFenString(fenString);
        }
    }

    private void ParseFenString(string fenString) {
        var segments = fenString.Split(" ");

        ColorToMove = segments[1] == "w" ? Piece.White : Piece.Black;
        CastlingAvailability = segments[2];
        EnPassantTarget = -1;
        HalfmoveClock = int.Parse(segments[4]);
        MoveCounter = int.Parse(segments[5]);
        
        if (segments[3].Length > 1) {
            var f = segments[3][0] - 97;
            var r = (int)char.GetNumericValue(segments[3][0]);
            EnPassantTarget = r * 8 + f;
        }

        var board = segments[0];
        int file = 0, rank = 7;

        foreach (var c in board) {
            if (c == '/') {
                file = 0;
                rank--;
            }
            else {
                if (char.IsDigit(c)) {
                    file += (int) char.GetNumericValue(c);
                }
                else {
                    var color = char.IsUpper(c) ? Color.White : Color.Black;
                    var piece = char.ToLower(c) switch {
                        'p' => Piece.Pawn,
                        'n' => Piece.Knight,
                        'b' => Piece.Bishop,
                        'r' => Piece.Rook,
                        'q' => Piece.Queen,
                        'k' => Piece.King,
                        _ => Piece.None
                    };

                    Board[rank * 8 + file] = piece | color;
                    file++;
                }
            }
        }
    }

    public string FenString() {
        var boardString = "";

        int space = 0, rank = 7, file = 0;
        for (int i = 0; i < 64; i++) {
            if (file == 8) {
                if (space > 0) {
                    boardString += space.ToString();
                    space = 0;
                }
                boardString += "/";
                file = 0;
                rank--;
            }
            var piece = Piece.PieceType(Board[rank * 8 + file]) switch {
                Piece.Pawn => 'p',
                Piece.Knight => 'n',
                Piece.Bishop => 'b',
                Piece.Rook => 'r',
                Piece.Queen => 'q',
                Piece.King => 'k',
                _ => ' '
            };

            if (piece == ' ') {
                space++;
            }
            else {
                if (space > 0) {
                    boardString += space.ToString();
                    space = 0;
                }

                boardString += Piece.Color(Board[rank * 8 + file]) == Color.White
                    ? char.ToUpper(piece)
                    : char.ToLower(piece);
            }

            file++;
        }

        var enPassant = "-";
        if (EnPassantTarget > -1) {
            file = EnPassantTarget % 8;
            rank = EnPassantTarget / 8;
            
            enPassant = ((char)(file + 97)).ToString() + rank;
        }

        return $"{boardString} {(ColorToMove == Color.White ? "w" : "b")} {CastlingAvailability} {enPassant} {HalfmoveClock} {MoveCounter}";
    }
}