using ChessEngine.BoardRepresentation;

namespace ChessEngine.Moves;

using static BoardUtility;
using static PrecomputedMoveData;

public class MoveGenerator {
    #region Singleton

    private static MoveGenerator? _instance;

    public static MoveGenerator Instance => _instance ??= new MoveGenerator();

    private MoveGenerator() { }

    #endregion

    public enum PromotionMode {
        All,
        Queen
    }

    private Position _position = null!;
    private List<Move> _moves = null!;
    private PromotionMode _promotionMode = PromotionMode.All;


    private bool _whiteToMove;
    private int _color;
    private int _colorIndex;
    private int _opponentColor;
    private int _opponentColorIndex;

    private bool _inCheck;
    private bool _inDoubleCheck;

    private ulong _pinnedSquares;
    private ulong _opponentAttackMap;
    private ulong _opponentPieceAttackMap;
    private ulong _checkRays;

    private int _ownKingSquare;

    public bool IsLegalMove(Position pos, Move move) => Piece.Color(pos.Board[move.Start]) == pos.ColorToMove &&
                                                        GenerateLegalMoves(pos).Contains(move);


    public List<Move> GenerateLegalMoves(Position pos) {
        _position = pos;

        Init();
        CalculateAttackData();
        GenerateKingMoves();

        if (_inDoubleCheck) return _moves;

        GenerateSlidingMoves();
        GenerateKnightMoves();
        GeneratePawnMoves();

        return _moves;
    }

    private void Init() {
        _moves = new List<Move>(256);

        _whiteToMove = _position.WhiteToMove;
        _color = _position.ColorToMove;
        _colorIndex = _whiteToMove ? 0 : 1;
        _opponentColor = _position.OpponentColor;
        _opponentColorIndex = 1 - _colorIndex;

        _inCheck = false;
        _inDoubleCheck = false;

        _pinnedSquares = 0ul;
        _opponentAttackMap = 0ul;
        _opponentPieceAttackMap = 0ul;
        _checkRays = 0ul;

        _ownKingSquare = _position.KingSquare[_colorIndex];
    }

    #region Attack Calculations

    private void CalculateAttackData() {
        void DoSlidingPiece(int startSquare, int startDirIndex, int endDirIndex) {
            for (var directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++) {
                int currentDirOffset = DirectionOffsets[directionIndex];
                for (var n = 0; n < NSquaresToEdge[startSquare][directionIndex]; n++) {
                    var targetSquare = startSquare + currentDirOffset * (n + 1);
                    var targetSquarePiece = _position.Board[targetSquare];

                    _opponentPieceAttackMap |= 1ul << targetSquare;
                    if (targetSquare != _ownKingSquare) {
                        if (targetSquarePiece != Piece.None) {
                            break;
                        }
                    }
                }
            }
        }

        // rook attacks
        var rooks = _position.Rooks[_opponentColorIndex];
        for (var i = 0; i < rooks.Count; i++) {
            DoSlidingPiece(rooks[i], 0, 4);
        }


        // bishop attacks
        var bishops = _position.Bishops[_opponentColorIndex];
        for (var i = 0; i < bishops.Count; i++) {
            DoSlidingPiece(bishops[i], 4, 8);
        }

        // queen attacks
        var queens = _position.Queens[_opponentColorIndex];
        for (var i = 0; i < queens.Count; i++) {
            DoSlidingPiece(queens[i], 0, 8);
        }

        GeneratePinBitBoard();
        
        // knight attacks
        var knights = _position.Knights[_opponentColorIndex];
        for (var i = 0; i < knights.Count; i++) {
            var startSquare = knights[i];
            var attackMap = KnightAttackBitBoards[startSquare];

            if (((attackMap >> _ownKingSquare) & 1) != 0) {
                _inDoubleCheck = _inCheck;
                _inCheck = true;
                _checkRays |= 1ul << startSquare;
            }

            _opponentPieceAttackMap |= attackMap;
        }

        _opponentAttackMap |= _opponentPieceAttackMap;
        
        
        // pawn attacks
        var pawns = _position.Pawns[_opponentColorIndex];
        for (var i = 0; i < pawns.Count; i++) {
            var startSquare = pawns[i];
            var attackMap = PawnAttackBitBoards[startSquare][_opponentColorIndex];

            if (((attackMap >> _ownKingSquare) & 1) != 0) {
                _inDoubleCheck = _inCheck;
                _inCheck = true;
                _checkRays |= 1ul << startSquare;
            }

            _opponentAttackMap |= attackMap;
        }
        
        // king attacks
        _opponentAttackMap |= KingAttackBitBoards[_position.KingSquare[_opponentColorIndex]];
    }

    private void GeneratePinBitBoard() {
        var startDirIndex = 0;
        var endDirIndex = 8;

        if (_position.Queens[_opponentColorIndex].Count == 0) {
            startDirIndex = _position.Rooks[_opponentColorIndex].Count > 0 ? 0 : 4;
            endDirIndex = _position.Bishops[_opponentColorIndex].Count > 0 ? 8 : 4;
        }

        for (var dir = startDirIndex; dir < endDirIndex; dir++) {
            var directionOffset = DirectionOffsets[dir];
            var isDiagonal = dir > 3;

            var n = NSquaresToEdge[_ownKingSquare][dir];
            var isPotentialPin = false;
            int pinSquare = -1;
            ulong rayMask = 0;

            for (var i = 0; i < n; i++) {
                var targetSquare = _ownKingSquare + directionOffset * (i + 1);
                var targetPiece = _position.Board[targetSquare];

                rayMask |= 1ul << targetSquare;

                if (targetPiece == Piece.None) continue;

                if (Piece.IsColor(targetPiece, _color)) {
                    if (!isPotentialPin) {
                        isPotentialPin = true;
                        pinSquare = targetSquare;
                    }
                    else {
                        break;
                    }
                }
                else {
                    var pieceType = Piece.PieceType(targetPiece);

                    if (pieceType == Piece.Queen || (isDiagonal && pieceType == Piece.Bishop) ||
                        (!isDiagonal && pieceType == Piece.Rook)) {
                        if (isPotentialPin) {
                            _pinnedSquares |= 1ul << pinSquare;
                        }
                        else {
                            _checkRays |= rayMask;
                            _inDoubleCheck = _inCheck;
                            _inCheck = true;
                        }
                    }

                    break;
                }
            }

            if (_inDoubleCheck) {
                break;
            }
        }
    }

    private bool IsPinned(int square) => ((_pinnedSquares >> square) & 1) != 0;
    private bool IsAttacked(int square) => ((_opponentAttackMap >> square) & 1) != 0;
    private bool PreventsCheck(int square) => ((_checkRays >> square) & 1) != 0;

    #endregion

    #region Move Generation

    private void GenerateKingMoves() {
        var startSquare = _ownKingSquare;
        for (var i = 0; i < KingMoves[startSquare].Length; i++) {
            var targetSquare = KingMoves[startSquare][i];
            var targetPiece = _position.Board[targetSquare];

            if (Piece.IsColor(targetPiece, _color)) continue;
            if (IsAttacked(targetSquare)) continue;

            _moves.Add(new Move(startSquare, targetSquare));

            var isCapture = Piece.IsColor(targetPiece, _opponentColor);
            if (!_inCheck && !isCapture) {
                if (targetSquare == F1 && _position.WhiteCanCastleKingSide || targetSquare == F8 && _position.BlackCanCastleKingSide) {
                    var castleKingSideSquare = targetSquare + 1;
                    if (targetPiece == Piece.None && _position.Board[castleKingSideSquare] == Piece.None) {
                        if (!IsAttacked(castleKingSideSquare)) {
                            _moves.Add(new Move(startSquare, castleKingSideSquare, Move.Flag.Castling));
                        }
                    }
                }
                else if (targetSquare == D1 && _position.WhiteCanCastleQueenSide || targetSquare == D8 && _position.BlackCanCastleQueenSide) {
                    var castleQueenSideSquare = targetSquare - 1;
                    if (_position.Board[castleQueenSideSquare] == Piece.None &&
                        _position.Board[castleQueenSideSquare - 1] == Piece.None && targetPiece == Piece.None) {
                        if (!IsAttacked(castleQueenSideSquare)) {
                            _moves.Add(new Move(startSquare, castleQueenSideSquare, Move.Flag.Castling));
                        }
                    }
                }
            }
        }
    }

    private void GenerateSlidingMoves() {
        void GenerateSlidingPieceMoves(int startSquare, int startDirIndex, int endDirIndex) {
            var isPinned = IsPinned(startSquare);

            if (_inCheck && isPinned) return;

            for (var directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++) {
                int currentDirOffset = DirectionOffsets[directionIndex];

                if (isPinned && !IsMovingAlongRay(startSquare, currentDirOffset, _ownKingSquare)) {
                    continue;
                }

                for (var n = 0; n < NSquaresToEdge[startSquare][directionIndex]; n++) {
                    var targetSquare = startSquare + currentDirOffset * (n + 1);
                    var targetPiece = _position.Board[targetSquare];

                    if (Piece.IsColor(targetPiece, _color)) break;

                    var isCapture = targetPiece != Piece.None;
                    var movePreventsCheck = PreventsCheck(targetSquare);

                    if (movePreventsCheck || !_inCheck) {
                        _moves.Add(new Move(startSquare, targetSquare));
                    }

                    if (isCapture || movePreventsCheck) {
                        break;
                    }
                }
            }
        }

        var rooks = _position.Rooks[_colorIndex];
        for (var i = 0; i < rooks.Count; i++) {
            GenerateSlidingPieceMoves(rooks[i], 0, 4);
        }

        var bishops = _position.Bishops[_colorIndex];
        for (var i = 0; i < bishops.Count; i++) {
            GenerateSlidingPieceMoves(bishops[i], 4, 8);
        }

        var queens = _position.Queens[_colorIndex];
        for (var i = 0; i < queens.Count; i++) {
            GenerateSlidingPieceMoves(queens[i], 0, 8);
        }
    }
    
    private void GenerateKnightMoves() {
        var knights = _position.Knights[_colorIndex];
        for (var i = 0; i < knights.Count; i++) {
            var startSquare = knights[i];
            if (IsPinned(startSquare)) continue;

            for (var j = 0; j < KnightMoves[startSquare].Length; j++) {
                var targetSquare = KnightMoves[startSquare][j];
                var targetPiece = _position.Board[targetSquare];

                if (Piece.IsColor(targetPiece, _color) || _inCheck && !PreventsCheck(targetSquare)) continue;
                _moves.Add(new Move(startSquare, targetSquare));
            }
        }
    }

    private void GeneratePawnMoves() {
        void MakePromotion(int startSquare, int targetSquare) {
            _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen));

            if (_promotionMode != PromotionMode.All) return;

            _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToRook));
            _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToBishop));
            _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToKnight));
        }

        var pawns = _position.Pawns[_colorIndex];
        var moveOffset = _color == Color.White ? 8 : -8;

        var startRank = _position.WhiteToMove ? 1 : 6;
        var rankBeforePromotion = _position.WhiteToMove ? 6 : 1;

        var enPassantFile = ((int) (_position.CurrentGameState >> 4) & 15) - 1;
        var enPassantSquare = -1;
        if (enPassantFile != -1) {
            enPassantSquare = 8 * (_position.WhiteToMove ? 5 : 2) + enPassantFile;
        }


        for (var i = 0; i < pawns.Count; i++) {
            var startSquare = pawns[i];
            var rank = RankIndex(startSquare);
            var targetSquare = startSquare + moveOffset;

            if (_position.Board[targetSquare] == Piece.None) {
                if (!(IsPinned(startSquare) && !IsMovingAlongRay(startSquare, moveOffset, _ownKingSquare))) {
                    if (!_inCheck || PreventsCheck(targetSquare)) {
                        if (rank == rankBeforePromotion) {
                            MakePromotion(startSquare, targetSquare);
                        }
                        else {
                            _moves.Add(new Move(startSquare, targetSquare));
                        }
                    }

                    if (rank == startRank) {
                        targetSquare += moveOffset;
                        if (_position.Board[targetSquare] == Piece.None) {
                            if (!_inCheck || PreventsCheck(targetSquare)) {
                                _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PawnTwoForward));
                            }
                        }
                    }
                }
                
            }

            for (var j = 0; j < 2; j++) {
                if (NSquaresToEdge[startSquare][PawnAttackDirections[_colorIndex][j]] == 0) continue;

                var captureDir = DirectionOffsets[PawnAttackDirections[_colorIndex][j]];
                targetSquare = startSquare + captureDir;
                var targetPiece = _position.Board[targetSquare];

                if (IsPinned(startSquare) && !IsMovingAlongRay(startSquare, captureDir, _ownKingSquare)) continue;

                if (Piece.IsColor(targetPiece, _opponentColor)) {
                    if (_inCheck && !PreventsCheck(targetSquare)) continue;

                    if (rank == rankBeforePromotion) {
                        MakePromotion(startSquare, targetSquare);
                    }
                    else {
                        _moves.Add(new Move(startSquare, targetSquare));
                    }
                }

                if (targetSquare == enPassantSquare) {
                    var captureSquare = targetSquare + (_position.WhiteToMove ? -8 : 8);
                    if (!InCheckAfterEnPassant(startSquare, targetSquare, captureSquare)) {
                        _moves.Add(new Move(startSquare, targetSquare, Move.Flag.EnPassantCapture));
                    }
                }
            }
        }
    }

    private static bool IsMovingAlongRay(int startSquare, int dirOffset, int targetSquare) {
        var result = (targetSquare - startSquare) % dirOffset == 0 || (startSquare - targetSquare) % dirOffset == 0;
        
        if (Math.Abs(dirOffset) == 1 )
        {
            result &= Math.Abs(targetSquare - startSquare) < 7; 
        }

        return result;
    }

    private bool InCheckAfterEnPassant(int startSquare, int targetSquare, int enPassantCaptureSquare) {
        bool CheckAfterEnPassant() {

            foreach (var dirIndex in new List<int>() {2, 3}) {
                for (var i = 1; i <= NSquaresToEdge[_ownKingSquare][dirIndex]; i++) {
                    var newTargetSquare = _ownKingSquare + DirectionOffsets[dirIndex] * i;
                    var targetPiece = _position.Board[newTargetSquare];

                    if (targetPiece == Piece.None) continue;

                    if (Piece.Color(targetPiece) == _opponentColor) {
                        if (Piece.PieceType(targetPiece) == Piece.Rook || Piece.PieceType(targetPiece) == Piece.Queen)
                            return true;
                    }

                    break;
                }
            }

            return false;
        }

        _position.Board[targetSquare] = _position.Board[startSquare];
        _position.Board[startSquare] = Piece.None;
        _position.Board[enPassantCaptureSquare] = Piece.None;

        var inCheck = CheckAfterEnPassant();

        _position.Board[targetSquare] = Piece.None;
        _position.Board[startSquare] = Piece.Pawn | _color;
        _position.Board[enPassantCaptureSquare] = Piece.Pawn | _opponentColor;

        return inCheck;
    }

    #endregion
}