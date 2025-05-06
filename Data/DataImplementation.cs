//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using System.Numerics;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        private bool Disposed = false;
        // Pozostawiamy wymiary stołu, ale możemy je dostosować jeśli potrzeba
        private readonly int width = 400;
        private readonly int height = 400;
        private List<IBall> BallsList = [];

        public DataImplementation()
        {
            // (1000ms / 60fps) = 16.7ms
            //const int frameRateMs = 17;
            //MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(frameRateMs));
            //_moveScaleFactor = frameRateMs / 100.0;
        }
        public override int getWidth()
        {
            return width;
        }
        public override int getHeight()
        {
            return height;
        }
        public override List<IBall> getAllBalls()
        {
            return BallsList;
        }


        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<Vector2, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                Vector2 startingPosition = new(random.Next(10, width - 10), random.Next(10, height - 10));
                Ball newBall = new(startingPosition);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);

                // Uruchamiamy wątek kulki - to kluczowa linia!
                newBall.StartThread();
            }
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    // Zatrzymaj wszystkie kulki przed wyczyszczeniem listy
                    foreach (var ball in BallsList)
                    {
                        ball.IsMoving = false;
                    }

                    // Daj czas na zakończenie wątków
                    Thread.Sleep(100);

                    BallsList.Clear();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}