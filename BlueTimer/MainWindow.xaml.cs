using System;
using System.Windows;
using System.Windows.Threading;

namespace BlueTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private members

        private int mMinutes = 0, mSeconds = 0, mHours = 0;

        private bool mIsSubtimerInProcess = false;

        private DispatcherTimer mMainTimer = new DispatcherTimer();
        private DispatcherTimer mSubtimer = new DispatcherTimer();

        /// <summary>
        /// Should we store time values before launching
        /// </summary>
        private bool mShouldStoreTimeBeforeLaunch = true;

        private int mBeforeLaunchHours = 0, mBeforeLaunchMinutes = 0, mBeforeLaunchSeconds = 0;
        private int mBeforeLaunchSubMinutes = 0, mBeforeLaunchSubSeconds = 0;

        private System.Windows.Forms.NotifyIcon mNotifyIcon;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            mMainTimer.Tick += new EventHandler(mMainTimer_Tick);
            mMainTimer.Interval = new TimeSpan(0, 0, 1);
        }

        private void mMainTimer_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void myWindow_StateChanged(object sender, EventArgs e)
        {

        }

        private void TextBoxNumberValidation(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Preset_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_StartPause_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Stop_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
