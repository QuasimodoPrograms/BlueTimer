using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using QP = QP_Helpers.QP_Helpers;

namespace BlueTimer
{
    /// <summary>
    /// Interaction logic for Window_Buy.xaml
    /// </summary>
    public partial class Window_Buy : Window
    {
        public Window_Buy()
        {
            InitializeComponent();
            Focus();
        }

        private void DragWithHeader(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void btn_Register_Click(object sender, RoutedEventArgs e)
        {
            if (QP.IsLicensed(tb_Password.Text, "BlueTimer.LicenseKeys.txt"))
            {
                string fullRegistryPath = @"HKEY_CURRENT_USER\Software\Quasimodo Programs\BT";
                string registryValueName = "Key";

                QP.SetRegister(fullRegistryPath, registryValueName, tb_Password.Text);
                QP._isLicensed = true;
                MessageBox.Show("Successful registration");

                (Owner as MainWindow).UnlockFull();

                Close();
            }
            else
            {
                MessageBox.Show("The key is not licensed.");
            }
        }

        private void btn_Buy_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.youtube.com/channel/UCtKDQALT9QMSg2Vet3zLRpQ?sub_confirmation=1");
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


    }
}
