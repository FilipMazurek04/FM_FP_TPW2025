//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessLogicImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (BusinessLogicImplementation newInstance = new(new DataLayerConstructorFixcure()))
            {
                bool newInstanceDisposed = true;
                newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
                Assert.IsFalse(newInstanceDisposed);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataLayerDisposeFixcure dataLayerFixcure = new DataLayerDisposeFixcure();
            BusinessLogicImplementation newInstance = new(dataLayerFixcure);
            Assert.IsFalse(dataLayerFixcure.Disposed);
            bool newInstanceDisposed = true;
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
            Assert.IsTrue(dataLayerFixcure.Disposed);
        }

        [TestMethod]
        public void StartTestMethod()
        {
            DataLayerStartFixcure dataLayerFixcure = new();
            using (BusinessLogicImplementation newInstance = new(dataLayerFixcure))
            {
                int called = 0;
                int numberOfBalls2Create = 10;
                newInstance.Start(
                  numberOfBalls2Create,
                  (startingPosition, ball) => { called++; Assert.IsNotNull(startingPosition); Assert.IsNotNull(ball); });
                Assert.AreEqual<int>(1, called);
                Assert.IsTrue(dataLayerFixcure.StartCalled);
                Assert.AreEqual<int>(numberOfBalls2Create, dataLayerFixcure.NumberOfBallseCreated);
            }
        }

        #region testing instrumentation

        private class DataLayerConstructorFixcure : Data.DataAbstractAPI
        {
            public override void Dispose()
            { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }
        }

        private class DataLayerDisposeFixcure : Data.DataAbstractAPI
        {
            internal bool Disposed = false;

            public override void Dispose()
            {
                Disposed = true;
            }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }
        }

        private class DataLayerStartFixcure : Data.DataAbstractAPI
        {
            internal bool StartCalled = false;
            internal int NumberOfBallseCreated = -1;

            public override void Dispose()
            { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                StartCalled = true;
                NumberOfBallseCreated = numberOfBalls;
                upperLayerHandler(new DataVectorFixture(), new DataBallFixture());
            }

            private record DataVectorFixture : Data.IVector
            {
                public double x { get; init; }
                public double y { get; init; }
            }

            private record VectorFixture(double x, double y) : Data.IVector;

            private class DataBallFixture : Data.IBall
            {
                private Data.IVector _position = new DataVectorFixture(); 
                public Data.IVector Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

                public event EventHandler<Data.IVector>? NewPositionNotification;

                public void SetPosition(Data.IVector position)
                {
                    _position = position;
                    NewPositionNotification?.Invoke(this, position);
                }

                internal void Move()
                {
                    NewPositionNotification?.Invoke(this, new DataVectorFixture()); // Użyj DataVectorFixture zamiast VectorFixture
                }
            }
        }




        // Test sprawdzający, czy kulka pozostaje w granicach stołu
        [TestMethod]
        public void BallPosition_ShouldRespectBoundaries_WhenOutsideOfTable()
        {
            // Arrange
            DataLayerStartFixcure dataLayerFixcure = new();
            using (BusinessLogicImplementation newInstance = new(dataLayerFixcure))
            {
                bool boundaryRespected = true;
                double tableWidth = 400; // Szerokość stołu
                double tableHeight = 420; // Wysokość stołu
                double ballDiameter = 20; // Średnica kulki

                // Pozycje do testowania - poza granicami stołu
                double[] testXPositions = new double[] { -10, tableWidth + 10 };
                double[] testYPositions = new double[] { -10, tableHeight + 10 };

                // Act
                foreach (var xPos in testXPositions)
                {
                    foreach (var yPos in testYPositions)
                    {
                        // Próba utworzenia kulki poza granicami stołu
                        newInstance.Start(1, (position, ball) =>
                        {
                            // Wywołaj zdarzenie ze współrzędnymi poza granicami
                            var vectorOutsideBounds = new TestVector { x = xPos, y = yPos };

                            // Symuluj ruch kulki poza granice
                            if (ball is BusinessLogic.IBall businessBall)
                            {
                                // Zarejestruj handler, który sprawdzi, czy kulka jest wewnątrz granic
                                businessBall.NewPositionNotification += (sender, pos) =>
                                {
                                    double radius = ballDiameter / 2.0;
                                    if (pos.x < radius || pos.x > tableWidth - radius ||
                                        pos.y < radius || pos.y > tableHeight - radius)
                                    {
                                        boundaryRespected = false;
                                    }
                                };

                                // Symuluj zdarzenie zmiany pozycji
                                // Musimy użyć interfejsu Data.IBall zamiast konkretnej klasy DataBallFixture
                                if (ball is Data.IBall dataBall)
                                {
                                    // Wywołaj NewPositionNotification przez SetPosition
                                    dataBall.SetPosition(vectorOutsideBounds);
                                }
                            }
                        });
                    }
                }

                // Assert
                Assert.IsTrue(boundaryRespected, "Kulka powinna pozostać w granicach stołu");
            }
        }

        // Klasa pomocnicza do testów 
        private class TestVector : Data.IVector
        {
            public double x { get; init; }
            public double y { get; init; }
        }



        #endregion testing instrumentation
    }
}