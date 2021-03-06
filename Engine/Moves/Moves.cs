namespace ChessEngine.Moves; 

public readonly struct Move {

    public readonly struct Flag {
        public const int None = 0;
        public const int Castling = 1;
        public const int PromoteToQueen = 2;
        public const int PromoteToRook = 3;
        public const int PromoteToKnight = 4;
        public const int PromoteToBishop = 5;
        public const int PawnTwoForward = 6;
        public const int EnPassantCapture = 7;
    }

    private readonly ushort _moveValue;

    private const ushort StartMask = 0b0000000000111111;
    private const ushort TargetMask = 0b0000111111000000;
    
    public Move(ushort value) {
        _moveValue = value;
    }

    public Move(int start, int target) {
        _moveValue = (ushort) (start | (target << 6));
    }
    
    public Move(int start, int target, int flag) {
        _moveValue = (ushort) (start | target << 6 | flag << 12);
    }

    public Move(string move) {
        var startFile = move[0] - 97;
        var startRank = move[1] - '0' - 1;
        var targetFile = move[2] - 97;
        var targetRank = move[3] - '0' - 1;

        var start = startRank * 8 + startFile;
        var target = targetRank * 8 + targetFile;
        _moveValue = (ushort) (start | (target << 6));
    }


    public ushort Value => _moveValue;
    public int Start => _moveValue & StartMask;
    public int Target => (_moveValue & TargetMask) >> 6;
    public int MoveFlag => _moveValue >> 12;
    public bool IsPromotion => MoveFlag is Flag.PromoteToQueen or Flag.PromoteToRook or Flag.PromoteToKnight or Flag.PromoteToBishop;


    public override string ToString() {
        return $"{(char)(Start % 8 + 97)}{Start / 8 + 1} {(char)(Target % 8 + 97)}{Target / 8 + 1}";
    }


    private bool Equals(Move other) {
        return Start  == other.Start && Target == other.Target;
    }

    public override bool Equals(object? obj) {
        return obj is Move other && Equals(other);
    }

    public override int GetHashCode() {
        return _moveValue.GetHashCode();
    }

    public static bool operator ==(Move left, Move right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Move left, Move right)
    {
        return !(left == right);
    }
}