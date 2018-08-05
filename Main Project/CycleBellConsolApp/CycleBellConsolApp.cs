// -------------------------------------------------------
// UI/Main
// -------------------------------------------------------

using System;
using System.Media;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using CycleBellLibrary;

namespace CycleBellConsolApp
{
    class CycleBellConsolApp
    {
        static void Main(string[] args)
        {
            try {

                var sound = new SoundPlayer("../../Sounds/sfx003.wav");
                //sound.Play();

                Preset.DefaultInterval = 1;

                // Create manager
                var manager = CycleBellTimerManager.Instance(new Director());

                AppDomain.CurrentDomain.ProcessExit += manager.OnAppExit;

                // Create TimePoints
                TimePoint[] points =
                {
                    new TimePoint {Time = TimeSpan.FromSeconds(10), TimerCycleNum = 1},
                    new TimePoint {Time = TimeSpan.FromSeconds(15), TimerCycleNum = 2},
                    new TimePoint {Time = TimeSpan.FromSeconds(20), TimerCycleNum = 3},
                    new TimePoint {Time = TimeSpan.FromSeconds(30), TimerCycleNum = 1},
                    new TimePoint {Time = TimeSpan.FromSeconds(35), TimerCycleNum = 2},
                    new TimePoint {Time = TimeSpan.FromSeconds(40), TimerCycleNum = 3},
                };

                // Set loops counts
                manager.Presets[0].AddTimePointRange(points);
                manager.Presets[0].TimersCycles[1] = 4;
                manager.Presets[0].TimersCycles[2] = 1;
                manager.Presets[0].TimersCycles[3] = 3;

                // Create DNS time points
                TimePoint[] dnsTimePoints = new[]
                {
                    new TimePoint("Начало", "8:00", TimePointType.Absolute) {Tag = ""},
                    new TimePoint("Начало", "8:05", TimePointType.Absolute, 1),
                    new TimePoint("Перекур", "8:55", TimePointType.Absolute, 1),
                    new TimePoint("Начало", "9:00", TimePointType.Absolute, 1) {Tag = "../../Sounds/Kiss.wav"},
                    new TimePoint("Начало", "9:55", TimePointType.Absolute, 1),
                    new TimePoint("Начало", "10:05", TimePointType.Absolute, 1),
                };

                manager.AddPreset(new Preset("DNS", dnsTimePoints, "8:00", true));


                TimePoint[] testTimePoints =
                {
                    new TimePoint("Start", "0:00:30"),
                    new TimePoint("0:00:33"),
                    new TimePoint("0:00:31"),
                };

                // Не гонись за идеалом. -Работает? -Работает!
                var startTime = DateTime.Now.TimeOfDay;
                startTime += TimeSpan.FromSeconds(11);

                manager.AddPreset(new Preset("Test", testTimePoints, startTime, true));
                int idx = manager.GetIndex("Test");

                #region Event Handlers

                // Load event handlers:

                manager.ChangeTimePointEvent += (sender, e) =>
                                                {
                                                    Console.WriteLine("\nOnChangeTimePoint in: " + DateTime.Now.TimeOfDay);

                                                    var x = Console.CursorLeft;
                                                    var y = Console.CursorTop;

                                                    Console.SetCursorPosition(Console.WindowWidth / 2 - 14, Console.WindowHeight / 2 - 12);
                                                    Console.WriteLine("OnChangeTimePoint:");
                                                    Console.SetCursorPosition(Console.WindowWidth / 2 - 10, Console.WindowHeight / 2 - 10);
                                                    Console.WriteLine($"Now is {DateTime.Now.TimeOfDay:h\\:mm\\:ss}");
                                                    Console.SetCursorPosition(Console.WindowWidth / 2 - 10, Console.WindowHeight / 2 - 8);
                                                    Console.WriteLine($"Next point at {e.NextTimePoint.GetAbsoluteTime()}");
                                                    Console.SetCursorPosition(Console.WindowWidth / 2 - 10, Console.WindowHeight / 2 - 6);
                                                    Console.WriteLine($"last: {e.LastTime:h\\:mm\\:ss}          ");
                                                    Console.SetCursorPosition(Console.WindowWidth / 2 - 10, Console.WindowHeight / 2 - 4);
                                                    Console.WriteLine($"Previous point at {e.NextTimePoint.GetAbsoluteTime()}");

                                                    Console.CursorTop = y;
                                                    Console.CursorLeft = x;


                                                    // Проверку вообще над делать по Tag'ам, но в UI
                                                    if (e.PrevTimePoint.Time < TimeSpan.Zero || e.PrevTimePoint.Time == manager.Presets[idx].StartTime) {
                                                        return;
                                                    }

                                                    Console.WriteLine("sound at: " + DateTime.Now.TimeOfDay);

                                                    sound.Stop();
                                                    sound.Play();
                                                };

                manager.TimerSecondPassedEvent += (s, e) =>
                                                  {
                                                      var x = Console.CursorLeft;
                                                      var y = Console.CursorTop;

                                                      Console.SetCursorPosition(Console.WindowWidth / 2 - 14, Console.WindowHeight / 2 - 1);
                                                      Console.WriteLine("OnTimerSecondPassed:");
                                                      Console.SetCursorPosition(Console.WindowWidth / 2 - 10, Console.WindowHeight / 2 + 1);
                                                      Console.WriteLine($"Now is {DateTime.Now.TimeOfDay:h\\:mm\\:ss}");
                                                      Console.SetCursorPosition(Console.WindowWidth / 2 - 10, Console.WindowHeight / 2 + 3);
                                                      Console.WriteLine($"To {e.NextTimePoint} last {e.LastTime:h\\:mm\\:ss}          ");

                                                      Console.CursorTop = y;
                                                      Console.CursorLeft = x;
                                                  };

                manager.TimerStopEvent += (s, e) => Console.WriteLine("Timer has stopped");

                #endregion

                foreach (var valueTuple in manager.GetTimerQueue(manager.Presets[idx])) {

                    Console.WriteLine($"{valueTuple.Item2} \n\t\t{valueTuple.Item1}");
                }
                Console.WriteLine();

                // Run cycle
                manager.Play(manager.Presets[idx]);

                string str = null;

                Console.WriteLine("\n's' - stop,\n'p' - play,\n' ' - pause/resume\n");

                while (str != "q") {

                    Console.Write("> ");
                    str = Console.ReadLine();

                    if (str == "s") {
                        manager.Stop();
                        Console.WriteLine($"\nstart point{manager.Presets[idx].StartTime}");
                    }
                    else if (str == "p") {
                        manager.Play(manager.Presets[idx]);
                        Console.WriteLine($"\nstart point{manager.Presets[idx].StartTime}");
                    }
                    else if (str == " ")
                        if (manager.IsRunning)
                            manager.Pouse();
                        else 
                            manager.Resume();
                }
            }
            catch (Exception ex) {

                Console.WriteLine(ex.Message);
                Console.ReadKey(true);
            }

        }

    }
}
