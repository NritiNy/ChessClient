using System.Globalization;
using ChessEngine.BoardRepresentation;

namespace ClientGUI; 

public class Match {
    
    public enum PlayerType { Human, Engine, JChessServer}

    public bool InMatch { get; private set; } = false;
    public Position CurrentGame { get; } = Position.InitialPosition;

    
    public PlayerType Player1Type { get; }
    public string Player1Name { get; }
    public double Player1Score { get; private set; } = 0.0;
    public string Player1ScoreString => Player1Score.ToString("N1", CultureInfo.InvariantCulture);
    public int CurrentPlayer1Color { get; private set; } = Color.White;
    
    public PlayerType Player2Type { get; }
    public string Player2Name { get; }
    public double Player2Score { get; private set; } = 0.0;
    public string Player2ScoreString => Player2Score.ToString("N1", CultureInfo.InvariantCulture);
    public int CurrentPlayer2Color { get; private set; } = Color.Black;


    public Match(string player1Name = "Player", string player2Name = "Opponent", PlayerType player1Type = PlayerType.Human, PlayerType player2Type = PlayerType.Human) {
        Player1Name = player1Name;
        Player1Type = player1Type;
        
        Player2Name = player2Name;
        Player2Type = player2Type;
    }
}