using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChessEngine;
using Color = ChessEngine.Color;


namespace ClientGUI {

    public partial class MainWindow : Window {

        private Position _position;
        
        public MainWindow() {
            InitializeComponent();
            
            this._position = Position.InitialPosition;
            
            DrawBoard();
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            var availableHeight = Math.Min(e.NewSize.Height, BoardContainer.ActualHeight);
            var availableWidth = Math.Min(e.NewSize.Width, BoardContainer.ActualWidth);

            if (availableHeight > availableWidth) {
                BoardGrid.Width = availableWidth - 20;
                BoardGrid.Height = BoardGrid.Width;
            } else if (availableWidth > availableHeight) {
                BoardGrid.Height = availableHeight - 20;
                BoardGrid.Width = BoardGrid.Height;
            }
        }

        private void DrawBoard(int orientation = Color.Black) {
            foreach (var child in BoardGrid.Children) {
                if (child is Button b) {
                    var (rank, file ) = (7 - (Grid.GetRow(b) - 1), Grid.GetColumn(b) - 1);

                    var color = (rank + file) % 2 != 1 ? Brushes.Sienna : Brushes.Bisque;
                    b.Background = color;

                    var pos = orientation == Color.White ? rank * 8 + file : (7 - rank) * 8 + 7 - file;
                    var piece = Piece.PieceType(_position.Board[pos]);
                    var pieceColor = Piece.Color(_position.Board[pos]);

                    if (piece > Piece.None) {
                        var resourceKey = pieceColor == Color.White ? "White" : "Black";
                        resourceKey += piece switch {
                            Piece.Pawn => "Pawn",
                            Piece.Knight => "Knight",
                            Piece.Bishop => "Bishop",
                            Piece.Rook => "Rook",
                            Piece.Queen => "Queen",
                            Piece.King => "King",
                            _ => ""
                        };
                        
                        b.Content = new Image {Source = (BitmapImage)FindResource(resourceKey)};
                    }
                }
                else if (child is TextBlock tb) {
                    var (row, column ) = (Grid.GetRow(tb), Grid.GetColumn(tb));

                    if (row == 0 || row == 9) {
                        tb.Text = ((char) (97 + (orientation == Color.White ? column - 1 : 7 - (column - 1) ))).ToString();
                    }
                    else if (column == 0 || column == 9){
                        tb.Text = (orientation == Color.White ? 9 - row : row).ToString();
                    }
                }
            }
        }
    }
}