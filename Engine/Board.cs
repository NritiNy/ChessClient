namespace ChessEngine;

public class Position {
    public static Position InitialPosition => new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");


    public string FenString =>
        $"{GetFenStringFromBoard()} {(ColorToMove == Piece.White ? 'w' : 'b')} {CastlingAvailability} {EnPassantTarget} {HalfmoveClock} {MoveCounter}";

    public int[] Board { get; } = new int[64];
    public int ColorToMove { get; private set; }
    public string CastlingAvailability { get; private set; } = "-";
    public string EnPassantTarget { get; private set; } = "-";
    public int HalfmoveClock { get; private set; }
    public int MoveCounter { get; private set; }


    public Position(string fenString) {
        ParseFenString(fenString);
    }

    private void ParseFenString(string fenString) {
        var segments = fenString.Split(" ");

        ColorToMove = segments[1] == "w" ? Piece.White : Piece.Black;
        CastlingAvailability = segments[2];
        EnPassantTarget = segments[3];
        HalfmoveClock = int.Parse(segments[4]);
        MoveCounter = int.Parse(segments[5]);

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

    private string GetFenStringFromBoard() {
        var fenString = "";

        int space = 0, rank = 7, file = 0;
        for (int i = 0; i < 64; i++) {
            if (file == 8) {
                if (space > 0) {
                    fenString += space.ToString();
                    space = 0;
                }
                fenString += "/";
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
                    fenString += space.ToString();
                    space = 0;
                }

                fenString += Piece.Color(Board[rank * 8 + file]) == Color.White
                    ? char.ToUpper(piece)
                    : char.ToLower(piece);
            }

            file++;
        }

        return fenString;
    }
}