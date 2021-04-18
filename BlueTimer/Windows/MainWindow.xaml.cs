using BlueTimer.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using QP = QP_Helpers.QP_Helpers;

namespace BlueTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// The event that is fired when any child property changes its value
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        /// <summary>
        /// Call this to fire a <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="name">The name of the property</param>
        public void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public enum TimerType { Main, Sub }

        #region VARIABLES AND ONLOAD

        #region Public Properties

        /// <summary>
        /// Is the license key valid
        /// </summary>
        public bool IsLicenseValid { get; set; } = false;

        #endregion

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
        private int mBeforeLaunchSubHours = 0, mBeforeLaunchSubMinutes = 0, mBeforeLaunchSubSeconds = 0;

        private System.Windows.Forms.NotifyIcon mNotifyIcon = new System.Windows.Forms.NotifyIcon();

        /// <summary>
        /// Actual hours, minutes and seconds
        /// </summary>
        private TimeSpan mTimeLeft;

        private double mDarkOpacity = 0.5;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            mMainTimer.Tick += new EventHandler(mMainTimer_Tick);
            mMainTimer.Interval = new TimeSpan(0, 0, 1);

            mSubtimer.Tick += new EventHandler(mSubtimer_Tick);
            mSubtimer.Interval = new TimeSpan(0, 0, 1);

            #region Language

            combo_language.SelectedIndex = (int)Settings.Default["defaultLanguageIndex"];

            radio_detectLanguage.IsChecked = (bool)Settings.Default["detectLanguageOnStart"];

            if (radio_detectLanguage.IsChecked == true)
                SetLanguageDictionary(Thread.CurrentThread.CurrentCulture.ToString());
            else
            {
                radio_detectLanguage.IsChecked = false;
                radio_defaultLanguage.IsChecked = true;

                if (combo_language.SelectedIndex == 0)
                    SetLanguageDictionary("en-US");
                else
                    SetLanguageDictionary("de-DE");
            }

            // Set data context to this window
            DataContext = this;

            #endregion
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

        private void myWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string fullRegistryPath = @"HKEY_CURRENT_USER\Software\Quasimodo Programs\BT";
            string registryValueName = "Key";

            string key = QP.GetRegister(fullRegistryPath, registryValueName);

            if (key != null && QP.IsLicensed(key, "BlueTimer.LicenseKeys.txt"))
            {
                QP._isLicensed = true;
                UnlockFull();
            }
            else
            {
                QP._isLicensed = false;

                this.Opacity = mDarkOpacity;
                Window_Buy win = new Window_Buy()
                {
                    Owner = this,
                    ShowInTaskbar = false
                };
                win.ShowDialog();

                this.Opacity = 1;
                ShowInTaskbar = true;
            }
        }

        public void UnlockFull()
        {
            // Announce the license key to be valid
            IsLicenseValid = true;

            // Fire PropertyChanged event
            OnPropertyChanged(nameof(IsLicenseValid));
        }

        #endregion

        /// <summary>
        /// Fill controls with actual time values.
        /// </summary>
        /// <param name="type">The type of a timer controls of which are filled.</param>
        private void FillPrimitives(TimerType type)
        {
            if (type == TimerType.Main)
            {
                mHours = Convert.ToInt32(numeric_Hours.Value);
                mMinutes = Convert.ToInt32(numeric_Minutes.Value);
                mSeconds = Convert.ToInt32(numeric_Seconds.Value);
            }
            else
            {
                mHours = Convert.ToInt32(numeric_SubHours.Value);
                mMinutes = Convert.ToInt32(numeric_SubMinutes.Value);
                mSeconds = Convert.ToInt32(numeric_SubSeconds.Value);
            }
        }

        #region TIMER BUTTONS

        private void btn_StartPause_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();

            FillPrimitives(TimerType.Main);

            mTimeLeft = new TimeSpan(mHours, mMinutes, mSeconds);

            if (mShouldStoreTimeBeforeLaunch == true)
            {
                mBeforeLaunchHours = mHours;
                mBeforeLaunchMinutes = mMinutes;
                mBeforeLaunchSeconds = mSeconds;
                mBeforeLaunchSubHours = Convert.ToInt32(numeric_SubHours.Text);
                mBeforeLaunchSubMinutes = Convert.ToInt32(numeric_SubMinutes.Text);
                mBeforeLaunchSubSeconds = Convert.ToInt32(numeric_SubSeconds.Text);

                mShouldStoreTimeBeforeLaunch = false;
            }

            // If the main timer is not working...
            if (mMainTimer.IsEnabled == false)
            {
                btn_StartPause.Content = "Pause";

                // If the subtimer is not working and not in process...
                if (mSubtimer.IsEnabled == false && mIsSubtimerInProcess == false)
                    // turn the main timer on.
                    mMainTimer.IsEnabled = true;
                // If the subtimer is not working but in process...
                else if (mSubtimer.IsEnabled == false && mIsSubtimerInProcess == true)
                {
                    // turn the subtimer on.
                    mSubtimer.IsEnabled = true;

                    FillPrimitives(TimerType.Sub);

                    mTimeLeft = new TimeSpan(mHours, mMinutes, mSeconds);
                }
                // If the subtimer is working...
                else
                {
                    // pause the subtimer.
                    mSubtimer.IsEnabled = false;
                    btn_StartPause.Content = "⯈";
                }

                btn_Stop.Visibility = Visibility.Visible;
            }
            // If the main timer is working...
            else
            {
                // pause the main timer.
                mMainTimer.IsEnabled = false;
                btn_StartPause.Content = "⯈";
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

        #endregion

        #region TICKS

        private void mMainTimer_Tick(object sender, EventArgs e)
        {
            if (mTimeLeft.Seconds <= 0 && mTimeLeft.Minutes == 0 && mTimeLeft.TotalHours == 0)
            {
                mMainTimer.IsEnabled = false;

                if (checkBox_sound.IsEnabled == true && checkBox_sound.IsChecked == true)
                    PlaySound(TimerType.Main);

                if (checkBox_subTimer.IsChecked == false)
                {
                    btn_StartPause.Content = "Start";
                    btn_Stop.Visibility = Visibility.Hidden;
                    EnableButtons();
                    RestoreTimeValues();

                    if (myWindow.WindowState == WindowState.Minimized)
                        ShowWindow();
                    this.Activate();
                }
                else
                {
                    mIsSubtimerInProcess = true;

                    FillPrimitives(TimerType.Sub);

                    mTimeLeft = new TimeSpan(mHours, mMinutes, mSeconds);

                    mSubtimer.IsEnabled = true;
                }
            }
            else
            {
                mTimeLeft = mTimeLeft.Subtract(new TimeSpan(0, 0, 1));

                numeric_Hours.Value = (int)mTimeLeft.TotalHours;
                numeric_Minutes.Value = (int)mTimeLeft.Minutes;
                numeric_Seconds.Value = (int)mTimeLeft.Seconds;
            }
        }

        private void mSubtimer_Tick(object sender, EventArgs e)
        {
            if (mTimeLeft.Seconds <= 0 && mTimeLeft.Minutes == 0 && mTimeLeft.TotalHours == 0)
            {
                mSubtimer.IsEnabled = false;

                mIsSubtimerInProcess = false;

                if (checkBox_sound.IsEnabled == true && checkBox_sound.IsChecked == true)
                    PlaySound(TimerType.Sub);


                btn_StartPause.Content = "Start";
                btn_Stop.Visibility = Visibility.Hidden;
                EnableButtons();
                RestoreTimeValues();

                if (myWindow.WindowState == WindowState.Minimized)
                    ShowWindow();
                this.Activate();
            }
            else
            {
                mTimeLeft = mTimeLeft.Subtract(new TimeSpan(0, 0, 1));

                numeric_SubHours.Value = (int)mTimeLeft.Hours;
                numeric_SubMinutes.Value = (int)mTimeLeft.Minutes;
                numeric_SubSeconds.Value = (int)mTimeLeft.Seconds;
            }
        }

        #endregion

        #region ENABLING AND DISABLING BUTTONS

        private void EnableButtons()
        {
            groupBox_SingleTimerPresets.IsEnabled = groupBox_SubtimerPresets.IsEnabled = true;

            numeric_Hours.IsReadOnly = numeric_Minutes.IsReadOnly = numeric_Seconds.IsReadOnly = numeric_SubHours.IsReadOnly = numeric_SubMinutes.IsReadOnly = numeric_SubSeconds.IsReadOnly = false;
        }

        private void DisableButtons()
        {
            groupBox_SingleTimerPresets.IsEnabled = groupBox_SubtimerPresets.IsEnabled = false;

            numeric_Hours.IsReadOnly = numeric_Minutes.IsReadOnly = numeric_Seconds.IsReadOnly = numeric_SubHours.IsReadOnly = numeric_SubMinutes.IsReadOnly = numeric_SubSeconds.IsReadOnly = true;
        }

        #endregion

        private void btn_Preset_Click(object sender, RoutedEventArgs e)
        {
            // First 2 characters.
            string num;

            Button btn = sender as Button;

            string tag = btn.Tag.ToString();

            object content = btn.Content;

            // Potential TextBlock set as button's Content
            TextBlock tblc = content as TextBlock;

            // If button's content is not a TextBlock (for all-0 preset only)...
            if (tblc == null)
                // get first 2 characters from Content.
                num = new string(content.ToString().Take(2).ToArray());
            // If button's content is a TextBlock (for all other presets)...
            else
            {
                // Get a collection of Runs inside a TextBlock.
                InlineCollection incol = tblc.Inlines;

                // Get the first Run.
                Inline inline = incol.FirstInline;

                // Get text of the first Run.
                string text = new TextRange(inline.ContentStart, inline.ContentEnd).Text;

                // Get first 2 characters from text.
                num = new string(text.Take(2).ToArray());
            }

            // Parse first 2 characters as digits.
            int val = int.Parse(num);

            // Parse Tag as format.

            if (tag == "all")
                numeric_Hours.Value = numeric_Minutes.Value = numeric_Seconds.Value = 0;

            else if (tag == "seconds")
                numeric_Seconds.Value = val;

            else if (tag == "minutes")
                numeric_Minutes.Value = val;

            else if (tag == "hours")
                numeric_Hours.Value = val;

            else if (tag.Contains("+"))
            {
                string[] time = tag.Split("+".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                numeric_Minutes.Value = int.Parse(time[0]);
                numeric_Seconds.Text = "00";
                numeric_SubMinutes.Text = string.Format("{0:D2}", int.Parse(time[1]));
                numeric_SubSeconds.Text = "00";
            }
        }

        private void ShowWindow()
        {
            mNotifyIcon.Visible = false;

            ShowInTaskbar = true;

            Show();

            WindowState = WindowState.Normal;
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void myWindow_StateChanged(object sender, EventArgs e)
        {
            if (myWindow.WindowState == WindowState.Minimized)
            {
                mNotifyIcon = new System.Windows.Forms.NotifyIcon();

                mNotifyIcon.Text = "BlueTimer";

                mNotifyIcon.Click += notifyIcon1_Click;

                Stream _imageStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/img/notify-icon_16.ico")).Stream;

                mNotifyIcon.Icon = new Icon(_imageStream);

                mNotifyIcon.Visible = true;

                this.Hide();
            }
        }

        private void RestoreTimeValues()
        {
            numeric_Hours.Value = mBeforeLaunchHours;
            numeric_Minutes.Value = mBeforeLaunchMinutes;
            numeric_Seconds.Value = mBeforeLaunchSeconds;

            numeric_SubHours.Value = mBeforeLaunchSubHours;
            numeric_SubMinutes.Value = mBeforeLaunchSubMinutes;
            numeric_SubSeconds.Value = mBeforeLaunchSubSeconds;
        }

        #region VALIDATION

        private void InputNumberValidation(object sender, TextCompositionEventArgs e)
        {
            // Regex that matches disallowed text
            Regex regex = new Regex("[^0-9]+");

            e.Handled = regex.IsMatch(e.Text);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;

            base.OnPreviewKeyDown(e);
        }

        #endregion

        #region SOUND NOTIFICATION

        private void PlaySound(TimerType timerType)
        {
            if (timerType == TimerType.Main)
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
            else
            {
                if (radio1sub.IsChecked == true)
                    PlayConsoleBeeps();
                else if (radio2sub.IsChecked == true)
                    PlayBeeps();
                else if (radio3sub.IsChecked == true)
                    PlayExclamations();
                else if (radio4sub.IsChecked == true)
                    PlayAsterisks();
                else if (radio5sub.IsChecked == true)
                    PlayHands(1);
            }
        }

        #region Sounds

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

        #endregion

        #endregion

        #region LANGUAGE

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

        #endregion

        private void btn_Custom_Click(object sender, RoutedEventArgs e)
        {
            Opacity = mDarkOpacity;
            Window_EditCustomPreset win = new Window_EditCustomPreset(TimerType.Main)
            {
                Owner = this,
                ShowInTaskbar = false
            };

            win.ShowDialog();

            Opacity = 1;
            ShowInTaskbar = true;
        }

        private void btn_CustomSubtimer_Click(object sender, RoutedEventArgs e)
        {
            Opacity = mDarkOpacity;
            Window_EditCustomPreset win = new Window_EditCustomPreset(TimerType.Sub)
            {
                Owner = this,
                ShowInTaskbar = false
            };

            win.ShowDialog();

            Opacity = 1;
            ShowInTaskbar = true;
        }

        private void btn_About_Click(object sender, RoutedEventArgs e)
        {
            Opacity = mDarkOpacity;

            Window_About win = new Window_About()
            {
                Owner = this,
                ShowInTaskbar = false
            };
            win.ShowDialog();

            Opacity = 1;
            ShowInTaskbar = true;
        }

        private void btn_Register_Click(object sender, RoutedEventArgs e)
        {
            Opacity = mDarkOpacity;
            Window_Buy win = new Window_Buy()
            {
                Owner = this,
                ShowInTaskbar = false
            };
            win.ShowDialog();

            Opacity = 1;
            ShowInTaskbar = true;
        }

        private void myWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default["detectLanguageOnStart"] = radio_detectLanguage.IsChecked;
            Settings.Default["defaultLanguageIndex"] = combo_language.SelectedIndex;

            Settings.Default.Save();
            Application.Current.Shutdown();
        }

    }
}
