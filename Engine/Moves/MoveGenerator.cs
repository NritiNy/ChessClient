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
    private int _colorToMove;
    private int _colorIndex;

    private int _opponentColor;
    private int _opponentColorIndex;

    private int _kingSquare;
    private bool _canCastleKingSide;
    private bool _canCastleQueenSide;

    private ulong _opponentAttackMap;
    public bool InCheck { get; private set; }
    public bool InDoubleCheck { get; private set; }


    private void Init(int colorToMove = Color.White) {
        _moves = new List<Move>();
        _colorToMove = colorToMove;
        _colorIndex = _colorToMove / 8 - 1;

        _opponentColor = _colorToMove == Color.White ? Color.Black : Color.White;
        _opponentColorIndex = 1 - _colorIndex;

        _kingSquare = _position.KingSquare[_colorIndex];
        _canCastleKingSide = _colorToMove == Color.White
            ? _position.WhiteCanCastleKingSide
            : _position.BlackCanCastleKingSide;
        _canCastleQueenSide = _colorToMove == Color.White
            ? _position.WhiteCanCastleQueenSide
            : _position.BlackCanCastleQueenSide;


        InCheck = false;
        InDoubleCheck = false;
    }


    public bool IsLegalMove(Position pos, Move move) {
        if (Piece.Color(pos.Board[move.Start]) != pos.ColorToMove) return false;

        return GenerateLegalMoves(pos).Contains(move);
    }

    #region Move generation

    public List<Move> GenerateLegalMoves(Position pos) {
        _position = pos;
        Init(pos.ColorToMove);

        CalculateAttackData();
        GenerateKingMoves();

        if (InDoubleCheck) return _moves;

        GenerateSlidingMoves();
        GenerateKnightMoves();
        GeneratePawnMoves();

        return _moves;
    }

    private void GenerateKingMoves() {
        foreach (var targetSquare in PrecomputedMoveData.KingMoves[_kingSquare]) {
            var targetPiece = _position.Board[targetSquare];
            if (Piece.IsColor(targetPiece, _colorToMove)) continue;
            if (SquareIsAttacked(targetSquare)) continue;

            _moves.Add(new Move(_kingSquare, targetSquare));

            var isCapture = Piece.IsColor(targetPiece, _opponentColor);
            if (InCheck || isCapture) continue;

            if (targetSquare == F1 || targetSquare == F8 && _canCastleKingSide) {
                var castleTarget = targetSquare + 1;
                if (_position.Board[castleTarget] == Piece.None) {
                    if (!SquareIsAttacked(castleTarget))
                        _moves.Add(new Move(_kingSquare, castleTarget, Move.Flag.Castling));
                }
            }

            if (targetSquare == D1 || targetSquare == D8 && _canCastleQueenSide) {
                var castleTarget = targetSquare - 1;
                if (_position.Board[castleTarget] == Piece.None && _position.Board[castleTarget - 1] == Piece.None) {
                    if (!SquareIsAttacked(castleTarget))
                        _moves.Add(new Move(_kingSquare, castleTarget, Move.Flag.Castling));
                }
            }
        }
    }

    private void GenerateSlidingMoves() {
        void GeneratePieceMoves(int startSquare, int startDirIndex, int endDirIndex) {
            var isPinned = IsPinned(startSquare);

            if (InCheck && isPinned) return;

            for (int dirIndex = startDirIndex; dirIndex < endDirIndex; dirIndex++) {
                int curDirOffset = DirectionOffsets[dirIndex];
                //TODO: check pin

                for (int n = 0; n < NSquaresToEdge[startSquare][dirIndex]; n++) {
                    var targetSquare = startSquare + curDirOffset * (n + 1);
                    var targetPiece = _position.Board[targetSquare];

                    if (Piece.IsColor(targetPiece, _colorToMove)) break;

                    var isCapture = Piece.IsColor(targetPiece, _opponentColor);

                    //TODO: check if move blocks check
                    var movePreventsCheck = false;

                    if (!InCheck || movePreventsCheck) {
                        _moves.Add(new Move(startSquare, targetSquare));
                    }

                    if (isCapture || movePreventsCheck) break;
                }
            }
        }

        PieceList rooks = _position.Rooks[_colorIndex];
        for (int i = 0; i < rooks.Count; i++) {
            GeneratePieceMoves(rooks[i], 0, 4);
        }

        PieceList bishops = _position.Bishops[_colorIndex];
        for (int i = 0; i < bishops.Count; i++) {
            GeneratePieceMoves(bishops[i], 4, 8);
        }

        PieceList queens = _position.Queens[_colorIndex];
        for (int i = 0; i < queens.Count; i++) {
            GeneratePieceMoves(queens[i], 0, 8);
        }
    }

    private void GenerateKnightMoves() {
        var knights = _position.Knights[_colorIndex];

        for (var i = 0; i < knights.Count; i++) {
            var startSquare = knights[i];

            if (IsPinned(startSquare)) continue;

            for (var moveIndex = 0; moveIndex < KnightMoves[startSquare].Length; moveIndex++) {
                var targetSquare = KnightMoves[startSquare][moveIndex];
                var targetPiece = _position.Board[targetSquare];

                if (Piece.IsColor(targetPiece, _colorToMove)) continue;

                //TODO: check if knight interposes check

                _moves.Add(new Move(startSquare, targetSquare));
            }
        }
    }

    private void GeneratePawnMoves() {
        var pawns = _position.Pawns[_colorIndex];
        var offset = _position.WhiteToMove ? 8 : -8;
        var startRank = _position.WhiteToMove ? 1 : 6;
        var rankBeforePromotion = (startRank * 6) % 36;

        var enPassantFile = ((int) (_position.CurrentGameState >> 4) & 15) - 1;
        var enPassantSquare = -1;
        if (enPassantFile != -1)
            enPassantSquare = 8 * (_position.WhiteToMove ? 5 : 2) + enPassantFile;

        for (var i = 0; i < pawns.Count; i++) {
            var startSquare = pawns[i];
            var rank = RankIndex(startSquare);
            var beforePromotion = rank == rankBeforePromotion;

            // normal moves
            var targetSquare = startSquare + offset;

            if (_position.Board[targetSquare] == Piece.None) {
                //TODO: check if moving in pin direction
                if (!IsPinned(startSquare) || false) {
                    //TODO: check if pawn interposes check
                    if (!InCheck || false) {
                        if (beforePromotion) MakePromotion(startSquare, targetSquare);
                        else _moves.Add(new Move(startSquare, targetSquare));
                    }

                    // initial two forward
                    if (rank == startRank) {
                        targetSquare += offset;
                        if (_position.Board[targetSquare] == Piece.None) {
                            //TODO: check if pawn interposes check
                            if (!InCheck || false) {
                                _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PawnTwoForward));
                            }
                        }
                    }
                }
            }

            // captures
            for (int j = 0; j < 2; j++) {
                if (NSquaresToEdge[startSquare][PawnAttackDirections[_colorIndex][j]] > 0) {
                    var captureDir = DirectionOffsets[PawnAttackDirections[_colorIndex][j]];
                    targetSquare = startSquare + captureDir;
                    var targetPiece = _position.Board[targetSquare];

                    // TODO: check if pawn moves along pin
                    if (IsPinned(startSquare) && true) continue;

                    if (Piece.IsColor(targetPiece, _opponentColor)) {
                        //TODO: check if target square interposes check
                        if (InCheck && true) continue;
                        
                        if (beforePromotion) MakePromotion(startSquare, targetSquare);
                        else _moves.Add(new Move(startSquare, targetSquare));
                    }

                    if (targetSquare == enPassantSquare) {
                        var capturedPawnSquare = targetSquare + (_position.WhiteToMove ? -8 : 8);
                        //TODO: check if in check after en passant
                        if (true) _moves.Add(new Move(startSquare, targetSquare, Move.Flag.EnPassantCapture));
                    }
                }
            }
        }
    }

    private void MakePromotion(int startSquare, int targetSquare) { }

    #endregion

    #region Utilities

    private bool SquareIsAttacked(int targetSquare) => BitboardContainsSquare(_opponentAttackMap, targetSquare);

    private void CalculateAttackData() { }

    private bool IsPinned(int square) {
        return false;
    }

    #endregion
}