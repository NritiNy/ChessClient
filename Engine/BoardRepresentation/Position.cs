namespace ChessEngine.BoardRepresentation;

public class Position {
    public static Position InitialPosition => new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");


    public int[] Board = null!;

    public const int WhiteIndex = 0;
    public const int BlackIndex = 1;

    public int ColorToMove;
    public bool WhiteToMove => ColorToMove == Color.White;
    public int OpponentColor => ColorToMove * 2 % 24;
    public int ColorToMoveIndex => ColorToMove / 8 - 1;


    private Stack<uint> _gameStateHistory = new();
    public uint CurrentGameState;

    private int _plyCount;
    private int _fiftyMoveCounter;

    public int MoveCounter => _plyCount / 2 + 1;

    public int[] KingSquare = null!;
    public PieceList[] Knights = null!;
    public PieceList[] Bishops = null!;
    public PieceList[] Rooks = null!;
    public PieceList[] Queens = null!;
    public PieceList[] Pawns = null!;

    private PieceList[] _allPieceLists = Array.Empty<PieceList>();


    private const uint WhiteCastleKingSideMask = 0b1111111111111110;
    private const uint WhiteCastleQueenSideMask = 0b1111111111111101;
    private const uint BlackCastleKingSideMask = 0b1111111111111011;
    private const uint BlackCastleQueenSideMask = 0b1111111111110111;

    private const uint WhiteCastleMask = WhiteCastleKingSideMask & WhiteCastleQueenSideMask;
    private const uint BlackCastleMask = BlackCastleKingSideMask & BlackCastleQueenSideMask;

    public bool WhiteCanCastleKingSide => (CurrentGameState & WhiteCastleKingSideMask) == 1 << 0;
    public bool WhiteCanCastleQueenSide => (CurrentGameState & WhiteCastleQueenSideMask) == 1 << 1;
    public bool BlackCanCastleKingSide => (CurrentGameState & BlackCastleKingSideMask) == 1 << 2;
    public bool BlackCanCastleQueenSide => (CurrentGameState & BlackCastleQueenSideMask) == 1 << 3;

    private PieceList GetPieceList(int pieceType, int colorIndex) => _allPieceLists[colorIndex * 8 + pieceType];

    public ulong ZobristKey;
    public Stack<ulong> PositionHistory = new();


    public Position(string? fenString = null) {
        if (fenString is not null) LoadFenString(fenString);
    }

    private void Init() {
        Board = new int[64];
        KingSquare = new int[2];

        _gameStateHistory = new Stack<uint>();
        ZobristKey = 0;
        PositionHistory = new Stack<ulong>();
        _plyCount = 0;
        _fiftyMoveCounter = 0;

        Knights = new PieceList[] {new(10), new(10)};
        Bishops = new PieceList[] {new(10), new(10)};
        Rooks = new PieceList[] {new(10), new(10)};
        Queens = new PieceList[] {new(9), new(9)};
        Pawns = new PieceList[] {new(8), new(8)};

        var emptyList = new PieceList(0);
        _allPieceLists = new[] {
            emptyList,
            Pawns[WhiteIndex],
            Knights[WhiteIndex],
            emptyList,
            Bishops[WhiteIndex],
            Rooks[WhiteIndex],
            Queens[WhiteIndex],
            emptyList,
            emptyList,
            Pawns[BlackIndex],
            Knights[BlackIndex],
            emptyList,
            Bishops[BlackIndex],
            Rooks[BlackIndex],
            Queens[BlackIndex],
        };
    }


    public bool IsLegalMove(Move move) => MoveGenerator.Instance.IsLegalMove(this, move);

    public int GenerateLegalMoves(out List<Move> moves) {
        moves = MoveGenerator.Instance.GenerateLegalMoves(this);
        return moves.Count;
    }


    public void MakeMove(Move move) {
        //TODO: include Zobrist key updates
        
        var colorIndex = ColorToMove / 8 - 1;
        var opponentColorIndex = 1 - colorIndex;
        
        var oldCastleState = CurrentGameState & 15;
        var newCastleState = oldCastleState;
        
        
        CurrentGameState = 0;

        var startSquare = move.Start;
        var targetSquare = move.Target;
        var flag = move.MoveFlag;
        var isPromotion = move.IsPromotion;
        var isEnPassant = flag == Move.Flag.EnPassantCapture;

        var piece = Board[startSquare];
        var pieceType = Piece.PieceType(piece);
        var targetPiece = Board[targetSquare];
        var targetPieceType = Piece.PieceType(targetPiece);

        // captures
        CurrentGameState |= (ushort) (targetPieceType << 8);
        if (targetPieceType != Piece.None && !isEnPassant)
            GetPieceList(targetPieceType, opponentColorIndex).RemovePieceAtSquare(targetSquare);

        // update piece lists
        if (pieceType == Piece.King) {
            KingSquare[colorIndex] = targetSquare;
            newCastleState &= WhiteToMove ? WhiteCastleMask : BlackCastleMask;
        }
        else {
            GetPieceList(pieceType, colorIndex).MovePiece(startSquare, targetSquare);
        }

        var newPiece = piece;
        if (isPromotion) {
            switch (flag) {
                case Move.Flag.PromoteToKnight:
                    newPiece = Piece.Knight;
                    Knights[colorIndex].AddPieceAtSquare(targetSquare);
                    break;
                case Move.Flag.PromoteToBishop:
                    newPiece = Piece.Bishop;
                    Bishops[colorIndex].AddPieceAtSquare(targetSquare);
                    break;
                case Move.Flag.PromoteToRook:
                    newPiece = Piece.Rook;
                    Rooks[colorIndex].AddPieceAtSquare(targetSquare);
                    break;
                case Move.Flag.PromoteToQueen:
                    newPiece = Piece.Queen;
                    Queens[colorIndex].AddPieceAtSquare(targetSquare);
                    break;
            }

            newPiece |= ColorToMove;
            Pawns[colorIndex].RemovePieceAtSquare(targetSquare);
        }
        else { // other special moves
            switch (flag) {
                case Move.Flag.EnPassantCapture:
                    var pawnSquare = targetSquare + (WhiteToMove ? -8 : 8);
                    CurrentGameState |= (ushort) (Board[pawnSquare] << 8);
                    Board[pawnSquare] = Piece.None;
                    Pawns[opponentColorIndex].RemovePieceAtSquare(pawnSquare);
                    break;
                case Move.Flag.Castling:
                    var kingSide = targetSquare == BoardUtility.G1 || targetSquare == BoardUtility.G8;
                    var rookStart = kingSide ? targetSquare + 1 : targetSquare - 2;
                    var rookTarget = kingSide ? targetSquare - 1 : targetSquare + 1;

                    Board[rookStart] = Piece.None;
                    Board[rookTarget] = Piece.Rook | ColorToMove;
                    Rooks[colorIndex].MovePiece(rookStart, rookTarget);
                    break;
            }
        }

        // update board
        Board[targetSquare] = newPiece;
        Board[startSquare] = Piece.None;
        
        // handle initial two squares move of pawns
        if (flag == Move.Flag.PawnTwoForward) {
            var file = BoardUtility.FileIndex(targetSquare) + 1;
            CurrentGameState |= (ushort) (file << 4);
        }

        // update castling rights
        if (oldCastleState != 0) {
            if (targetSquare == BoardUtility.H1 || startSquare == BoardUtility.H1) {
                newCastleState &= WhiteCastleKingSideMask;
            } else if (targetSquare == BoardUtility.A1 || startSquare == BoardUtility.A1) {
                newCastleState &= WhiteCastleQueenSideMask;
            }
            if (targetSquare == BoardUtility.H8 || startSquare == BoardUtility.H8) {
                newCastleState &= BlackCastleKingSideMask;
            } else if (targetSquare == BoardUtility.A8 || startSquare == BoardUtility.A8) {
                newCastleState &= BlackCastleQueenSideMask;
            }
        }

        CurrentGameState |= newCastleState;
        CurrentGameState |= (uint) _fiftyMoveCounter << 14;
        _gameStateHistory.Push(CurrentGameState);
        
        // change side
        ColorToMove = WhiteToMove ? Color.Black : Color.White;
        _plyCount++;
        
        // reset fifty move counter if applicable
        if (pieceType == Piece.Pawn || targetPieceType != Piece.None) {
            PositionHistory.Clear();
            _fiftyMoveCounter = 0;
        }
        else {
            PositionHistory.Push(ZobristKey);
            _fiftyMoveCounter++;
        }
    }

    public void UnmakeMove(Move move) {
        //TODO: include Zobrist key
        
        var opponentColorIndex = ColorToMoveIndex;

        ColorToMove = OpponentColor;

        var capturedPieceType = ((int) CurrentGameState >> 8) & 63;
        var capturedPiece = capturedPieceType == 0 ? 0 : capturedPieceType | OpponentColor;

        var startSquare = move.Start;
        var targetSquare = move.Target;
        var flag = move.MoveFlag;
        var isPromotion = move.IsPromotion;
        var isEnPassant = flag == Move.Flag.EnPassantCapture;

        var endPieceType= Piece.PieceType(Board[targetSquare]);
        var pieceType = isPromotion ? Piece.Pawn : endPieceType;

        // restore normal capture
        if (capturedPieceType != 0 && !isEnPassant) {
            GetPieceList(capturedPieceType, opponentColorIndex).AddPieceAtSquare(targetSquare);
        }
        
        if (pieceType == Piece.King) {
            KingSquare[ColorToMoveIndex] = startSquare;
        } else if (!isPromotion) {
            GetPieceList(pieceType, ColorToMoveIndex).MovePiece(targetSquare, startSquare);
        }

        Board[startSquare] = pieceType | ColorToMove;
        Board[targetSquare] = capturedPiece;

        if (isPromotion) {
            Pawns[ColorToMoveIndex].AddPieceAtSquare(startSquare);
            switch (flag) {
                case Move.Flag.PromoteToKnight:
                    Knights[ColorToMoveIndex].RemovePieceAtSquare(targetSquare);
                    break;
                case Move.Flag.PromoteToBishop:
                    Bishops[ColorToMoveIndex].RemovePieceAtSquare(targetSquare);
                    break;
                case Move.Flag.PromoteToRook:
                    Rooks[ColorToMoveIndex].RemovePieceAtSquare(targetSquare);
                    break;
                case Move.Flag.PromoteToQueen:
                    Queens[ColorToMoveIndex].RemovePieceAtSquare(targetSquare);
                    break;
            }
        } else if (isEnPassant) {
            var epIndex = targetSquare + (ColorToMove == Piece.White ? -8 : 8);
            Board[targetSquare] = 0;
            Board[epIndex] = capturedPiece;
            Pawns[opponentColorIndex].AddPieceAtSquare (epIndex);
        } else if (flag == Move.Flag.Castling) {
            var kingside = targetSquare is BoardUtility.G1 or BoardUtility.G8;
            var castlingRookFromIndex = kingside ? targetSquare + 1 : targetSquare - 2;
            var castlingRookToIndex = kingside ? targetSquare - 1 : targetSquare + 1;

            Board[castlingRookToIndex] = 0;
            Board[castlingRookFromIndex] = Piece.Rook | ColorToMove;

            Rooks[ColorToMoveIndex].MovePiece (castlingRookToIndex, castlingRookFromIndex);
        }

        _gameStateHistory.Pop();
        CurrentGameState = _gameStateHistory.Peek();

        _plyCount--;

        if (PositionHistory.Count > 0) PositionHistory.Pop();
    }

    #region FEN conversion

    public void LoadFenString(string fenString) {
        Init();

        var segments = fenString.Split(" ");

        // parse color to move
        ColorToMove = segments[1] == "w" ? Color.White : Color.Black;

        // parse castling rights
        var castlingRights = segments.Length > 2 ? segments[2] : "KQkq";
        var whiteCastleKingSide = castlingRights.Contains('K');
        var whiteCastleQueenSide = castlingRights.Contains('Q');
        var blackCastleKingSide = castlingRights.Contains('k');
        var blackCastleQueenSide = castlingRights.Contains('q');

        var whiteCastle = (whiteCastleKingSide ? 1 << 0 : 0) | (whiteCastleQueenSide ? 1 << 1 : 0);
        var blackCastle = (blackCastleKingSide ? 1 << 2 : 0) | (blackCastleQueenSide ? 1 << 3 : 0);

        // parse en passant target
        var epState = 0;
        if (segments.Length > 3 && BoardUtility.FileNames.Contains(segments[3][0]))
            epState = (segments[3][0] - 96) << 4;

        // set initial game state
        var initialGameState = (ushort) (whiteCastle | blackCastle | epState);
        _gameStateHistory.Push(initialGameState);
        CurrentGameState = initialGameState;

        // parse move counters
        _fiftyMoveCounter = segments.Length > 4 ? int.Parse(segments[4]) : 0;

        _plyCount = 0;
        if (segments.Length > 5) {
            _plyCount = int.Parse(segments[5]) * 2;
            _plyCount -= WhiteToMove ? 2 : 1;
        }

        // parse board
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

                    Board[rank * 8 + file] = Piece.None;
                    if (piece != Piece.None) {
                        Board[rank * 8 + file] = piece | color;

                        var squareIndex = rank * 8 + file;
                        var colorIndex = color / 8 - 1;

                        switch (piece) {
                            case Piece.Knight:
                                Knights[colorIndex].AddPieceAtSquare(squareIndex);
                                break;
                            case Piece.Bishop:
                                Bishops[colorIndex].AddPieceAtSquare(squareIndex);
                                break;
                            case Piece.Rook:
                                Rooks[colorIndex].AddPieceAtSquare(squareIndex);
                                break;
                            case Piece.Queen:
                                Queens[colorIndex].AddPieceAtSquare(squareIndex);
                                break;
                            case Piece.Pawn:
                                Pawns[colorIndex].AddPieceAtSquare(squareIndex);
                                break;
                            case Piece.King:
                                KingSquare[colorIndex] = squareIndex;
                                break;
                        }
                    }

                    file++;
                }
            }
        }
    }

    public string FenString() {
        var boardString = "";

        // convert board
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

        // convert game state
        var castlingAvailability = "";
        castlingAvailability += (CurrentGameState & 1) == 1 ? "K" : "";
        castlingAvailability += (CurrentGameState >> 1 & 1) == 1 ? "Q" : "";
        castlingAvailability += (CurrentGameState >> 2 & 1) == 1 ? "k" : "";
        castlingAvailability += (CurrentGameState >> 3 & 1) == 1 ? "q" : "";
        castlingAvailability += (CurrentGameState & 15) == 0 ? "-" : "";

        var enPassant = "-";
        file = (int) (CurrentGameState >> 4) & 15;
        if (file != 0) {
            rank = WhiteToMove ? 6 : 3;

            enPassant = ((char) (file + 97)).ToString() + rank;
        }

        return
            $"{boardString} {(ColorToMove == Color.White ? "w" : "b")} {castlingAvailability} {enPassant} {_fiftyMoveCounter} {MoveCounter}";
    }

    #endregion
}