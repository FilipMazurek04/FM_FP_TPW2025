//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2023, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//  by introducing yourself and telling us what you do with this community.
//_____________________________________________________________________________________________________________________________________

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TP.ConcurrentProgramming.BusinessLogic;
using LogicIBall = TP.ConcurrentProgramming.BusinessLogic.IBall;

namespace TP.ConcurrentProgramming.Presentation.Model
{
    internal class ModelBall : IBall
    {
        public ModelBall(double top, double left, LogicIBall underneathBall, double scale = 1.0)
        {
            _originalTop = top;
            _originalLeft = left;
            _scaleFactor = scale;
            TopBackingField = top * _scaleFactor;
            LeftBackingField = left * _scaleFactor;
            underneathBall.NewPositionNotification += NewPositionNotification;
        }

        #region IBall

        public double Top
        {
            get { return TopBackingField; }
            private set
            {
                if (TopBackingField == value)
                    return;
                TopBackingField = value;
                RaisePropertyChanged();
            }
        }

        public double Left
        {
            get { return LeftBackingField; }
            private set
            {
                if (LeftBackingField == value)
                    return;
                LeftBackingField = value;
                RaisePropertyChanged();
            }
        }

        public double Diameter { get; set; } = 0;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged

        #endregion IBall

        #region private

        private double TopBackingField;
        private double LeftBackingField;
        private double _originalTop;
        private double _originalLeft;
        private double _scaleFactor;

        internal void UpdateScale(double newScaleFactor)
        {
            if (_scaleFactor != newScaleFactor && newScaleFactor > 0)
            {
                _scaleFactor = newScaleFactor;

                // Aktualizacja pozycji
                Top = _originalTop * _scaleFactor;
                Left = _originalLeft * _scaleFactor;

                // Aktualizacja średnicy
                if (Diameter > 0)
                {
                    double originalDiameter = Diameter / (this._scaleFactor / newScaleFactor);
                    Diameter = originalDiameter * _scaleFactor;
                }
            }
        }

        private void NewPositionNotification(object sender, IPosition e)
        {
            // Zapamiętanie oryginalnych współrzędnych
            _originalTop = e.y;
            _originalLeft = e.x;

            // Ustawienie skalowanych wartości
            Top = _originalTop * _scaleFactor;
            Left = _originalLeft * _scaleFactor;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion private

        #region testing instrumentation

        [Conditional("DEBUG")]
        internal void SetLeft(double x)
        { Left = x; }

        [Conditional("DEBUG")]
        internal void SettTop(double x)
        { Top = x; }

        #endregion testing instrumentation
    }
}