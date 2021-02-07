using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private double darkOpacity = 0.5;

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

        private void SetLanguageDictionary(string culture)
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (culture)
            {
                case "en-US":
                    dict.Source = new Uri("..\\Resources\\StringResources.xaml",
                        UriKind.Relative);
                    break;

                case "de-DE":
                    dict.Source = new Uri("..\\Resources\\StringResources.de-DE.xaml",
                        UriKind.Relative);
                    break;

                default:
                    dict.Source = new Uri("..\\Resources\\StringResources.xaml",
                        UriKind.Relative);
                    break;
            }

            App.Current.Resources.MergedDictionaries.Add(dict);
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
                        numeric_Minutes.Value = mMinutes;

                        mSeconds = 60;
                    }
                }
                else
                {
                    mMinutes -= 1;
                    numeric_Minutes.Value = mMinutes;
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
            numeric_Minutes.IsReadOnly = false;
            tb_Seconds.IsReadOnly = false;
            tb_SubMinutes.IsReadOnly = false;
            tb_SubSeconds.IsReadOnly = false;
        }

        private void DisableButtons()
        {
            groupBox_SingleTimerPresets.IsEnabled = false;
            groupBox_SubtimerPresets.IsEnabled = false;

            tb_Hours.IsReadOnly = true;
            numeric_Minutes.IsReadOnly = true;
            tb_Seconds.IsReadOnly = true;
            tb_SubMinutes.IsReadOnly = true;
            tb_SubSeconds.IsReadOnly = true;
        }

        private void btn_StartPause_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();

            mHours = Convert.ToInt32(tb_Hours.Text);
            mMinutes = Convert.ToInt32(numeric_Minutes.Value);
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

                numeric_Minutes.Value = int.Parse(time[0]);

                tb_Seconds.Text = string.Format("{0:D2}", int.Parse(time[1]));
                tb_SubMinutes.Text = "00";
                tb_SubSeconds.Text = "00";
            }
            else if (tag.Contains("+"))
            {
                string[] time = tag.Split("+".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                numeric_Minutes.Value = int.Parse(time[0]);
                tb_Seconds.Text = "00";
                tb_SubMinutes.Text = string.Format("{0:D2}", int.Parse(time[1]));
                tb_SubSeconds.Text = "00";
            }
            else
            {
                numeric_Minutes.Value = 0;
                tb_Seconds.Text = string.Format("{0:D2}", int.Parse(tag));
                tb_SubMinutes.Text = "00";
                tb_SubSeconds.Text = "00";
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            mNotifyIcon.Visible = false;

            ShowInTaskbar = true;

            Show();

            WindowState = WindowState.Normal;
        }

        private void myWindow_StateChanged(object sender, EventArgs e)
        {
            if (myWindow.WindowState == WindowState.Minimized)
            {
                mNotifyIcon = new System.Windows.Forms.NotifyIcon();

                mNotifyIcon.Text = "Blue Timer";

                mNotifyIcon.Click += notifyIcon1_Click;

                Stream _imageStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Timer.ico")).Stream;

                mNotifyIcon.Icon = new Icon(_imageStream);

                mNotifyIcon.Visible = true;

                this.Hide();
            }
        }

        private void RestoreTimeValues()
        {
            tb_Hours.Text = mBeforeLaunchHours.ToString("00");
            numeric_Minutes.Value = mBeforeLaunchMinutes;
            tb_Seconds.Text = mBeforeLaunchSeconds.ToString("00");
            tb_SubMinutes.Text = mBeforeLaunchSubMinutes.ToString("00");
            tb_SubSeconds.Text = mBeforeLaunchSubSeconds.ToString("00");
        }

        private void InputNumberValidation(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Regex that matches disallowed text
            Regex regex = new Regex("[^0-9]+");

            e.Handled = regex.IsMatch(e.Text);
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text == "")
            {
                (sender as TextBox).Text = "00";
            }
            else
            {
                (sender as TextBox).Text = Convert.ToInt32((sender as TextBox).Text).ToString("00");

                if (Convert.ToInt32((sender as TextBox).Text) > 59)
                    (sender as TextBox).Text = "59";
            }
        }

        private void PlaySound()
        {
            if (radio1.IsChecked == true)
                PlayConsoleBeeps();
            else if (radio2.IsChecked == true)
                PlayBeeps();
            else if (radio3.IsChecked == true)
                PlayExclamations();
            else if (radio4.IsChecked == true)
                PlayAsterisks();
            else if (radio5.IsChecked == true)
                PlayHands(1);
        }

        private void ButtonDetectLanguage_Click(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionary(Thread.CurrentThread.CurrentCulture.ToString());
        }

        private void ButtonEnglish_Click(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionary("en-US");
        }

        private void ButtonGerman_Click(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionary("de-DE");
        }

        private void btn_About_Click(object sender, RoutedEventArgs e)
        {
            Opacity = darkOpacity;

            Window_About win = new Window_About()
            {
                Owner = this,
                ShowInTaskbar = false
            };
            win.ShowDialog();

            Opacity = 1;
            ShowInTaskbar = true;
        }

        private void PlayConsoleBeeps()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 3; j > 0; j--)
                {
                    Console.Beep(4500, 100);
                }

                Thread.Sleep(125);

                Console.Beep(4600, 200);

                Thread.Sleep(500);
            }
        }

        private void PlayBeeps()
        {
            for (int j = 4; j > 0; j--)
            {
                SystemSounds.Beep.Play();

                Thread.Sleep(400);
            }
        }

        private void PlayExclamations()
        {
            for (int j = 3; j > 0; j--)
            {
                for (int i = 3; i > 0; i--)
                {
                    SystemSounds.Exclamation.Play();
                    Thread.Sleep(300);
                }

                Thread.Sleep(125);
                SystemSounds.Exclamation.Play();
                Thread.Sleep(500);
            }
        }

        private void PlayAsterisks()
        {
            for (int j = 2; j > 0; j--)
            {
                SystemSounds.Asterisk.Play();
                Thread.Sleep(800);
            }
        }

        private void PlayHands(int times)
        {
            for (int j = times; j > 0; j--)
            {
                SystemSounds.Hand.Play();
                Thread.Sleep(2500);
            }
        }

    }
}
