namespace ChessEngine;

using BoardRepresentation;
using static BoardRepresentation.BoardUtility;
using static PrecomputedMoveData;

public class MoveGenerator {
    #region Singleton

    private static MoveGenerator? _instance;

    public static MoveGenerator Instance => _instance ??= new MoveGenerator();

    private MoveGenerator() { }

    #endregion


    private Position _position = null!;

    private List<Move> _moves = null!;

    public bool IsLegalMove(Position pos, Move move) => true;
    

    public List<Move> GenerateLegalMoves(Position pos) {
        return new List<Move>();
    }
}