using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private string mWebsite = "https://www.youtube.com/c/MishaDubok";

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
