namespace ChessEngine.BoardRepresentation; 

public class PieceList {
    
    public readonly int[] OccupiedSquares;
    private readonly int[] _map;
    private int _numPieces;

    public PieceList (int maxPieceCount = 16) {
        OccupiedSquares = new int[maxPieceCount];
        _map = new int[64];
        _numPieces = 0;
    }

    public int Count => _numPieces;

    public void AddPieceAtSquare (int square) {
        OccupiedSquares[_numPieces] = square;
        _map[square] = _numPieces;
        _numPieces++;
    }

    public void RemovePieceAtSquare (int square) {
        var pieceIndex = _map[square]; 
        OccupiedSquares[pieceIndex] = OccupiedSquares[_numPieces - 1]; 
        _map[OccupiedSquares[pieceIndex]] = pieceIndex; 
        _numPieces--;
    }

    public void MovePiece (int startSquare, int targetSquare) {
        var pieceIndex = _map[startSquare]; 
        OccupiedSquares[pieceIndex] = targetSquare;
        _map[targetSquare] = pieceIndex;
    }

    public int this [int index] => OccupiedSquares[index];

}