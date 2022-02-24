namespace ChessEngine.BoardRepresentation; 

public class PieceList {
    private readonly int[] _occupiedSquares;
    private readonly int[] _map;

    public PieceList (int maxPieceCount = 16) {
        _occupiedSquares = new int[maxPieceCount];
        _map = new int[64];
        Count = 0;
    }

    public int Count { get; private set; }

    public void AddPieceAtSquare (int square) {
        _occupiedSquares[Count] = square;
        _map[square] = Count;
        Count++;
    }

    public void RemovePieceAtSquare (int square) {
        var pieceIndex = _map[square]; 
        _occupiedSquares[pieceIndex] = _occupiedSquares[Count - 1]; 
        _map[_occupiedSquares[pieceIndex]] = pieceIndex; 
        Count--;
    }

    public void MovePiece (int startSquare, int targetSquare) {
        var pieceIndex = _map[startSquare]; 
        _occupiedSquares[pieceIndex] = targetSquare;
        _map[targetSquare] = pieceIndex;
    }

    public int this [int index] => _occupiedSquares[index];
}