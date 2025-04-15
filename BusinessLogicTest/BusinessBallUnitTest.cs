//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

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
            // Implementuj właściwość Velocity, aby nie rzucała wyjątku
            private Data.IVector _velocity = new VectorFixture(5.0, 3.0); // Domyślna prędkość

            public Data.IVector Velocity
            {
                get => _velocity;
                set => _velocity = value;
            }

            public event EventHandler<Data.IVector>? NewPositionNotification;

            // Dodanie brakującej implementacji metody SetPosition
            public void SetPosition(Data.IVector position)
            {
                // Przy wywołaniu tej metody generujemy zdarzenie NewPositionNotification
                NewPositionNotification?.Invoke(this, position);
            }

            internal void Move()
            {
                // Symulacja rzeczywistego ruchu kulki
                double currentX = 150.0; // Przykładowa początkowa pozycja X
                double currentY = 200.0; // Przykładowa początkowa pozycja Y

                // Wykorzystaj właściwość Velocity zamiast sztywno zakodowanych wartości
                double velocityX = Velocity.x;
                double velocityY = Velocity.y;

                // Obliczenie nowej pozycji
                double newX = currentX + velocityX;
                double newY = currentY + velocityY;

                // Wywołanie zdarzenia z nową pozycją
                NewPositionNotification?.Invoke(this, new VectorFixture(newX, newY));
            }
        }


        private class VectorFixture : Data.IVector
    {
      internal VectorFixture(double X, double Y)
      {
        x = X; y = Y;
      }

      public double x { get; init; }
      public double y { get; init; }
    }

    #endregion testing instrumentation
  }
}