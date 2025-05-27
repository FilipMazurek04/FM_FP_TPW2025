using System.Collections.Concurrent;
using System.Numerics;
using Newtonsoft.Json;

namespace TP.ConcurrentProgramming.Data
{
    internal class Logger : IDisposable
    {
        private class BallToLog
        {
            public int BallId { get; }
            public Vector2 Position { get; }
            public Vector2 Velocity { get; }
            public DateTime Date { get; }

            public BallToLog(int ballID, Vector2 pos, Vector2 vel, DateTime date)
            {
                BallId = ballID;
                Position = pos;
                Velocity = vel;
                Date = date;
            }
        }

        private readonly string _logFilePath;
        private readonly ConcurrentQueue<BallToLog> _queue = new();
        private readonly ManualResetEvent _stopEvent = new(false);
        private readonly Thread _loggerThread;
        private bool _isDisposed = false;
        private static Logger? logger;
        private static readonly object _singletonLock = new();

        public Logger()
        {
            string pathToSave = Path.GetTempPath(); // tutaj \AppData\Local\Temp
            _logFilePath = Path.Combine(pathToSave, $"TPW_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fff}.json");
            File.WriteAllText(_logFilePath, $"Logger start: {DateTime.Now}\n");

            _loggerThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "LoggerThread"
            };
            _loggerThread.Start();
        }

        public static Logger CreateLogger()
        {
            lock (_singletonLock)
            {
                if (logger == null)
                    logger = new Logger();
                return logger;
            }
        }

        public void Log(IBall ball, DateTime date)
        {
            if (_isDisposed) return;
            _queue.Enqueue(new BallToLog(ball.BallId, ball.Position, ball.Velocity, date));
        }

        private void ProcessLogQueue()
        {
            while (!_stopEvent.WaitOne(100)) // co 100ms sprawdza czy są nowe wpisy w kolejce
            {
                while (_queue.TryDequeue(out BallToLog entry))
                {
                    try
                    {
                        string jsonString = JsonConvert.SerializeObject(entry);
                        File.AppendAllText(_logFilePath, jsonString + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(_logFilePath + ".error.json", $"{DateTime.Now}: Error writing log: {ex.Message}\n");
                    }
                }
            }
            // Zapisuje pozostałe wpisy przed zakończeniem
            while (_queue.TryDequeue(out BallToLog entry))
            {
                try
                {
                    string jsonString = JsonConvert.SerializeObject(entry);
                    File.AppendAllText(_logFilePath, jsonString + Environment.NewLine);
                }
                catch { }
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _stopEvent.Set();
            if (!_loggerThread.Join(1000))
            {
                try { _loggerThread.Interrupt(); }
                catch { }
            }
            _stopEvent.Dispose();
        }
    }
}