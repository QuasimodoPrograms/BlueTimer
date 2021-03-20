using BlueTimer.Properties;
using System.Windows;
using System.Windows.Input;

namespace BlueTimer
{
    public partial class Window_EditCustomPreset : Window
    {
        MainWindow owner;
        MainWindow.TimerType timerType;

        public Window_EditCustomPreset(MainWindow.TimerType timertype)
        {
            InitializeComponent();

            timerType = timertype;

            if (timerType == MainWindow.TimerType.Main)
            {
                numeric_Hours.Value = (int)Settings.Default["customHours"];
                numeric_Minutes.Value = (int)Settings.Default["customMinutes"];
                numeric_Seconds.Value = (int)Settings.Default["customSeconds"];
            }
            else
            {
                numeric_Hours.Value = (int)Settings.Default["customSubHours"];
                numeric_Minutes.Value = (int)Settings.Default["customSubMinutes"];
                numeric_Seconds.Value = (int)Settings.Default["customSubSeconds"];
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            owner = Owner as MainWindow;
        }

        private void DragWithHeader(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_Set_Click(object sender, RoutedEventArgs e)
        {
            if (timerType == MainWindow.TimerType.Main)
            {
                owner.numeric_Hours.Value = numeric_Hours.Value;
                owner.numeric_Minutes.Value = numeric_Minutes.Value;
                owner.numeric_Seconds.Value = numeric_Seconds.Value;

                Settings.Default["customHours"] = numeric_Hours.Value;
                Settings.Default["customMinutes"] = numeric_Minutes.Value;
                Settings.Default["customSeconds"] = numeric_Seconds.Value;
            }
            else
            {
                owner.numeric_SubHours.Value = numeric_Hours.Value;
                owner.numeric_SubMinutes.Value = numeric_Minutes.Value;
                owner.numeric_SubSeconds.Value = numeric_Seconds.Value;

                Settings.Default["customSubHours"] = numeric_Hours.Value;
                Settings.Default["customSubMinutes"] = numeric_Minutes.Value;
                Settings.Default["customSubSeconds"] = numeric_Seconds.Value;
            }

            Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


    }
}
