using System.Threading;
using ChessEngine;
using ChessEngine.BoardRepresentation;
using ChessEngine.Moves;

namespace ClientGUI.Play;

public abstract class AbstractMatch {
    
    public AbstractGame CurrentGame { get; protected set; }
    
    public abstract bool IsAnalysis { get; }
    public abstract bool SupportsPremoves { get; }

    protected AbstractMatch(AbstractGame game) {
        CurrentGame = game;
    }

    public abstract void StartMatch();
    public abstract void ResetMatch();
    
    public abstract void StartNextGame();
    public abstract void OfferDraw(int playerId);
    public abstract void Resign(int playerId);
    public abstract void Rematch();
}

public abstract class AbstractGame {
    
    public enum GameStatus {NotStarted, Running, Finished}
    public enum GameResult {DrawByAgreement, DrawByRepetition, DrawTimeoutVsInsufficientMaterial, DrawByStalemate, Checkmate, Resignation}
    
    
    public GameStatus Status { get; protected set; } = GameStatus.NotStarted;
    public GameResult? Result { get; protected set; } = null;
    public Player? Winner { get; protected set; } = null;

    
    public readonly Position Position;
    public int[] Board => Position.Board;
    public int ColorToMove => Position.ColorToMove;

    public readonly Player White;
    public readonly Player Black;
    public Player PlayerToMove => ColorToMove == Color.White ? White : Black;

    protected readonly Thread GameThread;

    protected AbstractGame(Player white, Player black, Position? fromPosition = null) {
        Position = fromPosition ?? Position.InitialPosition;
        White = white;
        Black = black;

        GameThread = new Thread(GameLoop);
    }

    public abstract void MakeMove(Move move);

    protected abstract void GameLoop();

    public void StartGame() {
        GameThread.Start();
    }
}