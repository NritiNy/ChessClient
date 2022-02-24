using ChessEngine.BoardRepresentation;

namespace ChessEngine.Search; 

public class Search {
    
    #region Singleton
    private static Search? _instance;

    public static Search Instance => _instance ??= new Search();

    private Search() { }
    #endregion

    private Position _position = null!;
    private Limits _limits;

    public volatile bool Stop = false;
    public volatile bool Ponder = false;

    
    public void StartThreads() {
        // TODO: set up thread(s) for calculation instead of blocking the main thread
    }

    public void StartSearch(Position position, Limits limits) {
        _position = position;
        _limits = limits;

        var tmpMovesToSearch = limits.MovesToSearch;
        limits.MovesToSearch.Clear();

        _ = _position.GenerateLegalMoves(out var legalMoves);
        if (tmpMovesToSearch.Count == 0) {
            limits.MovesToSearch = legalMoves;
        }
        else {
            foreach (var move in legalMoves) {
                if (tmpMovesToSearch.Contains(move))
                    limits.MovesToSearch.Add(move);
            }
        }
        
        // TODO: order moves
        
        DoSearch();
    }

    private void DoSearch() {
        if (_limits.PerftDepth > 0) {
            Perft.Run(_position, _limits.PerftDepth);
            return;
        }
        
        // TODO: actually start searching
    }

    public void Reset() {

    }
}