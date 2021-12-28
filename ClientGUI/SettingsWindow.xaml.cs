using System.Windows;

namespace ClientGUI; 

public partial class SettingsWindow : Window {
    public SettingsWindow() {
        InitializeComponent();

        this.Owner = Application.Current.MainWindow;
    }

    public void ApplySettings() {
        
    }
}