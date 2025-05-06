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

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        private readonly BusinessLogicImplementation _logic = new BusinessLogicImplementation();
        public Ball(Data.IBall ball)
        {
            ball.NewPositionNotification += RaisePositionChangeEvent;
        }

        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;

        #endregion IBall

        #region private

        private void RaisePositionChangeEvent(object? sender, Vector2 position)
        {
            Data.IBall dataBall = (Data.IBall)sender!;

            _logic.WallCollision(dataBall);
            _logic.BallCollision(dataBall);
            NewPositionNotification?.Invoke(this, new Position(position.X, position.Y));
        }
    }
    #endregion private
}