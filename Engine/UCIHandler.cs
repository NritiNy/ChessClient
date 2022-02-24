using ChessEngine.BoardRepresentation;
using ChessEngine.Moves;
using ChessEngine.Search;

namespace ChessEngine; 

public static class UciHandler {
    private static Position _position = null!;
    public static void Loop(string[] argv) {
        _position = BoardRepresentation.Position.InitialPosition;
        string token;
        var cmd = "";

        var argc = argv.Length;
        foreach (var arg in argv) {
            cmd += arg + " ";
        }

        do {
            if (argc == 0 && (cmd = Console.ReadLine()) is null) {
                cmd = "quit";
            }
            
            token = cmd.Trim().Split(" ")[0];
            switch (token) {
                case "quit":
                case "stop":
                    Engine.Search.Stop = true;
                    break;
                case "uci":
                    Console.WriteLine("id name Jan's Engine");
                    Console.WriteLine("id author Jan Schultz");
                    SendOptions();
                    Console.WriteLine("uciok");
                    break;
                case "debug":
                    Globals.InDebugMode = cmd.Trim().Split(" ")[1] == "on";
                    break;
                case "isready":
                    Engine.Init();
                    Console.WriteLine("readyok");
                    break;
                case "setoption":
                    SetOption(cmd);
                    break;
                case "ucinewgame":
                    Engine.Search.Reset();
                    break;
                case "position":
                    Engine.Search.Reset();
                    Position(cmd);
                    break;
                case "go":
                    Go(cmd);
                    break;
                case "ponderhit":
                    Engine.Search.Ponder = false;
                    break;
                default:
                    Console.WriteLine($"Unknown command '{cmd}'");
                    break;
            }
        } while (token != "quit" && argc == 0);
    }

    private static void SendOptions() {
        // TODO
    }

    private static void SetOption(string optionString) {
        var segments = new Queue<string>(
            optionString.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        do {
            var token = segments.Dequeue();
            if (token != "name") continue;
            
            string name = "";
            while ((token = segments.Dequeue()) != "value") {
                name += token + " ";
            }
                
            if (Globals.Options.ContainsKey(name))
                Globals.Options[name].SetValue(string.Join(" ", segments));
            else {
                Console.WriteLine($"No option with name '{name}' available.");
            }
        } while (segments.Count > 0);
    }

    private static void Position(string positionString) {
        var segments = new Queue<string>(
            positionString.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

        _ = segments.Dequeue();
        string token = segments.Dequeue();
        string fen = "";

        if (token == "startpos") {
            fen = BoardRepresentation.Position.InitialPosition.FenString();
        } else if (token == "fen") {
            fen += segments.Dequeue() + " ";
            while (segments.Count > 0 && token != "moves") fen += segments.Dequeue() + " ";
        }
        else {
            return;
        }
        
        _position.LoadFenString(fen);

        if (segments.Count < 1) return;
        
        _ = segments.Dequeue();
        Move move;
        while (segments.Count > 0 && (move = new Move(segments.Dequeue())).Value != 0) {
            // make sure the move flags are included
            _position.GenerateLegalMoves(out var legalMoves);
            foreach (var m in legalMoves) {
                if (m == move) {
                    _position.MakeMove(m);
                }
            }
        }
    }

    private static void Go(string commandString) {
        var segments = new Queue<string>(
            commandString.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        
        var limits = new Limits();

        string token;
        while (segments.Count > 0) {
            token = segments.Dequeue();
            string value;

            switch (token) {
                case "searchmoves":
                    while (segments.Count > 0) {
                        token = segments.Dequeue();
                        limits.MovesToSearch.Add(new Move(token));
                    }
                    break;
                case "ponder":
                    Engine.Search.Ponder = true;
                    break;
                case "wtime":
                    value = segments.Dequeue();
                    uint.TryParse(value, out limits.TimeLimitWhite);
                    break;
                case "btime":
                    value = segments.Dequeue();
                    uint.TryParse(value, out limits.TimeLimitBlack);
                    break;
                case "winc":
                    value = segments.Dequeue();
                    uint.TryParse(value, out limits.TimeIncrementWhite);
                    break;
                case "binc":
                    value = segments.Dequeue();
                    uint.TryParse(value, out limits.TimeIncrementBlack);
                    break;
                case "movestogo":
                    value = segments.Dequeue();
                    ushort.TryParse(value, out limits.MovesUntilTimeControl);
                    break;
                case "depth":
                    value = segments.Dequeue();
                    ushort.TryParse(value, out limits.SearchDepth);
                    break;
                case "nodes":
                    value = segments.Dequeue();
                    ulong.TryParse(value, out limits.MaximumNodesToSearch);
                    break;
                case "mate":
                    value = segments.Dequeue();
                    ushort.TryParse(value, out limits.SearchForMateInX);
                    break;
                case "movetime":
                    value = segments.Dequeue();
                    uint.TryParse(value, out limits.SearchForXMilliSeconds);
                    break;
                case "infinite":
                    limits.SearchIndefinitely = true;
                    break;
                case "perft":
                    value = segments.Dequeue();
                    ushort.TryParse(value, out limits.PerftDepth);
                    break;
            }
        }
        limits.StartTime = (ulong) DateTimeOffset.Now.ToUnixTimeMilliseconds();
        
        Engine.Search.StartSearch(_position, limits);
    }
}