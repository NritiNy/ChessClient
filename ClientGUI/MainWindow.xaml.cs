using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ChessEngine.BoardRepresentation;
using ChessEngine.Moves;
using ClientGUI.Play;
using ChessColor = ChessEngine.BoardRepresentation.Color;


namespace ClientGUI {

    public partial class MainWindow {
        
        // general settings
        private int _orientation = ChessColor.White;

        // match statistics
        public AbstractMatch CurrentAbstractMatch { get; private set; } = new Analysis();
        
        // game statistics
        private List<Move> _possibleMoves;
        private bool[] _validMoves = new bool[64];
        private int _startSquare = -1;
        private int _targetSquare = -1;
        
        public MainWindow() {
            InitializeComponent();

            DrawBoard();
            _possibleMoves = MoveGenerator.Instance.GenerateLegalMoves(CurrentAbstractMatch.CurrentGame.Position);
        }


        #region Update UI
        
        private void DrawBoard() {
            foreach (var child in BoardGrid.Children) {
                if (child is Button b) {
                    var (rank, file ) = (7 - (Grid.GetRow(b) - 1), Grid.GetColumn(b) - 1);

                    var isDarkSquare = (rank + file) % 2 != 1;
                    var color = isDarkSquare ? Brushes.Sienna : Brushes.Bisque;
                    
                    if (rank * 8 + file == _startSquare && _startSquare > -1) {
                        if (_targetSquare > -1) 
                            color = isDarkSquare ? Brushes.DarkGoldenrod : Brushes.Tan;
                        else 
                            color = isDarkSquare ? Brushes.Peru : Brushes.Khaki;
                    } else if (rank * 8 + file == _targetSquare &&_targetSquare > -1) {
                        color = isDarkSquare ? Brushes.Peru : Brushes.Khaki;
                    } else if (_validMoves[rank * 8 + file]) {
                        color = isDarkSquare ? Brushes.Brown : Brushes.Crimson;
                    }

                    /*if ((MoveGenerator.Instance._opponentAttackMap >> square & 1) != 0)
                        color = Brushes.Green;*/

                    b.Background = color;

                    var pos = _orientation == ChessColor.White ? rank * 8 + file : (7 - rank) * 8 + 7 - file;
                    var piece = Piece.PieceType(CurrentAbstractMatch.CurrentGame.Board[pos]);
                    var pieceColor = Piece.Color(CurrentAbstractMatch.CurrentGame.Board[pos]);

                    if (piece > Piece.None) {
                        var resourceKey = pieceColor == ChessColor.White ? "White" : "Black";
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
                    else {
                        b.Content = null;
                    }
                }
                else if (child is TextBlock tb) {
                    var (row, column ) = (Grid.GetRow(tb), Grid.GetColumn(tb));

                    if (row == 0 || row == 9) {
                        tb.Text = ((char) (97 + (_orientation == ChessColor.White ? column - 1 : 7 - (column - 1) ))).ToString();
                    }
                    else if (column == 0 || column == 9){
                        tb.Text = (_orientation == ChessColor.White ? 9 - row : row).ToString();
                    }
                }
            }
        }
        
        #endregion

        #region Handle Resizing events
        
        private void AdjustBoardSize() {
            var availableHeight = BoardContainer.ActualHeight;
            var availableWidth = Math.Min(BoardContainer.ActualWidth,
                ActualWidth - (2 * GameInformationSidebar.ActualWidth + 20));

            if (availableHeight > availableWidth) {
                BoardGrid.Width = availableWidth - 20;
                BoardGrid.Height = BoardGrid.Width;
            }
            else if (availableWidth > availableHeight) {
                BoardGrid.Height = availableHeight - 20;
                BoardGrid.Width = BoardGrid.Height;
            }
        }
        
        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e) => AdjustBoardSize();
        
        private void SidebarExpanded(object sender, RoutedEventArgs e) {
            try {
                AdjustBoardSize();
            }
            catch {
                // ignored
            }
        }
        
        #endregion

        private void SquareClicked(object sender, RoutedEventArgs e) {
            if (sender is not Button b) return;
            //if (!_currentMatch.InMatch) return;
            
            int rank, file;
            if (_orientation == ChessColor.White) {
                rank = 7 - (Grid.GetRow(b) - 1);
                file = Grid.GetColumn(b) - 1;
            }
            else {
                rank = Grid.GetRow(b) - 1;
                file = 7 - (Grid.GetColumn(b) - 1);
            }

            void HandleStartSquareClicked() {
                if (!Piece.IsColor(CurrentAbstractMatch.CurrentGame.Board[rank * 8 + file], CurrentAbstractMatch.CurrentGame.ColorToMove) || CurrentAbstractMatch.CurrentGame.Board[rank * 8 + file] == Piece.None) {
                    _startSquare = -1;
                    _targetSquare = -1;
                    
                    DrawBoard();
                    return;
                }
                
                _startSquare = rank * 8 + file;
                foreach (var move in _possibleMoves) {
                    if (move.Start == _startSquare) 
                        _validMoves[move.Target] = true;
                }

                DrawBoard();
            }

            void HandleTargetSquareClicked() {
                _validMoves = new bool[64];
                
                if (Piece.IsColor(CurrentAbstractMatch.CurrentGame.Board[rank * 8 + file], CurrentAbstractMatch.CurrentGame.ColorToMove)) {
                    HandleStartSquareClicked();
                    
                    return;
                }
                
                _targetSquare = rank * 8 + file;
                var tmpMove = new Move(_startSquare, _targetSquare);
                var allMoves = MoveGenerator.Instance.GenerateLegalMoves(CurrentAbstractMatch.CurrentGame.Position);
                
                foreach (var move in allMoves) {
                    if (move == tmpMove) {
                        CurrentAbstractMatch.CurrentGame.MakeMove(move);
                        _possibleMoves = MoveGenerator.Instance.GenerateLegalMoves(CurrentAbstractMatch.CurrentGame.Position);
                        break;
                    }
                }

                DrawBoard();
                
                _startSquare = -1;
                _targetSquare = -1;
            }
            
            if (_startSquare < 0) {
                HandleStartSquareClicked();
            } else if (_targetSquare < 0 && rank * 8 + file != _startSquare) {
                HandleTargetSquareClicked();
            }
        }
        
        

        #region Handle Sidebar events

        private void NewGame(object sender, RoutedEventArgs e) {
            var newGame = new NewGameWindow() {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var result = newGame.ShowDialog();
            if (result.HasValue  && result.Value) newGame.StartNewGame(); 
        }
        
        private void OpenSettings(object sender, RoutedEventArgs e) {
            var settings = new SettingsWindow {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var result = settings.ShowDialog();
            if (result.HasValue  && result.Value) settings.ApplySettings(); 
        }
        #endregion

        
        
        
    }
}