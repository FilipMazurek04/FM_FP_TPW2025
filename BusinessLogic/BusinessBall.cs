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
        public Ball(Data.IBall ball)
        {
            ball.NewPositionNotification += RaisePositionChangeEvent;
        }

        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;

        #endregion IBall

        #region private

        private void RaisePositionChangeEvent(object? sender, Data.IVector pos)
        {
            Data.IBall dataBall = (Data.IBall)sender!;

            // Ograniczamy współrzędne, żeby nie wyszły poza stół
            ConstrainPosition(dataBall, pos);
            NewPositionNotification?.Invoke(this, new Position(pos.x, pos.y));
        }

        private void ConstrainPosition(Data.IBall ball, Data.IVector pos)
        {
            // Wymiary stołu i kulki z API warstwy logiki biznesowej
            double _ballDiameter = BusinessLogicAbstractAPI.GetDimensions.BallDimension;
            double _tableWidth = BusinessLogicAbstractAPI.GetDimensions.TableWidth;
            double _tableHeight = BusinessLogicAbstractAPI.GetDimensions.TableHeight;

            // Aktualna pozycja i prędkość kulki
            double posX = pos.x;
            double posY = pos.y;
            Data.IVector velocity = ((Data.IBall)ball).Velocity;

            bool touchedWall = false;
            double newVelocityX = velocity.x;
            double newVelocityY = velocity.y;
            double newPosX = posX;
            double newPosY = posY;

            // Kolizja z lewą ścianą
            if (posX <= 0)
            {
                newVelocityX = -velocity.x;  // Odbijamy prędkość w osi X
                newPosX = 0;  // Ustawiamy tuż przy lewej ścianie
                touchedWall = true;
            }
            // Kolizja z prawą ścianą
            else if (posX + _ballDiameter >= _tableWidth)  // Poprawiony warunek
            {
                newVelocityX = -velocity.x;  // Odbijamy prędkość w osi X
                newPosX = _tableWidth - _ballDiameter;  // Ustawiamy tuż przy prawej ścianie
                touchedWall = true;
            }

            // Kolizja z górną ścianą
            if (posY <= 0)
            {
                newVelocityY = -velocity.y;  // Odbijamy prędkość w osi Y
                newPosY = 0;  // Ustawiamy tuż przy górnej ścianie
                touchedWall = true;
            }
            // Kolizja z dolną ścianą
            else if (posY + _ballDiameter >= _tableHeight)
            {
                newVelocityY = -velocity.y;  // Odbijamy prędkość w osi Y
                newPosY = _tableHeight - _ballDiameter;  // Ustawiamy tuż przy dolnej ścianie
                touchedWall = true;
            }

            // Jeśli kulka dotknęła ściany, zmieniamy prędkość używając interfejsu abstrakcyjnego
            if (touchedWall)
            {
                ((Data.IBall)ball).Velocity = new BusinessVector(newVelocityX, newVelocityY);
                ((Data.IBall)ball).SetPosition(new BusinessVector(newPosX, newPosY));
                touchedWall = false;
            }
        }

        private class BusinessVector : Data.IVector
        {
            public double x { get; init; }
            public double y { get; init; }

            public BusinessVector(double xNew, double yNew)
            {
                x = xNew;
                y = yNew;
            }
        }

    }
        #endregion private
}