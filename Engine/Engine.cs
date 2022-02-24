using ChessEngine;
using ChessEngine.Moves;
using ChessEngine.Search;

Engine.Run(args);

public static class Engine {

    private static bool _initialized = false;
    public static readonly Search Search = Search.Instance;


    public static void Init() {
        if (_initialized) return;
        _initialized = true;
        
        Globals.Init();
        PrecomputedMoveData.Init();
        
        Search.StartThreads();
    }

    public static void Run(string[] args) {
        UciHandler.Loop(args);
    }
}


