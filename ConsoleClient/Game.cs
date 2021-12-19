namespace ConsoleClient;

public class Game
{
    public static void Main(string[] args)
    {
        var game = new Game();
        game.PrintBoard();
    }

    private Position _position;

    public Game(Position? initialPosition = null)
    {
        this._position = initialPosition ?? Position.Start;
    }

    public void PrintBoard(int colorToPlay = Piece.Black)
    {
        var board = this._position.Board;
        
        var (s, e) = colorToPlay == Piece.White ? (8, 0) : (1, 9);
        var dir = s < e ? 1 : -1;

        Console.Write("      ");
        for (int i = 0; i < 8; i++)
        {
            Console.Write($"  {(char)(colorToPlay == Piece.White ? i + 97 : 104 - i)}   ");
        }
        Console.Write("\n");
        Console.WriteLine("     +-----+-----+-----+-----+-----+-----+-----+-----+");
        
        for (int r = 7; r >= 0; r--)
        {
            Console.Write($"  {(colorToPlay == Piece.White ? r + 1 : 8 - r)}  |");
            for (int j = 0; j < 8; j++)
            {
                var row = colorToPlay == Piece.White ? 7 - r : r;
                var col = colorToPlay == Piece.White ? j : 7 - j;
                
                char c = Piece.PieceType(board[row, col]) switch
                {
                    Piece.Pawn => 'p',
                    Piece.Knight => 'n',
                    Piece.Bishop => 'b',
                    Piece.Rook => 'r',
                    Piece.Queen => 'q',
                    Piece.King => 'k',
                    _ => ' '
                };
                c = Piece.Color(board[row, col]) == Piece.White ? Char.ToUpper(c) : Char.ToLower(c);
                Console.Write($"  {c}  |");
            }
            Console.Write($"  {(colorToPlay == Piece.White ? r + 1 : 8 - r)}\n");
            Console.WriteLine("     +-----+-----+-----+-----+-----+-----+-----+-----+");
        }
        
        Console.Write("      ");
        for (int i = 0; i < 8; i++)
        {
            Console.Write($"  {(char)(colorToPlay == Piece.White ? i + 97 : 104 - i)}   ");
        }
        Console.Write("\n");
        
        
    }
}