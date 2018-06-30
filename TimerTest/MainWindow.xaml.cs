using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TimerTest.Annotations;

namespace TimerTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const byte TestIntervalConst = 1;

        #region privat fields

        private static Timer _timer;
        private static TimeSpan _timeLeft;
        private static TimeSpan _newTimeLeft;
        private static TimeSpan _startTime;
        private static TimeSpan _currentTime;
        private static TimeSpan _nextTime;
        private static ushort _dueTime;
        private string _someText;
        private bool _isTimerRunning;
        private readonly Queue<TimeSpan> _queue = new Queue<TimeSpan>();
        private readonly SoundPlayer sp = new SoundPlayer();

        #endregion

        #region constructor

        public MainWindow()
        {
            InitializeComponent();
            _timer = new Timer(TimerCallbackHandle, null, Timeout.Infinite, Timeout.Infinite);
            SetTimes();
            sp.SoundLocation = @"..\..\Sounds\1.wav";
            sp.Play();
            Thread.Sleep (1000);
            sp.Play();
        }

        #endregion

        #region properties

        /// <summary>
        /// Начало отсчёта
        /// </summary>
        public TimeSpan StartTime
        {
            get => _startTime;
            set {
                _startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }

        /// <summary>
        /// Оставшееся время до следующей точки
        /// </summary>
        public TimeSpan TimeLeft
        {
            get => _timeLeft;
            set {
                _timeLeft = value;
                OnPropertyChanged(nameof(TimeLeft));
            }
        }

        /// <summary>
        /// Current time
        /// </summary>
        public TimeSpan CurrentTime
        {
            get  => _currentTime;
            set {
                _currentTime = value;
                OnPropertyChanged(nameof(CurrentTime));
            }
        }

        /// <summary>
        /// Next timer point
        /// </summary>
        public TimeSpan NextTime
        {
            get => _nextTime;
            set {
                _nextTime = value;
                OnPropertyChanged(nameof(NextTime));
            }
        }

        /// <summary>
        /// Due time for timer
        /// </summary>
        public ushort DueTime
        {
            get => _dueTime;
            set {
                _dueTime = value;
                OnPropertyChanged(nameof(DueTime));
            }
        }

        /// <summary>
        /// State of timer, shows whether the timer is on
        /// </summary>
        public bool IsTimerRunning
        {
            get => _isTimerRunning;
            set {
                _isTimerRunning = value;
                OnPropertyChanged (nameof(IsTimerRunning));
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Setup times. Uses in Constructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetTimes()
        {
            CurrentTime = DateTime.Now.TimeOfDay;
            StartTime = new TimeSpan(14, 30, 0);
            //StartTime = _currentTime + new TimeSpan (1 * TimeSpan.TicksPerMinute) - new TimeSpan(_currentTime.Milliseconds * TimeSpan.TicksPerMillisecond);
            //StartTime = _currentTime - new TimeSpan (1 * TimeSpan.TicksPerMinute) + new TimeSpan(_currentTime.Milliseconds * TimeSpan.TicksPerMillisecond);
            _queue.Enqueue (_startTime);

            NextTime = _startTime + new TimeSpan (TestIntervalConst * TimeSpan.TicksPerMinute);
            _queue.Enqueue (_nextTime);
            _queue.Enqueue (_nextTime + new TimeSpan (TestIntervalConst * TimeSpan.TicksPerMinute));

            if (_startTime <= _currentTime)
                TimeLeft = TimeSpan.FromHours (24) - _currentTime + _startTime;
            else
                TimeLeft = _startTime - _currentTime;
        }

        /// <summary>
        /// Button Click Handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M_startButton_OnClick (object sender, RoutedEventArgs e)
        {
            if (IsTimerRunning) {

                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                IsTimerRunning = false;
                TimeLeft = NextTime = TimeSpan.Zero;
                
                return;
            }

            InitializeTimer();
        }

        private void InitializeTimer()
        {
            if (_queue.Count == 0) {
                return;
            }

            NextTime = _queue.Dequeue();
            CurrentTime = DateTime.Now.TimeOfDay;

            if (_startTime <= _currentTime)
                TimeLeft = TimeSpan.FromHours (24) - _currentTime + _nextTime;
            else
                TimeLeft = _nextTime - _currentTime;

            DueTime = (ushort)(1000 - _currentTime.Milliseconds);

            _timer.Change (_dueTime, 0);

            IsTimerRunning = true;
        }


        private void TimerCallbackHandle(object state)
        {
            CurrentTime = DateTime.Now.TimeOfDay;

            if (_currentTime < _nextTime)
                _newTimeLeft = _nextTime - _currentTime;
            else
                _newTimeLeft = TimeSpan.FromHours (24) - _currentTime + _nextTime;

            if (_newTimeLeft > _timeLeft) {

                // If time is up:
                sp.Play();

                if (_queue.Count == 0) {

                    _timer.Dispose();
                    IsTimerRunning = false;
                    return;
                }

                // Load next point
                NextTime = _queue.Dequeue();

                if (_currentTime < _nextTime)
                    TimeLeft = _nextTime - _currentTime;
                else
                    TimeLeft = TimeSpan.FromHours (24) - _currentTime + _nextTime;
            }
            else {

                TimeLeft = _newTimeLeft;
            }

            DueTime = (ushort)(1000 - _currentTime.Milliseconds);

            _timer.Change (_dueTime, 0);
        }

        #endregion methods

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged
    }
}
