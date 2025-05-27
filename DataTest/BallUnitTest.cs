//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;
using System.Threading;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            // Utworzenie nowej piłki z pozycją początkową
            Vector2 initialPosition = new Vector2(100f, 100f);
            Ball ball = new Ball(1, initialPosition);

            // Sprawdzenie, czy wartości początkowe są ustawione
            Assert.AreEqual(initialPosition, ball.Position);
            Assert.IsTrue(ball.Velocity.X != 0 || ball.Velocity.Y != 0);
            Assert.IsFalse(ball.IsMoving); // Przed wywołaniem StartThread, IsMoving powinno być false

            // Nasłuchiwanie zdarzeń zmiany pozycji
            int positionChangeCount = 0;
            ManualResetEvent positionChangedEvent = new ManualResetEvent(false);

            ball.NewPositionNotification += (sender, position) =>
            {
                positionChangeCount++;
                Assert.IsNotNull(sender);
                Assert.IsNotNull(position);

                if (positionChangeCount >= 3)
                    positionChangedEvent.Set();
            };

            // Uruchom wątek piłki
            ball.StartThread();

            // Poczekaj na zmianę pozycji
            bool signaled = positionChangedEvent.WaitOne(TimeSpan.FromSeconds(2));

            // Sprawdź, czy pozycja się zmieniła
            Assert.IsTrue(signaled, "Piłka nie powinna się poruszać");
            Assert.IsTrue(positionChangeCount >= 3, "Pozycja piłki powinna się zmienić co najmniej 3 razy");
            Assert.IsTrue(ball.IsMoving);

            // Zatrzymaj ruch piłki
            ball.IsMoving = false;

            // Poczekaj na zatrzymanie
            Thread.Sleep(100);

            // Sprawdź, czy piłka się zatrzymała
            Assert.IsFalse(ball.IsMoving);
        }
    }
}