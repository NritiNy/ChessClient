namespace ChessEngine; 

public class MoveGenerator {

    #region Singleton
    private static MoveGenerator? _instance;

    public static MoveGenerator Instance => _instance ??= new MoveGenerator();

    private MoveGenerator() { }
    #endregion


    private Position? _position;
    private List<Move> _moves = new ();
    private int _colorToMove;

    private bool _inCheck = false;
    private bool _inDoubleCheck = false;
    
    
    private void Init(int colorToMove = Color.White) {
        _moves = new List<Move>();
        _colorToMove = colorToMove;

        _inCheck = false;
        _inDoubleCheck = false;
    }
    
    
    public bool IsLegalMove(Position pos, int colorToMove, Move move) {
        if (Piece.Color(pos.Board[move.Start]) != colorToMove) return false;
        
        //return GenerateLegalMoves(board, colorToMove).Contains(move);
        return true;
    }
    
    public List<Move> GenerateLegalMoves(Position pos, int colorToMove) {
        _position = pos;
        Init(colorToMove);
        
        CalculateAttackData();
        GenerateKingMoves();

        if (_inDoubleCheck) return _moves;
        
        GenerateSlidingMoves();
        GenerateKnightMoves();
        GeneratePawnMoves();
        
        return _moves;
    }


    private void CalculateAttackData() {

    }

    private void GenerateKingMoves() {
        
    }

    private void GenerateSlidingMoves() {
        
    }

    private void GenerateKnightMoves() {
        
    }

    private void GeneratePawnMoves() {
        
    }
}