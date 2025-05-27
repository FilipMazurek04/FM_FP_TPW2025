//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Numerics;

namespace TP.ConcurrentProgramming.Data
{
    public abstract class DataAbstractAPI : IDisposable
    {
        #region Layer Factory

        public static DataAbstractAPI GetDataLayer()
        {
            return modelInstance.Value;
        }

        #endregion Layer Factory

        #region public API

        public abstract int getWidth();
        public abstract int getHeight();

        public abstract List<IBall> getAllBalls();

        public abstract void Start(int numberOfBalls, Action<Vector2, IBall> upperLayerHandler);

        #endregion public API

        #region IDisposable

        public abstract void Dispose();

        #endregion IDisposable

        #region private

        private static Lazy<DataAbstractAPI> modelInstance = new Lazy<DataAbstractAPI>(() => new DataImplementation());

        #endregion private
    }

    public interface IBall
    {
        event EventHandler<Vector2> NewPositionNotification;

        int BallId { get; }
        Vector2 Position { get; }
        Vector2 Velocity { get; set; }
        bool IsMoving { get; set; }
        void StartThread();
    }
}