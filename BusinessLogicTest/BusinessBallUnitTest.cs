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

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            Ball newInstance = new(dataBallFixture);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); Assert.IsNotNull(position); numberOfCallBackCalled++; };
            dataBallFixture.Move();
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
        }

        #region testing instrumentation

        private class DataBallFixture : Data.IBall
        {
            public int BallId { get; }

            private Vector2 _position = new Vector2(100.0f, 150.0f);
            private Vector2 _velocity = new Vector2(5.0f, 3.0f);

            public Vector2 Position => _position;
            public Vector2 Velocity
            {
                get => _velocity;
                set => _velocity = value;
            }
            public bool IsMoving { get; set; } = true;

            public event EventHandler<Vector2>? NewPositionNotification;

            public void StartThread() { }

            internal void Move()
            {
                // Symulacja rzeczywistego ruchu kulki
                float velocityX = Velocity.X;
                float velocityY = Velocity.Y;

                // Obliczenie nowej pozycji
                float newX = Position.X + velocityX;
                float newY = Position.Y + velocityY;

                // Aktualizacja pozycji
                _position = new Vector2(newX, newY);

                // Wywołanie zdarzenia z nową pozycją
                NewPositionNotification?.Invoke(this, _position);
            }

            // Metoda do ustawiania pozycji z zewnątrz
            public void SetPosition(Vector2 position)
            {
                _position = position;
                NewPositionNotification?.Invoke(this, position);
            }
        }

        #endregion testing instrumentation
    }
}