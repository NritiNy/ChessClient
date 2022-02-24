using ChessEngine;
using ChessEngine.BoardRepresentation;
using ChessEngine.Moves;

namespace ClientGUI.Play;

public sealed class Analysis : AbstractMatch {
    public override bool IsAnalysis => true;
    public override bool SupportsPremoves => false;

    public Analysis() : base(new AnalysisGame()) {
        StartMatch();
    }

    public override void StartMatch() {
        CurrentGame.StartGame();
    }

    public override void ResetMatch() {
        StartNextGame();
    }

    public override void StartNextGame() {
        CurrentGame = new AnalysisGame();
        CurrentGame.StartGame();
    }

    public override void OfferDraw(int playerId) {
        // not supported in analysis
    }

    public override void Resign(int playerId) {
        // not supported in analysis, will result in reset of position
        ResetMatch();
    }

    public override void Rematch() {
        // not supported in analysis, will result in reset of position
        ResetMatch();
    }
}

public class AnalysisGame : AbstractGame {
    public AnalysisGame(Position? initialPosition = null) : base(new Human("White"), new Human("Black"), initialPosition) { }

    public override void MakeMove(Move move) {
        if (Position.IsLegalMove(move)) Position.MakeMove(move);
        
        // TODO: update evaluation
    }

    protected override void GameLoop() {
        // no game loop needed in analysis board
    }
}