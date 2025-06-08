using Microsoft.UI.Composition.Interactions;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using System.Timers;
using Windows.Graphics;
using Windows.Graphics;


namespace TodoMoro
{
    public sealed partial class MainWindow : Window
    {
        private bool isRunning = false;              // Flag to know if it is active
        private bool paused = false;                 // flag to know if it is paused
        private DispatcherTimer dispatcherTimer;     // Timer that updates every second
        private enum SessionType { Work, Rest }
        private SessionType currentSession = SessionType.Work;
        private TimeSpan timeLeft;            // Current remaining timel
        private TimeSpan defaultDuration = TimeSpan.FromMinutes(25);     // Default time (used on reboot

        /* WORKING TIMER */

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

            defaultDuration = TimeSpan.FromMinutes(25);  // Standard Pomodoro Duration
            timeLeft = defaultDuration;

            UpdateDisplay(); // Displays the initial time
        }

        // Button: Start
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                // Si es una nueva sesión, toma el valor del NumberBox
                if (currentSession == SessionType.Work)
                {
                    int minutes = (int)DurationBox.Value;
                    timeLeft = TimeSpan.FromMinutes(minutes);
                }
                else
                {
                    int restMinutes = (int)RestBox.Value;
                    timeLeft = TimeSpan.FromMinutes(restMinutes);
                }

                defaultDuration = timeLeft;
                UpdateDisplay();
                StartTimer();
                isRunning = true;
            }
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

            timeLeft = defaultDuration;
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
            if (timeLeft.TotalSeconds > 0)
            {
                timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));
                UpdateDisplay();
            }
            else
            {
                dispatcherTimer.Stop();
                isRunning = false;

                // Cambiar de sesión
                if (currentSession == SessionType.Work)
                {
                    currentSession = SessionType.Rest;
                    int restMinutes = (int)RestBox.Value;
                    timeLeft = TimeSpan.FromMinutes(restMinutes);
                    TimerDisplay.Text = "Descanso...";
                }
                else
                {
                    currentSession = SessionType.Work;
                    int workMinutes = (int)DurationBox.Value;
                    timeLeft = TimeSpan.FromMinutes(workMinutes);
                    TimerDisplay.Text = "¡A trabajar!";
                }

                // Esperar 1 segundo y luego reiniciar automáticamente
                Task.Delay(1000).ContinueWith(_ =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        UpdateDisplay();
                        StartTimer();
                        isRunning = true;
                    });
                });
            }
        }

        private void Time_Changed(object sender, RoutedEventArgs e)
        {
            // Read custom duration from input field
            this.minutesWorking = (int)DurationBox.Value;
            this.timeLeft = TimeSpan.FromMinutes(this.minutesWorking); // Use the chosen duration
            this.defaultDuration = timeLeft;

            UpdateDisplay(); //Updates the current time with the new one

        }

        private void DurationBox_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs e)
        {
            // Obtiene el nuevo valor introducido por el usuario
            int newMinutes = (int)e.NewValue;

            // Actualiza la duración por defecto
            this.defaultDuration = TimeSpan.FromMinutes(newMinutes);

            // Si el temporizador no está activo y estás en modo trabajo, actualiza también timeLeft
            if (!isRunning && currentSession == SessionType.Work)
            {
                timeLeft = this.defaultDuration;
                UpdateDisplay();
            }
        }


        // Update the timer text
        private void UpdateDisplay()
        {
            TimerDisplay.Text = timeLeft.ToString(@"mm\:ss");
        }
    }
}

