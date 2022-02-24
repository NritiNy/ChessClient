using ChessEngine.Moves;

namespace ChessEngine.Search; 

public struct Limits {
    public uint TimeLimitWhite = 0;
    public uint TimeLimitBlack = 0;

    public uint TimeIncrementWhite = 0;
    public uint TimeIncrementBlack  = 0;

    public ushort MovesUntilTimeControl = 0;

    public ushort SearchDepth = 0;
    public ulong MaximumNodesToSearch = 0;
    
    public List<Move> MovesToSearch = new();
    public ushort SearchForMateInX = 0;
    public ulong StartTime = 0 ;
    public uint SearchForXMilliSeconds = 0;
    public bool SearchIndefinitely = false;
    
    
    public ushort PerftDepth = 0;
}