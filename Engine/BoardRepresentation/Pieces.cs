namespace ChessEngine.BoardRepresentation;

public static class Color
{
    public const int White = 8;
    public const int Black = 16;
}

public static class Piece
{
    public const int None = 0;
    public const int Pawn = 1;
    public const int Knight = 2;
    public const int King = 3;
    public const int Bishop = 4;
    public const int Rook = 5;
    public const int Queen = 6;

    private const int TypeMask = 0b00111;
    private const int BlackMask = 0b10000;
    private const int WhiteMask = 0b01000;
    private const int ColorMask = WhiteMask | BlackMask;

    public static bool IsColor(int piece, int color) => (piece & ColorMask) == color;

    public static int Color(int piece) => piece & ColorMask;

    public static int PieceType(int piece) => piece & TypeMask;
}

