//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        private readonly Data.IBall _ball;
        private static readonly double _ballDiameter = BusinessLogicAbstractAPI.GetDimensions.BallDimension;
        private static readonly double _tableWidth = BusinessLogicAbstractAPI.GetDimensions.TableWidth;
        private static readonly double _tableHeight = BusinessLogicAbstractAPI.GetDimensions.TableHeight;

        public Ball(Data.IBall ball)
        {
            _ball = ball;
            ball.NewPositionNotification += RaisePositionChangeEvent;
        }

        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;

        #endregion IBall

        #region private

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            // Ograniczamy współrzędne, żeby nie wyszły poza stół
            double constrainedX = ConstrainPosition(e.x, _ballDiameter, _tableWidth);
            double constrainedY = ConstrainPosition(e.y, _ballDiameter, _tableHeight);

            // Odwracamy prędkość piłki jeśli uderzy w stół
            if (constrainedX != e.x || constrainedY != e.y)
            {
                Data.IVector currentVelocity = _ball.Velocity;

                if (constrainedX != e.x)
                {
                    _ball.Velocity = Data.DataAbstractAPI.GetDataLayer().CreateVector(-currentVelocity.x, currentVelocity.y);
                }

                if (constrainedY != e.y)
                {
                    _ball.Velocity = Data.DataAbstractAPI.GetDataLayer().CreateVector(currentVelocity.x, -currentVelocity.y);
                }
            }

            NewPositionNotification?.Invoke(this, new Position(constrainedX, constrainedY));
        }

        private static double ConstrainPosition(double position, double ballSize, double tableSize)
        {
            double radius = ballSize / 2.0;
            double min = radius;
            double max = tableSize - radius;

            if (position < min)
            {
                return min;
            }
            if (position > max)
            {
                return max;
            }
            return position;
        }

        #endregion private
    }
}