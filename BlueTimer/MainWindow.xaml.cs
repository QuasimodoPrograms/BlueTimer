using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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

            mSubtimer.Tick += new EventHandler(mSubtimer_Tick);
            mSubtimer.Interval = new TimeSpan(0, 0, 1);

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Title += string.Format(" - Version {0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        private void mMainTimer_Tick(object sender, EventArgs e)
        {
            #region If seconds == 0

            if (mSeconds == 0)
            {
                if (mMinutes == 0)
                {
                    if (mHours == 0)
                    {
                        mMainTimer.IsEnabled = false;

                        if (checkBox_sound.IsChecked == true)
                        {
                            PlaySound();
                        }

                        if (checkBox_subTimer.IsChecked == true)
                        {
                            mIsSubtimerInProcess = true;
                            mSubtimer.IsEnabled = true;
                        }
                        else
                        {
                            btn_StartPause.Content = "Start";

                            btn_Stop.Visibility = Visibility.Hidden;

                            EnableButtons();

                            RestoreTimeValues();
                        }
                    }
                    else
                    {
                        mHours -= 1;

                        tb_Hours.Text = mHours.ToString("00");

                        mMinutes = 59;
                        tb_Minutes.Text = mMinutes.ToString("00");

                        mSeconds = 60;
                    }
                }
                else
                {
                    mMinutes -= 1;
                    tb_Minutes.Text = mMinutes.ToString("00");
                    mSeconds = 60;
                }
            }

            #endregion

            if (mMainTimer.IsEnabled == true)
            {
                mSeconds -= 1;
                tb_Seconds.Text = mSeconds.ToString("00");
            }
        }

        private void EnableButtons()
        {
            groupBox_SingleTimerPresets.IsEnabled = true;
            groupBox_SubtimerPresets.IsEnabled = true;

            tb_Hours.IsReadOnly = false;
            tb_Minutes.IsReadOnly = false;
            tb_Seconds.IsReadOnly = false;
            tb_SubMinutes.IsReadOnly = false;
            tb_SubSeconds.IsReadOnly = false;
        }

        private void DisableButtons()
        {
            groupBox_SingleTimerPresets.IsEnabled = false;
            groupBox_SubtimerPresets.IsEnabled = false;

            tb_Hours.IsReadOnly = true;
            tb_Minutes.IsReadOnly = true;
            tb_Seconds.IsReadOnly = true;
            tb_SubMinutes.IsReadOnly = true;
            tb_SubSeconds.IsReadOnly = true;
        }

        private void btn_StartPause_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();

            mHours = Convert.ToInt32(tb_Hours.Text);
            mMinutes = Convert.ToInt32(tb_Minutes.Text);
            mSeconds = Convert.ToInt32(tb_Seconds.Text);

            if (mShouldStoreTimeBeforeLaunch == true)
            {
                mBeforeLaunchHours = mHours;
                mBeforeLaunchMinutes = mMinutes;
                mBeforeLaunchSeconds = mSeconds;
                mBeforeLaunchSubMinutes = Convert.ToInt32(tb_SubMinutes.Text);
                mBeforeLaunchSubSeconds = Convert.ToInt32(tb_SubSeconds.Text);

                mShouldStoreTimeBeforeLaunch = false;
            }

            if (mMainTimer.IsEnabled == false)
            {
                btn_StartPause.Content = "Pause";

                if (mSubtimer.IsEnabled == false && mIsSubtimerInProcess == false)
                {
                    mMainTimer.IsEnabled = true;
                }
                else if (mSubtimer.IsEnabled == false && mIsSubtimerInProcess == true)
                {
                    mSubtimer.IsEnabled = true;
                }
                else
                {
                    mSubtimer.IsEnabled = false;
                    btn_StartPause.Content = "Continue";
                }

                btn_Stop.Visibility = Visibility.Visible;
            }
            else
            {
                mMainTimer.IsEnabled = false;
                btn_StartPause.Content = "Continue";
            }
        }

        private void btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            mShouldStoreTimeBeforeLaunch = true;

            mHours = 0;
            mMinutes = 0;
            mSeconds = 0;

            mMainTimer.IsEnabled = false;
            mSubtimer.IsEnabled = false;

            mIsSubtimerInProcess = false;
            btn_StartPause.Content = "Start";

            btn_Stop.Visibility = Visibility.Hidden;

            RestoreTimeValues();

            EnableButtons();
        }

        private void mSubtimer_Tick(object sender, EventArgs e)
        {
            mMinutes = Convert.ToInt32(tb_SubMinutes.Text);
            mSeconds = Convert.ToInt32(tb_SubSeconds.Text);

            if (mSeconds == 0)
            {
                if (mMinutes == 0)
                {
                    mSubtimer.IsEnabled = false;
                    mIsSubtimerInProcess = false;

                    if (checkBox_sound.IsChecked == true)
                    {
                        PlaySound();
                    }

                    btn_StartPause.Content = "Start";
                    btn_Stop.Visibility = Visibility.Hidden;
                    EnableButtons();
                    RestoreTimeValues();
                }
                else
                {
                    mMinutes -= 1;
                    tb_SubMinutes.Text = mMinutes.ToString("00");
                    mSeconds = 60;
                }
            }
            if (mSubtimer.IsEnabled == true)
            {
                mSeconds -= 1;
                tb_SubSeconds.Text = mSeconds.ToString("00");
            }
        }

        private void btn_Preset_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button).Tag.ToString();

            if (tag.Contains("."))
            {
                string[] time = tag.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                tb_Minutes.Text = string.Format("{0:D2}", int.Parse(time[0]));

                tb_Seconds.Text = string.Format("{0:D2}", int.Parse(time[1]));
                tb_SubMinutes.Text = "00";
                tb_SubSeconds.Text = "00";
            }
            else if (tag.Contains("+"))
            {
                string[] time = tag.Split("+".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                tb_Minutes.Text = string.Format("{0:D2}", int.Parse(time[0]));
                tb_Seconds.Text = "00";
                tb_SubMinutes.Text = string.Format("{0:D2}", int.Parse(time[1]));
                tb_SubSeconds.Text = "00";
            }
            else
            {
                tb_Minutes.Text = "00";
                tb_Seconds.Text = string.Format("{0:D2}", int.Parse(tag));
                tb_SubMinutes.Text = "00";
                tb_SubSeconds.Text = "00";
            }
        }

        private void RestoreTimeValues()
        {
            throw new NotImplementedException();
        }



        private void PlaySound()
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






    }
}
