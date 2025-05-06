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
    internal class Ball : IBall
    {
        #region ctor
        public event EventHandler<Vector2>? NewPositionNotification;
        private Vector2 position;
        private Vector2 velocity;
        private readonly int radius = 10;
        private readonly object positionLock = new object();
        private readonly object velocityLock = new object();
        private bool isMoving;

        internal Ball(Vector2 initialPosition)
        {
            Random random = new Random();
            position = initialPosition;
            velocity = new Vector2(random.Next(1, 7), random.Next(1, 7));

        }

        #endregion ctor

        #region IBall

        int IBall.Radius => radius;
        public Vector2 Position => position;
        public Vector2 Velocity
        {
            get => velocity;
            set
            {
                lock (velocityLock)
                {
                    velocity = value;
                }
            }
        }
        public bool IsMoving
        {
            get => isMoving;
            set
            {
                isMoving = value;
            }
        }

        #endregion IBall

        #region private

        public void StartThread()
        {
            Thread thread = new Thread(Move);
            thread.Start();
        }

        private async void Move()
        {
            isMoving = true;
            while (isMoving)
            {
                lock (positionLock)
                {
                    //position = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
                    position += velocity * 0.5f;
                }

                RaiseNewPositionChangeNotification();

                double calculatedVelocity = Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2));

                int delay = (int)(1000 / Math.Max(20, calculatedVelocity * 2));

                delay = Math.Min(delay, 100);

                await Task.Delay(delay);
            }
        }

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        #endregion private
    }
}