using ChessEngine.BoardRepresentation;

namespace ChessEngine.Moves;

public static class PrecomputedMoveData {
    public static readonly sbyte[] DirectionOffsets = {8, -8, -1, 1, 7, -7, 9, -9};

    public static readonly byte[][] NSquaresToEdge;

    public static readonly byte[][] KingMoves;
    public static readonly ulong[] KingAttackBitBoards;
    public static readonly ulong[][] PawnAttackBitBoards;
    public static readonly byte[][] KnightMoves;
    public static readonly ulong[] KnightAttackBitBoards;
    public static readonly byte[][] BishopMoves;
    public static readonly byte[][] RookMoves;
    public static readonly byte[][] QueenMoves;

    public static readonly byte[][] PawnAttackDirections = {
        new byte[] {4, 6},
        new byte[] {7, 5}
    };

    private static readonly sbyte[] KnightJumpDeltas = {15, 17, -17, -15, 10, -6, 6, -10};

    public static void Init() {
        // only used to force static initialization of move data
    }
    static PrecomputedMoveData() {
        NSquaresToEdge = new byte[64][];

        KingMoves = new byte[64][];
        KingAttackBitBoards = new ulong[64];
        PawnAttackBitBoards = new ulong[64][];
        KnightMoves = new byte[64][];
        KnightAttackBitBoards = new ulong[64];
        BishopMoves = new byte[64][];
        RookMoves = new byte[64][];
        QueenMoves = new byte[64][];

        for (sbyte idx = 0; idx < 64; idx++) {
            var rank = idx / 8;
            var file = idx - rank * 8;

            var up = 7 - rank;
            var down = rank;
            var left = file;
            var right = 7 - file;

            NSquaresToEdge[idx] = new byte[8];
            NSquaresToEdge[idx][0] = Convert.ToByte(up);
            NSquaresToEdge[idx][1] = Convert.ToByte(down);
            NSquaresToEdge[idx][2] = Convert.ToByte(left);
            NSquaresToEdge[idx][3] = Convert.ToByte(right);
            NSquaresToEdge[idx][4] = Convert.ToByte(Math.Min(up, left));
            NSquaresToEdge[idx][5] = Convert.ToByte(Math.Min(down, right));
            NSquaresToEdge[idx][6] = Convert.ToByte(Math.Min(up, right));
            NSquaresToEdge[idx][7] = Convert.ToByte(Math.Min(down, left));
            
            
            var legalKingMoves = new List<byte>();
            foreach (var kmDelta in DirectionOffsets) {
                var targetSquare = idx + kmDelta;
                if (targetSquare is < 0 or >= 64) continue;

                var kRank = targetSquare / 8;
                var kFile = targetSquare - kRank * 8;

                if (Math.Max(Math.Abs(rank - kRank), Math.Abs(file - kFile)) == 1) {
                    legalKingMoves.Add((byte) targetSquare);
                    KingAttackBitBoards[idx] |= 1ul << targetSquare;
                }
            }
            KingMoves[idx] = legalKingMoves.ToArray();
            
            PawnAttackBitBoards[idx] = new ulong[2];
            if (file > 0) {
                if (rank < 7) {
                    PawnAttackBitBoards[idx][Position.WhiteIndex] |= 1ul << (idx + 7);
                }
                if (rank > 0) {
                    PawnAttackBitBoards[idx][Position.BlackIndex] |= 1ul << (idx - 9);
                }
            }
            if (file < 7) {
                if (rank < 7) {
                    PawnAttackBitBoards[idx][Position.WhiteIndex] |= 1ul << (idx + 9);
                }
                if (rank > 0) {
                    PawnAttackBitBoards[idx][Position.BlackIndex] |= 1ul << (idx - 7);
                }
            }

            
            var legalKnightJumps = new List<byte>();
            foreach (int kjDelta in KnightJumpDeltas) {
                var targetSquare = idx + kjDelta;
                if (targetSquare is < 0 or >= 64) continue;

                var kRank = targetSquare / 8;
                var kFile = targetSquare - kRank * 8;

                if (Math.Max(Math.Abs(rank - kRank), Math.Abs(file - kFile)) == 2) {
                    legalKnightJumps.Add((byte) targetSquare);
                    KnightAttackBitBoards[idx] |= 1ul << targetSquare;
                }
            }
            KnightMoves[idx] = legalKnightJumps.ToArray();

            var legalRookMoves = new List<byte>();
            for (var directionIndex = 0; directionIndex < 4; directionIndex++) {
                int cdOffset = DirectionOffsets[directionIndex];
                for (var n = 0; n < NSquaresToEdge[idx][directionIndex]; n++) {
                    var targetSquare = idx + cdOffset * (n + 1);
                    legalRookMoves.Add((byte) targetSquare);
                }
            }
            RookMoves[idx] = legalRookMoves.ToArray();
            
            var legalBishopMoves = new List<byte>();
            for (var directionIndex = 4; directionIndex < 8; directionIndex++) {
                int cdOffset = DirectionOffsets[directionIndex];
                for (var n = 0; n < NSquaresToEdge[idx][directionIndex]; n++) {
                    var targetSquare = idx + cdOffset * (n + 1);
                    legalBishopMoves.Add((byte) targetSquare);
                }
            }
            BishopMoves[idx] = legalBishopMoves.ToArray();

            QueenMoves[idx] = legalRookMoves.Concat(legalBishopMoves).ToArray();
        }
    }
}