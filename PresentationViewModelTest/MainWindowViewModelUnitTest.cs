//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using TP.ConcurrentProgramming.Presentation.Model;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel.Test
{
    [TestClass]
    public class MainWindowViewModelUnitTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            ModelNullFixture nullModelFixture = new();
            Assert.AreEqual<int>(0, nullModelFixture.Disposed);
            Assert.AreEqual<int>(0, nullModelFixture.Started);
            Assert.AreEqual<int>(0, nullModelFixture.Subscribed);
            using (MainWindowViewModel viewModel = new(nullModelFixture))
            {
                Random random = new Random();
                int numberOfBalls = random.Next(1, 10);
                viewModel.Start(numberOfBalls);
                Assert.IsNotNull(viewModel.Balls);
                Assert.AreEqual<int>(0, nullModelFixture.Disposed);
                Assert.AreEqual<int>(numberOfBalls, nullModelFixture.Started);
                Assert.AreEqual<int>(1, nullModelFixture.Subscribed);
            }
            Assert.AreEqual<int>(1, nullModelFixture.Disposed);
        }

        [TestMethod]
        public void BehaviorTestMethod()
        {
            ModelSimulatorFixture modelSimulator = new();
            MainWindowViewModel viewModel = new(modelSimulator);
            Assert.IsNotNull(viewModel.Balls);
            Assert.AreEqual<int>(0, viewModel.Balls.Count);
            Random random = new Random();
            int numberOfBalls = random.Next(1, 10);
            viewModel.Start(numberOfBalls);
            Assert.AreEqual<int>(numberOfBalls, viewModel.Balls.Count);
            viewModel.Dispose();
            Assert.IsTrue(modelSimulator.Disposed);
            Assert.AreEqual<int>(0, viewModel.Balls.Count);
        }

        #region testing infrastructure

        private class ModelNullFixture : ModelAbstractApi
        {
            #region Test

            internal int Disposed = 0;
            internal int Started = 0;
            internal int Subscribed = 0;

            #endregion Test

            #region ModelAbstractApi

            // Implementacja brakuj¹cej w³aœciwoœci ScaleFactor
            public override double ScaleFactor { get; set; } = 1.0;

            public override void Dispose()
            {
                Disposed++;
            }

            public override void Start(int numberOfBalls)
            {
                Started = numberOfBalls;
            }

            public override IDisposable Subscribe(IObserver<ModelIBall> observer)
            {
                Subscribed++;
                return new NullDisposable();
            }

            #endregion ModelAbstractApi

            #region private

            private class NullDisposable : IDisposable
            {
                public void Dispose()
                { }
            }

            #endregion private
        }

        private class ModelSimulatorFixture : ModelAbstractApi
        {
            #region Testing indicators

            internal bool Disposed = false;

            #endregion Testing indicators

            #region ctor

            public ModelSimulatorFixture()
            {
                eventObservable = Observable.FromEventPattern<BallChaneEventArgs>(this, "BallChanged");
            }

            #endregion ctor

            #region ModelAbstractApi fixture

            // Implementacja brakuj¹cej w³aœciwoœci ScaleFactor
            public override double ScaleFactor { get; set; } = 1.0;

            public override IDisposable? Subscribe(IObserver<ModelIBall> observer)
            {
                return eventObservable?.Subscribe(x => observer.OnNext(x.EventArgs.Ball), ex => observer.OnError(ex), () => observer.OnCompleted());
            }

            public override void Start(int numberOfBalls)
            {
                for (int i = 0; i < numberOfBalls; i++)
                {
                    ModelBall newBall = new ModelBall(0, 0) { };
                    BallChanged?.Invoke(this, new BallChaneEventArgs() { Ball = newBall });
                }
            }

            public override void Dispose()
            {
                Disposed = true;
            }

            #endregion ModelAbstractApi

            #region API

            public event EventHandler<BallChaneEventArgs> BallChanged;

            #endregion API

            #region private

            private IObservable<EventPattern<BallChaneEventArgs>>? eventObservable = null;

            private class ModelBall : ModelIBall
            {
                public ModelBall(double top, double left)
                { }

                #region IBall

                public double Diameter => throw new NotImplementedException();

                public double Top => throw new NotImplementedException();

                public double Left => throw new NotImplementedException();

                #region INotifyPropertyChanged

                public event PropertyChangedEventHandler? PropertyChanged;

                #endregion INotifyPropertyChanged

                #endregion IBall
            }

            #endregion private
        }

//Test Sprawdzaj¹cy czy pi³ki siê poruszaj¹

        [TestMethod]
        public void BallPositions_ShouldChange_WhenBallsMove()
        {
           
            MovingBallsModelFixture modelFixture = new();
            using (MainWindowViewModel viewModel = new(modelFixture))
            {
                
                viewModel.Start(1); 

                
                var initialBall = viewModel.Balls[0];
                double initialTop = initialBall.Top;
                double initialLeft = initialBall.Left;

                
                modelFixture.MoveBalls();

                
                double finalTop = initialBall.Top;
                double finalLeft = initialBall.Left;

                
                Assert.AreNotEqual(initialTop, finalTop, "Pozycja Y (Top) pi³ki powinna siê zmieniæ");
                Assert.AreNotEqual(initialLeft, finalLeft, "Pozycja X (Left) pi³ki powinna siê zmieniæ");
            }
        }

        
        private class MovingBallsModelFixture : ModelAbstractApi
        {
            private readonly List<MovableBall> _balls = new List<MovableBall>();

            public override double ScaleFactor { get; set; } = 1.0;

            public override void Dispose() { }

            public override void Start(int numberOfBalls)
            {
                for (int i = 0; i < numberOfBalls; i++)
                {
                    var ball = new MovableBall(100.0, 150.0);
                    _balls.Add(ball);
                    BallChanged?.Invoke(this, new BallChaneEventArgs() { Ball = ball });
                }
            }

            public void MoveBalls()
            {
                
                foreach (var ball in _balls)
                {
                    
                    ball.Move(10.0, 15.0);
                }
            }

            public override IDisposable Subscribe(IObserver<ModelIBall> observer)
            {
                eventObservable = Observable.FromEventPattern<BallChaneEventArgs>(this, nameof(BallChanged));
                return eventObservable.Subscribe(x => observer.OnNext(x.EventArgs.Ball));
            }

            public event EventHandler<BallChaneEventArgs> BallChanged;
            private IObservable<EventPattern<BallChaneEventArgs>> eventObservable;

            
            private class MovableBall : ModelIBall, INotifyPropertyChanged
            {
                private double _top;
                private double _left;

                public MovableBall(double top, double left)
                {
                    _top = top;
                    _left = left;
                }

                public double Top => _top;
                public double Left => _left;
                public double Diameter => 20.0;

                public event PropertyChangedEventHandler PropertyChanged;

                public void Move(double deltaTop, double deltaLeft)
                {
                    _top += deltaTop;
                    _left += deltaLeft;

                    
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Top)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Left)));
                }

                protected void OnPropertyChanged(string propertyName)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }


        #endregion testing infrastructure
    }
}