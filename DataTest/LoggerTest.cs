using System.Numerics;
using TP.ConcurrentProgramming.Data;
using Newtonsoft.Json.Linq;

namespace TP.ConcurrentProgramming.DataTest
{
    [TestClass]
    public class LoggerTest
    {
        private class TestBall : IBall
        {
            public event EventHandler<Vector2> NewPositionNotification;
            public int BallId { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public bool IsMoving { get; set; }
            public void StartThread() { }
        }

        [TestMethod]
        public void Log_SingleEntry_WritesJsonLine()
        {
            var logger = new Logger();
            var ball = new TestBall { BallId = 1, Position = new Vector2(1, 2), Velocity = new Vector2(3, 4) };
            var logTime = DateTime.Now;

            logger.Log(ball, logTime);
            Thread.Sleep(200); // Żeby logger miał czas na zapisanie

            logger.Dispose();

            var logFile = Directory.GetFiles(Path.GetTempPath(), "TPW_*.json")[^1];
            var lines = File.ReadAllLines(logFile);
            Assert.IsTrue(lines.Length > 1, "Brak wpisów w pliku logu.");
            var json = JObject.Parse(lines[1]);
            Assert.AreEqual(1, (int)json["BallId"]);
            Assert.AreEqual(1, (float)json["Position"]["X"]);
            Assert.AreEqual(2, (float)json["Position"]["Y"]);
            Assert.AreEqual(3, (float)json["Velocity"]["X"]);
            Assert.AreEqual(4, (float)json["Velocity"]["Y"]);
        }

        [TestMethod]
        public void Log_MultipleEntries_AllAreLogged()
        {
            var logger = new Logger();
            for (int i = 0; i < 5; i++)
            {
                var ball = new TestBall { BallId = i, Position = new Vector2(i, i), Velocity = new Vector2(i, -i) };
                logger.Log(ball, DateTime.Now);
            }
            Thread.Sleep(300);

            logger.Dispose();

            var logFile = Directory.GetFiles(Path.GetTempPath(), "TPW_*.json")[^1];
            var lines = File.ReadAllLines(logFile);
            // Pierwsza linia to nagłówek, kolejne to logi
            Assert.IsTrue(lines.Length >= 6, "Nie wszystkie wpisy zostały zapisane.");
        }

        [TestMethod]
        public void Logger_Dispose_StopsLogging()
        {
            var logger = new Logger();
            var ball = new TestBall { BallId = 99, Position = new Vector2(9, 9), Velocity = new Vector2(0, 0) };
            logger.Dispose();

            logger.Log(ball, DateTime.Now);
            Thread.Sleep(100);

            var logFile = Directory.GetFiles(Path.GetTempPath(), "TPW_*.json")[^1];
            var content = File.ReadAllText(logFile);
            Assert.IsFalse(content.Contains("99"), "Logger nie powinien logować po Dispose.");
        }
    }
}