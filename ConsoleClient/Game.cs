using ChessEngine;

namespace ConsoleClient;

public class Game
{
    public static void Main(string[] args)
    {
        var game = new Game();
        game.PrintBoard();
    }

    private Position _position;

    private Game(Position? initialPosition = null)
    {
        _position = initialPosition ?? Position.InitialPosition;
    }

    private void PrintBoard(int colorToPlay = Color.White)
    {
        Console.Write("      ");
        for (int i = 0; i < 8; i++)
        {
            Console.Write($"  {(char)(colorToPlay == Color.White ? i + 97 : 104 - i)}   ");
        }
        Console.Write("\n");
        Console.WriteLine("     +-----+-----+-----+-----+-----+-----+-----+-----+");
        
        for (int r = 7; r >= 0; r--)
        {
            Console.Write($"  {(colorToPlay == Color.White ? r + 1 : 8 - r)}  |");
            for (int j = 0; j < 8; j++)
            {
                var row = colorToPlay == Color.White ? r : 7 - r;
                var col = colorToPlay == Color.White ? j : 7 - j;
                
                char c = Piece.PieceType(_position.Board[row * 8 + col]) switch
                {
                    Piece.Pawn => 'p',
                    Piece.Knight => 'n',
                    Piece.Bishop => 'b',
                    Piece.Rook => 'r',
                    Piece.Queen => 'q',
                    Piece.King => 'k',
                    _ => ' '
                };
                c = Piece.Color(_position.Board[row * 8 + col]) == Color.White ? Char.ToUpper(c) : Char.ToLower(c);
                Console.Write($"  {c}  |");
            }
            Console.Write($"  {(colorToPlay == Piece.White ? r + 1 : 8 - r)}\n");
            Console.WriteLine("     +-----+-----+-----+-----+-----+-----+-----+-----+");
        }
        
        Console.Write("      ");
        for (int i = 0; i < 8; i++)
        {
            Console.Write($"  {(char)(colorToPlay == Color.White ? i + 97 : 104 - i)}   ");
        }
        Console.Write("\n");
        
        
    }
}