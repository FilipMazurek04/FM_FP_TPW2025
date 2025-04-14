//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Windows;
using System.Windows.Controls;
using TP.ConcurrentProgramming.Presentation.ViewModel;

namespace TP.ConcurrentProgramming.PresentationView
{
    /// <summary>
    /// View implementation
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _initialized = false;

        public MainWindow()
        {
            Random random = new Random();
            InitializeComponent();

            // Sprawdzamy czy element istnieje
            if (BallCountSlider != null && SliderValueText != null)
            {
                UpdateSliderValueText();
            }

            // Inicjalizacja ViewModel
            MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
        }

        /// <summary>
        /// Raises the <seealso cref="System.Windows.Window.Closed"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.Dispose();
            base.OnClosed(e);
        }

        private void BallCountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                UpdateSliderValueText();
            }
        }

        private void UpdateSliderValueText()
        {
            try
            {
                if (BallCountSlider != null && SliderValueText != null)
                {
                    int value = (int)BallCountSlider.Value;
                    SliderValueText.Text = value.ToString();
                }
            }
            catch (Exception ex)
            {
                
                MessageBox.Show($"Błąd podczas aktualizacji tekstu: {ex.Message}");
            }
        }

        private void InitializeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_initialized)
                    return;

                if (BallCountSlider == null || !(DataContext is MainWindowViewModel viewModel))
                    return;

                int ballCount = (int)BallCountSlider.Value;

                // Uruchom symulację
                viewModel.Start(ballCount);

                // Aktualizuj interfejs
                if (BallCountText != null)
                {
                    BallCountText.Text = $"Liczba kulek w symulacji: {ballCount}";
                    BallCountText.Visibility = Visibility.Visible;
                }

                // Dezaktywuj panel konfiguracyjny
                if (ConfigPanel != null)
                    ConfigPanel.IsEnabled = false;

                // Zmień status
                if (StatusText != null)
                    StatusText.Text = "Symulacja uruchomiona";

                _initialized = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas inicjalizacji: {ex.Message}");
            }
        }
    }
}
