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
using System.Collections.Generic;
using System.Numerics;
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
                            var vectorOutsideBounds = new Vector2((float)xPos, (float)yPos);

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
                                if (ball is Data.IBall dataBall)
                                {
                                    // Symulowanie zmiany pozycji poprzez bezpośredni wywołanie zdarzenia
                                    ((TestBall)dataBall).SimulatePositionChange(vectorOutsideBounds);
                                }
                            }
                        });
                    }
                }

                // Assert
                Assert.IsTrue(boundaryRespected, "Kulka powinna pozostać w granicach stołu");
            }
        }

        // Test sprawdzający odbicie piłki od ściany
        [TestMethod]
        public void WallCollision_ShouldChangeVelocity_WhenBallHitsWall()
        {
            // Arrange
            var dataLayer = new DataLayerCollisionFixture();
            using (BusinessLogicImplementation businessLogic = new(dataLayer))
            {
                // Przypadek 1: Zderzenie z lewą ścianą
                var ball1 = new TestBall
                {
                    Position = new Vector2(5, 50),
                    Velocity = new Vector2(-2, 1)
                };
                Vector2 initialVelocity1 = ball1.Velocity;

                // Act
                businessLogic.WallCollision(ball1);

                // Assert
                Assert.AreEqual(-initialVelocity1.X, ball1.Velocity.X, "Prędkość X powinna być odwrócona po kolizji z lewą ścianą");
                Assert.AreEqual(initialVelocity1.Y, ball1.Velocity.Y, "Prędkość Y nie powinna się zmieniać przy kolizji z pionową ścianą");

                // Przypadek 2: Zderzenie z górną ścianą
                var ball2 = new TestBall
                {
                    Position = new Vector2(50, 5),
                    
                    Velocity = new Vector2(1, -2)
                };
                Vector2 initialVelocity2 = ball2.Velocity;

                // Act
                businessLogic.WallCollision(ball2);

                // Assert
                Assert.AreEqual(initialVelocity2.X, ball2.Velocity.X, "Prędkość X nie powinna się zmieniać przy kolizji z poziomą ścianą");
                Assert.AreEqual(-initialVelocity2.Y, ball2.Velocity.Y, "Prędkość Y powinna być odwrócona po kolizji z górną ścianą");
            }
        }

        // Test sprawdzający kolizję między dwiema piłkami
        [TestMethod]
        public void BallCollision_ShouldChangeVelocities_WhenTwoBallsCollide()
        {
            // Arrange
            var dataLayer = new DataLayerCollisionFixture();
            using (BusinessLogicImplementation businessLogic = new(dataLayer))
            {
                // Utworzenie dwóch piłek, które są w kolizji
                var ball1 = new TestBall
                {
                    Position = new Vector2(50, 50),
                    
                    Velocity = new Vector2(2, 0)
                };

                var ball2 = new TestBall
                {
                    Position = new Vector2(69, 50), // Odległość = 19 < suma promieni (20)
                    
                    Velocity = new Vector2(-1, 0)
                };

                // Zapisujemy początkowe prędkości
                Vector2 initialVelocity1 = ball1.Velocity;
                Vector2 initialVelocity2 = ball2.Velocity;

                // Dodajemy piłki do listy w warstwie danych
                dataLayer.Balls.Add(ball1);
                dataLayer.Balls.Add(ball2);

                // Act
                businessLogic.BallCollision(ball1);

                // Assert
                // Sprawdzamy, czy prędkości się zmieniły po kolizji
                Assert.AreNotEqual(initialVelocity1, ball1.Velocity, "Prędkość pierwszej piłki powinna się zmienić po kolizji");
                Assert.AreNotEqual(initialVelocity2, ball2.Velocity, "Prędkość drugiej piłki powinna się zmienić po kolizji");

                Assert.IsTrue(ball1.Velocity.X < 0, "Po kolizji czołowej pierwsza piłka powinna zmienić kierunek ruchu");
                Assert.IsTrue(ball2.Velocity.X > 0, "Po kolizji czołowej druga piłka powinna zmienić kierunek ruchu");
            }
        }

        // Test sprawdzający brak kolizji między piłkami, które są za daleko
        [TestMethod]
        public void BallCollision_ShouldNotChangeVelocities_WhenBallsAreTooFarApart()
        {
            // Arrange
            var dataLayer = new DataLayerCollisionFixture();
            using (BusinessLogicImplementation businessLogic = new(dataLayer))
            {
                // Utworzenie dwóch piłek, które nie są w kolizji
                var ball1 = new TestBall
                {
                    Position = new Vector2(50, 50),
                    
                    Velocity = new Vector2(2, 0)
                };

                var ball2 = new TestBall
                {
                    Position = new Vector2(100, 50), // Odległość = 50 > suma promieni (20)
                    
                    Velocity = new Vector2(-1, 0)
                };

                // Zapisujemy początkowe prędkości
                Vector2 initialVelocity1 = ball1.Velocity;
                Vector2 initialVelocity2 = ball2.Velocity;

                // Dodajemy piłki do listy w warstwie danych
                dataLayer.Balls.Add(ball1);
                dataLayer.Balls.Add(ball2);

                // Act
                businessLogic.BallCollision(ball1);

                // Assert
                // Sprawdzamy, czy prędkości nie zmieniły się, bo piłki są za daleko
                Assert.AreEqual(initialVelocity1, ball1.Velocity, "Prędkość pierwszej piłki nie powinna się zmienić, gdy piłki są za daleko");
                Assert.AreEqual(initialVelocity2, ball2.Velocity, "Prędkość drugiej piłki nie powinna się zmienić, gdy piłki są za daleko");
            }
        }

        // Test sprawdzający przekazywanie energii podczas kolizji pod kątem
        [TestMethod]
        public void BallCollision_ShouldTransferEnergy_WhenBallsCollideAtAngle()
        {
            // Arrange
            var dataLayer = new DataLayerCollisionFixture();
            using (BusinessLogicImplementation businessLogic = new(dataLayer))
            {
                // Utworzenie dwóch piłek, które zderzają się pod kątem
                var ball1 = new TestBall
                {
                    Position = new Vector2(50, 50),
                    
                    Velocity = new Vector2(2, 1)
                };

                var ball2 = new TestBall
                {
                    Position = new Vector2(65, 60), // Kolizja pod kątem
                    
                    Velocity = new Vector2(-1, -2)
                };

                // Obliczamy początkową energię kinetyczną
                float initialEnergy =
                    (ball1.Velocity.LengthSquared() + ball2.Velocity.LengthSquared()) / 2; // Dla równych mas (m=1)

                // Dodajemy piłki do listy w warstwie danych
                dataLayer.Balls.Add(ball1);
                dataLayer.Balls.Add(ball2);

                // Act
                businessLogic.BallCollision(ball1);

                // Assert
                // Obliczamy końcową energię kinetyczną
                float finalEnergy =
                    (ball1.Velocity.LengthSquared() + ball2.Velocity.LengthSquared()) / 2;

                // W zderzeniu sprężystym energia kinetyczna powinna być zachowana (z małą tolerancją na błędy zaokrągleń)
                Assert.AreEqual(initialEnergy, finalEnergy, 0.01,
                    "Energia kinetyczna powinna być zachowana w zderzeniu sprężystym");

                // Sprawdzamy, czy prędkości zmieniły się po kolizji
                Assert.AreNotEqual(new Vector2(2, 1), ball1.Velocity,
                    "Prędkość pierwszej piłki powinna się zmienić po kolizji pod kątem");
                Assert.AreNotEqual(new Vector2(-1, -2), ball2.Velocity,
                    "Prędkość drugiej piłki powinna się zmienić po kolizji pod kątem");
            }
        }

        #region testing instrumentation

        private class DataLayerConstructorFixcure : Data.DataAbstractAPI
        {
            public override void Dispose()
            { }

            public override void Start(int numberOfBalls, Action<Vector2, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }
            public override int getWidth() => 400;
            public override int getHeight() => 400;
            public override List<Data.IBall> getAllBalls() => new List<Data.IBall>();
        }

        private class DataLayerDisposeFixcure : Data.DataAbstractAPI
        {
            internal bool Disposed = false;

            public override void Dispose()
            {
                Disposed = true;
            }

            public override void Start(int numberOfBalls, Action<Vector2, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }
            public override int getWidth() => 400;
            public override int getHeight() => 400;
            public override List<Data.IBall> getAllBalls() => new List<Data.IBall>();
        }

        private class DataLayerStartFixcure : Data.DataAbstractAPI
        {
            internal bool StartCalled = false;
            internal int NumberOfBallseCreated = -1;

            public override void Dispose()
            { }

            public override void Start(int numberOfBalls, Action<Vector2, Data.IBall> upperLayerHandler)
            {
                StartCalled = true;
                NumberOfBallseCreated = numberOfBalls;
                upperLayerHandler(new Vector2(0, 0), new DataBallFixture());
            }

            public override int getWidth() => 400;
            public override int getHeight() => 400;
            public override List<Data.IBall> getAllBalls() => new List<Data.IBall>();

            private class DataBallFixture : Data.IBall
            {
                public int BallId => BallId;

                private Vector2 _position = new Vector2(0, 0);

                public Vector2 Position => _position;
                public Vector2 Velocity { get; set; } = new Vector2(0, 0);
                public bool IsMoving { get; set; } = true;

                public event EventHandler<Vector2>? NewPositionNotification;

                public void StartThread() { }

                public void SimulatePositionChange(Vector2 newPosition)
                {
                    _position = newPosition;
                    NewPositionNotification?.Invoke(this, newPosition);
                }
            }
        }

        // Klasa pomocnicza - implementacja DataAbstractAPI dla testów kolizji
        private class DataLayerCollisionFixture : Data.DataAbstractAPI
        {
            public List<Data.IBall> Balls { get; } = new List<Data.IBall>();

            public override void Dispose() { }

            public override void Start(int numberOfBalls, Action<Vector2, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }

            public override List<Data.IBall> getAllBalls()
            {
                return Balls;
            }

            public override int getWidth() => 400;
            public override int getHeight() => 400;
        }

        private class TestBall : Data.IBall
        {
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public bool IsMoving { get; set; } = true;
            public int BallId { get; } 

            public event EventHandler<Vector2>? NewPositionNotification;

            public void StartThread() { }

            // Metoda pomocnicza do symulacji zmiany pozycji  
            public void SimulatePositionChange(Vector2 newPosition)
            {
                Position = newPosition;
                NewPositionNotification?.Invoke(this, newPosition);
            }
        }

        // Klasa pomocnicza z właściwościami dla pozycji (używana w starszych testach)
        private class TestPosition : IPosition
        {
            public double x { get; init; }
            public double y { get; init; }
        }

        #endregion testing instrumentation
    }
}