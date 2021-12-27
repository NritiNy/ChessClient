namespace ChessEngine.BoardRepresentation;


public struct Coordinate {
    public readonly int FileIndex;
    public readonly int RankIndex;

    public Coordinate (int fileIndex, int rankIndex) {
        FileIndex = fileIndex;
        RankIndex = rankIndex;
    }

    public bool IsLightSquare () => (FileIndex + RankIndex) % 2 != 0;
}


public static class BoardUtility {

    public const string FileNames = "abcdefgh";
    public const string RankNames = "12345678";

    
    public const int A1 = 0;
    public const int B1 = 1;
    public const int C1 = 2;
    public const int D1 = 3;
    public const int E1 = 4;
    public const int F1 = 5;
    public const int G1 = 6;
    public const int H1 = 7;

    public const int A8 = 56;
    public const int B8 = 57;
    public const int C8 = 58;
    public const int D8 = 59;
    public const int E8 = 60;
    public const int F8 = 61;
    public const int G8 = 62;
    public const int H8 = 63;
    
    
    public static int RankIndex (int squareIndex) => squareIndex >> 3;
    
    public static int FileIndex (int squareIndex) => squareIndex & 0b000111;

    public static bool IsLightSquare (int fileIndex, int rankIndex) => (fileIndex + rankIndex) % 2 != 0;
    
    
    public static int IndexFromCoord (int fileIndex, int rankIndex) => rankIndex * 8 + fileIndex;

    public static int IndexFromCoord (Coordinate coordinate) {
        return IndexFromCoord (coordinate.FileIndex, coordinate.RankIndex);
    }

    public static Coordinate CoordFromIndex (int squareIndex) {
        return new Coordinate (FileIndex (squareIndex), RankIndex (squareIndex));
    }
    
    
    public static string SquareName(int fileIndex, int rankIndex) => FileNames[fileIndex] + "" + (rankIndex + 1);
    public static string SquareName(int squareIndex) => SquareName(CoordFromIndex (squareIndex));
    public static string SquareName(Coordinate coord) => SquareName(coord.FileIndex, coord.RankIndex);
    
    
    public static bool BitboardContainsSquare(ulong bitboard, int squareIndex) => ((bitboard >> squareIndex) & 1) != 0;
}