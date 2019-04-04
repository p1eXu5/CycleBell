using System.Threading;
using System.Windows.Threading;
using CycleBellLibrary.Context;
using CycleBellLibrary.Models;

namespace CycleBell.ViewModels
{
    public static class AlarmExtensions
    {
        public static void PlayDispatcher ( this IAlarm alarm, TimePoint nextTimePoint = null )
        {
            (( DispatcherObject )alarm.Player).Dispatcher.BeginInvoke( DispatcherPriority.Normal, (ThreadStart)delegate ()
                                                                                                               {
                                                                                                                   alarm.Play();
                                                                                                                   alarm.LoadSound( nextTimePoint );
                                                                                                               }
                                                                     );
        }

        public static void PlayDefaultDispatcher ( this IAlarm alarm )
        {
            (( DispatcherObject )alarm.DefaultPlayer).Dispatcher.BeginInvoke( DispatcherPriority.Normal, (ThreadStart)alarm.PlayDefault );
        }

        public static void StopDispatcher ( this IAlarm alarm )
        {
            (( DispatcherObject )alarm.Player).Dispatcher.BeginInvoke( DispatcherPriority.Normal, (ThreadStart)alarm.Stop );
        }

        public static void StopDefaultDispatcher ( this IAlarm alarm )
        {
            (( DispatcherObject )alarm.DefaultPlayer ).Dispatcher.BeginInvoke( DispatcherPriority.Normal, (ThreadStart)alarm.StopDefault );
        }

        public static void LoadSoundDispatcher ( this IAlarm alarm, TimePoint nextTimePoint )
        {
            (( DispatcherObject )alarm.Player).Dispatcher.BeginInvoke( DispatcherPriority.Normal, (ThreadStart)delegate ()
                                                                                                               {
                                                                                                                   alarm.LoadSound( nextTimePoint );
                                                                                                               }
                                                                     );
        }
    }
}
