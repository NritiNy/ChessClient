namespace ClientGUI.Play;

public enum PlayerType {
    Human,
    Engine,
    JChessServer
}

public abstract class Player {

    public readonly string DisplayName;
    public int ID => GetHashCode();
    public readonly PlayerType Type;

    protected Player(string displayName, PlayerType type) {
        DisplayName = displayName;
        Type = type;
    }
}

public sealed class Human : Player {

    public Human(string displayName) : base(displayName, PlayerType.Human) {
        
    }

}

public sealed class Engine : Player {
    
    public Engine(string displayName) : base(displayName, PlayerType.Engine) {
        
    }
}