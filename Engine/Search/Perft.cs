using ChessEngine.BoardRepresentation;

namespace ChessEngine.Search; 

public static class Perft {
    
    public static void Run(Position position, int depth) {
        ulong Perft(Position pos, int d) {
            ulong nodes = 0;

            var nMoves = pos.GenerateLegalMoves(out var moves);

            if (d == 1)
                return (ulong) nMoves;

            for (var i = 0; i < nMoves; i++) {
                pos.MakeMove(moves[i]);
                var n = Perft(pos, d - 1);
                nodes += n;

                if (d == depth) {
                    Console.WriteLine($"{moves[i]}: {n}");
                }
                pos.UnmakeMove(moves[i]);
            }

            return nodes;
        }
        
        var calculated = Perft(position, depth);

        Console.WriteLine();
        Console.WriteLine($"Number of calculated positions: {calculated.ToString(), 20}");
    }
}