using System;
using System.Globalization;
using ChessEngine.BoardRepresentation;

namespace ClientGUI;

public class Match {
    public enum PlayerType {
        Human,
        Engine,
        JChessServer
    }

    public bool InMatch { get; private set; } = false;
    public Position CurrentGame { get; } = Position.InitialPosition;

    public TimeSpan TimeIncrement { get; private set; } = new (0, 0, 30);
    public string TimeIncrementString => $"+{TimeIncrement:ss}s";


    public PlayerType Player1Type { get; }
    public string Player1Name { get; }
    public double Player1Score { get; private set; } = 0.0;
    public string Player1ScoreString => Player1Score.ToString("N1", CultureInfo.InvariantCulture);
    public double Player1CaptureDifference { get; private set; } = 0.0;
    public string Player1CaptureDifferenceString {
        get {
            if (Player1CaptureDifference > 0)
                return "+" + Player1CaptureDifference.ToString("N1", CultureInfo.InvariantCulture);
            return "";
        }
    }
    public int Player1Color { get; private set; } = Color.White;
    public TimeSpan Player1GameTimerValue { get; private set; } = new (0, 0, 0, 0, 0);
    public string Player1GameTimerValueString => Player1GameTimerValue.Hours > 0
        ? Player1GameTimerValue.ToString(@"hh\:mm\:ss")
        : Player1GameTimerValue.ToString(Player1GameTimerValue.Minutes > 0 ? @"mm\:ss\" : @"mm\:ss\:ff");
    public TimeSpan Player1MoveTimerValue { get; private set; } = new(0,0,15);
    public string Player1MoveTimerValueString => Player1MoveTimerValue.ToString(@"ss\:ff");


    public PlayerType Player2Type { get; }
    public string Player2Name { get; }
    public double Player2Score { get; private set; } = 0.0;
    public string Player2ScoreString => Player2Score.ToString("N1", CultureInfo.InvariantCulture);
    public double Player2CaptureDifference { get; private set; } = 0.0;
    public string Player2CaptureDifferenceString {
        get {
            if (Player2CaptureDifference > 0)
                return "+" + Player2CaptureDifference.ToString("N1", CultureInfo.InvariantCulture);
            return "";
        }
    }
    public int Player2Color { get; private set; } = Color.Black;
    public TimeSpan Player2GameTimerValue { get; private set; } = new TimeSpan(0, 0, 0, 0, 0);
    public string Player2GameTimerValueString => Player2GameTimerValue.Hours > 0
        ? Player2GameTimerValue.ToString(@"hh\:mm\:ss")
        : Player2GameTimerValue.ToString(Player2GameTimerValue.Minutes > 0 ? @"mm\:ss\" : @"mm\:ss\:ff");
    public TimeSpan Player2MoveTimerValue { get; private set; } = new(0,0,15);
    public string Player2MoveTimerValueString => Player2MoveTimerValue.ToString(@"ss\:ff");


    public Match(string player1Name = "Player", string player2Name = "Opponent",
        PlayerType player1Type = PlayerType.Human, PlayerType player2Type = PlayerType.Human) {
        Player1Name = player1Name;
        Player1Type = player1Type;

        Player2Name = player2Name;
        Player2Type = player2Type;
    }
}