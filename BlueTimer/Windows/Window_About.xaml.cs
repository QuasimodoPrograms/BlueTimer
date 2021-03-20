using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace BlueTimer
{
    /// <summary>
    /// Interaction logic for Window_About.xaml
    /// </summary>
    public partial class Window_About : Window
    {
        #region VARIABLES AND ON LOAD

        #region Variables

        MainWindow owner;

        private string mWebsite = "https://www.youtube.com/channel/UCtKDQALT9QMSg2Vet3zLRpQ?sub_confirmation=1";

        #endregion

        #region On Load

        public Window_About()
        {
            InitializeComponent();

            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            image_Sub.ToolTip = assemblyName;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            tblc_Subheader.Text = string.Format(" - Version {0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            owner = Owner as MainWindow;
        }

        #endregion

        #endregion


        private void DragWithHeader(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(mWebsite);
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_Visit_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(mWebsite);
        }


    }
}
