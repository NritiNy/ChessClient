namespace ChessEngine; 

public readonly  struct Move {

    public readonly struct Flag {
        public const int None = 0;
        public const int Castling = 1;
        public const int PromoteToQueen = 2;
        public const int PromoteToRook = 3;
        public const int PromoteToKnight = 4;
        public const int PromoteToBishop = 5;
        public const int PawnTwoForward = 6;
    }

    private readonly ushort _moveValue;

    private const ushort StartMask = 0b0000000000111111;
    private const ushort TargetMask = 0b0000111111000000;
    private const ushort FlagMask = 0b1111000000000000;

    public Move(ushort value) {
        _moveValue = value;
    }

    public Move(int start, int target) {
        _moveValue = (ushort) (start | target << 6);
    }
    
    public Move (int start, int target, int flag) {
        _moveValue = (ushort) (start | target << 6 | flag << 12);
    }


    public ushort Value => _moveValue;
    public int Start => _moveValue & StartMask;
    public int Target => _moveValue & TargetMask;
    public int MoveFlag => _moveValue >> 12;
    public bool IsPromotion => MoveFlag == Flag.PromoteToQueen || MoveFlag == Flag.PromoteToRook ||
                               MoveFlag == Flag.PromoteToKnight || MoveFlag == Flag.PromoteToBishop;
    
    
    public bool Equals(Move other) {
        return _moveValue == other._moveValue;
    }

    public override bool Equals(object? obj) {
        return obj is Move other && Equals(other);
    }

    public override int GetHashCode() {
        return _moveValue.GetHashCode();
    }
}