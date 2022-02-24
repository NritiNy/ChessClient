using System.Windows;

namespace ClientGUI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void CloseWindowClicked(object sender, RoutedEventArgs e) {
            MainWindow?.Close();
        }

        private void MaximiseWindowClicked(object sender, RoutedEventArgs e) {
            if (MainWindow is not null) {
                MainWindow.WindowState = MainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                
                //TODO: adapt icon to match window state
            }
        }

        private void MinimiseWindowClicked(object sender, RoutedEventArgs e) {
            if (MainWindow is not null) MainWindow.WindowState = WindowState.Minimized;
        }
    }
}