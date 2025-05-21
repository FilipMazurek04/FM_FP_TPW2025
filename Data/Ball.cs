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
    internal class Ball : IBall
    {
        #region ctor
        public event EventHandler<Vector2>? NewPositionNotification;
        private int ballId;
        private Vector2 position;
        private Vector2 velocity;
        private readonly int radius = 10;
        private readonly object positionLock = new object();
        private readonly object velocityLock = new object();
        private bool isMoving;
        private readonly Logger logger;

        internal Ball(int id, Vector2 initialPosition)
        {
            Random random = new Random();
            ballId = id;
            position = initialPosition;
            velocity = new Vector2(random.Next(1, 7), random.Next(1, 7));
            logger = Logger.CreateLogger();

        }

        #endregion ctor

        #region IBall

        int IBall.Radius => radius;

        public int BallId => ballId;

        public Vector2 Position
        {
            get
            {
                lock (positionLock)
                {
                    return position;
                }
            }
        }
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

            Stopwatch stopwatch = new();
            stopwatch.Start();
            float startingTime = 0f;

            while (isMoving)
            {
                float currentTime = stopwatch.ElapsedMilliseconds;
                float delta = currentTime - startingTime;
                if (delta >= 1f / 60f)
                {
                    lock (positionLock)
                    {
                        //position = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
                        position += velocity * 0.5f;
                    }

                    logger.Log(this, DateTime.Now);
                    startingTime = currentTime;

                    RaiseNewPositionChangeNotification();

                    //double calculatedVelocity = Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2));

                    //int delay = (int)(1000 / Math.Max(20, calculatedVelocity * 2));

                    //delay = Math.Min(delay, 100);

                    await Task.Delay(TimeSpan.FromSeconds(1f / 60f));
                }
            }
        }

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        #endregion private
    }
}