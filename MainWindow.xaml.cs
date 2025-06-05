using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Timers;
using Windows.Graphics;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using Microsoft.UI.Composition.Interactions;


namespace TodoMoro
{
    public sealed partial class MainWindow : Window
    {
        private bool isRunning = false;              // Flag to know if it is active
        private bool paused = false;                 // flag to know if it is paused
        private DispatcherTimer dispatcherTimer;     // Timer that updates every second

        /* WORKING TIMER */
        private TimeSpan timeLeftWorking;            // Current remaining timel
        private TimeSpan defaultDurationWorking;     // Default time (used on reboot
        private int minutesWorking = 25;             // Configurable minutes

        /* REST TIMER */
        private bool resting = false;
        private TimeSpan timeLeftRest;
        private TimeSpan defaultDurationRest;
        private int minutesRest = 5;                    // Configurable minutes


        public MainWindow()
        {
            this.InitializeComponent();

            // Set window title and size using AppWindow
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Title = "TodoMoro";
            appWindow.Resize(new SizeInt32(400, 750));

            defaultDurationWorking = TimeSpan.FromMinutes(25);  // Standard Pomodoro Duration
            timeLeftWorking = defaultDurationWorking;

            UpdateDisplay(); // Displays the initial time
        }

        // Button: Start
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning && !paused)
            { 
                UpdateDisplay();   // Shows the new time
                StartTimer();      // Timer starts
                isRunning = true;
            }
            else
            {
                StartTimer();
                isRunning = true;

            }

                this.paused = false;
        }

        // Button: Pause
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.paused = true;
            if (dispatcherTimer != null && dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Stop();
                isRunning = false;
            }
        }

        // Button: Restart
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
            }

            timeLeftWorking = defaultDurationWorking;
            isRunning = false;
            UpdateDisplay();
        }

        // Logic to start the timer
        private void StartTimer()
        {
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
                dispatcherTimer.Tick += Timer_Tick;
            }

            dispatcherTimer.Start();
        }

        // Every second updates the timer
        private void Timer_Tick(object sender, object e)
        {
            if (timeLeftWorking.TotalSeconds > 0)
            {
                timeLeftWorking = timeLeftWorking.Subtract(TimeSpan.FromSeconds(1));
                UpdateDisplay();
            }
            else
            {
                dispatcherTimer.Stop();
                TimerDisplay.Text = "¡Tiempo!";
                isRunning = false;
            }
        }

        private void Time_Changed(object sender, RoutedEventArgs e)
        {
            // Read custom duration from input field
            this.minutesWorking = (int)DurationBox.Value;
            this.timeLeftWorking = TimeSpan.FromMinutes(this.minutesWorking); // Use the chosen duration
            this.defaultDurationWorking = timeLeftWorking;
            
            UpdateDisplay(); //Updates the current time with the new one

        }


        // Update the timer text
        private void UpdateDisplay()
        { 
            TimerDisplay.Text = timeLeftWorking.ToString(@"mm\:ss");
        }
    }
}

