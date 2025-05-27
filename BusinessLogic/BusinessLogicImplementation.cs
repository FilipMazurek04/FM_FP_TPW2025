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
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        private readonly object _collisionLock = new object();
        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;

        #region ctor

        public BusinessLogicImplementation() : this(null)
        { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
        }

        #endregion ctor

        #region BusinessLogicAbstractAPI

        internal void WallCollision(Data.IBall ball)
        {
            double tableWidth = layerBellow.getWidth();
            double tableHeight = layerBellow.getHeight();
            
            // 10 - promień piłki

            if (ball.Position.X - 10 <= 0 || ball.Position.X + 10 >= tableWidth)
                ball.Velocity = new Vector2(-ball.Velocity.X, ball.Velocity.Y);
            if (ball.Position.Y - 10 <= 0 || ball.Position.Y + 10 >= tableHeight)
                ball.Velocity = new Vector2(ball.Velocity.X, -ball.Velocity.Y);
        }

        internal void BallCollision(Data.IBall ball)
        {
            lock (_collisionLock)
            {
                List<Data.IBall> balls = layerBellow.getAllBalls();
                foreach (Data.IBall otherBall in balls)
                {
                    if (otherBall != ball)
                    {
                        double distance = Math.Sqrt(Math.Pow(ball.Position.X - otherBall.Position.X, 2) + Math.Pow(ball.Position.Y - otherBall.Position.Y, 2));
                        if (distance <= 10 + 10)
                        {
                            Vector2 collisionVector = Vector2.Normalize(new Vector2(
                                (float)(otherBall.Position.X - ball.Position.X),
                                (float)(otherBall.Position.Y - ball.Position.Y)));

                            if (distance < 10 + 10)
                            {
                                float overlap = (float)(10 + 10 - distance) / 2;
                                // Nie zmieniamy bezpośrednio pozycji, tylko prędkości
                            }

                            float v1 = Vector2.Dot(ball.Velocity, collisionVector);
                            float v2 = Vector2.Dot(otherBall.Velocity, collisionVector);

                            float newV1 = v2;
                            float newV2 = v1;

                            Vector2 v1Change = collisionVector * (newV1 - v1);
                            Vector2 v2Change = collisionVector * (newV2 - v2);

                            ball.Velocity += v1Change;
                            otherBall.Velocity += v2Change;
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            layerBellow.Start(numberOfBalls, (startingPosition, databall) => upperLayerHandler(new Position(startingPosition.X, startingPosition.Y), new Ball(databall)));
        }

        #endregion BusinessLogicAbstractAPI

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}