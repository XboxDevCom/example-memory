using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MemoryGame.Models;

namespace MemoryGame
{
    /// <summary>
    /// Hauptseite des Memory-Spiels mit Spiellogik und Gamepad-Steuerung.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int Rows = 4;
        private const int Columns = 4;
        private const int PairCount = 8;
        private const int FlipBackDelayMs = 1000;

        private static readonly string[] Symbols =
        {
            "🎮", "🎯", "🎲", "🎨", "🎭", "🎸", "🎺", "🎻"
        };

        private readonly List<MemoryCard> _cards = new List<MemoryCard>();
        private readonly Button[,] _cardButtons = new Button[Rows, Columns];
        private readonly DispatcherTimer _gameTimer = new DispatcherTimer();
        private readonly DispatcherTimer _flipBackTimer = new DispatcherTimer();

        private MemoryCard _firstCard;
        private MemoryCard _secondCard;
        private Button _firstButton;
        private Button _secondButton;
        private int _moves;
        private int _pairsFound;
        private bool _isBusy;
        private DateTime _startTime;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="MainPage"/>-Klasse.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            _gameTimer.Interval = TimeSpan.FromSeconds(1);
            _gameTimer.Tick += GameTimer_Tick;

            _flipBackTimer.Interval = TimeSpan.FromMilliseconds(FlipBackDelayMs);
            _flipBackTimer.Tick += FlipBackTimer_Tick;

            StartNewGame();
        }

        /// <summary>
        /// Startet ein neues Spiel und setzt den kompletten Spielzustand zurück.
        /// </summary>
        private void StartNewGame()
        {
            _gameTimer.Stop();
            _flipBackTimer.Stop();

            _firstCard = null;
            _secondCard = null;
            _firstButton = null;
            _secondButton = null;
            _moves = 0;
            _pairsFound = 0;
            _isBusy = false;

            MovesText.Text = "0";
            PairsText.Text = "0 / 8";
            TimerText.Text = "0:00";
            WinText.Visibility = Visibility.Collapsed;

            CardGrid.Children.Clear();
            _cards.Clear();

            CreateCards();
            ShuffleCards();
            BuildGrid();
        }

        /// <summary>
        /// Erzeugt die Karten mit den Symbolen als Paare.
        /// </summary>
        private void CreateCards()
        {
            int id = 0;
            for (int i = 0; i < PairCount; i++)
            {
                _cards.Add(new MemoryCard { Id = id++, Symbol = Symbols[i] });
                _cards.Add(new MemoryCard { Id = id++, Symbol = Symbols[i] });
            }
        }

        /// <summary>
        /// Mischt die Karten zufällig durch.
        /// </summary>
        private void ShuffleCards()
        {
            var random = new Random();
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                MemoryCard temp = _cards[i];
                _cards[i] = _cards[j];
                _cards[j] = temp;
            }
        }

        /// <summary>
        /// Erzeugt die Buttons für das 4x4-Raster und konfiguriert die Gamepad-Navigation.
        /// </summary>
        private void BuildGrid()
        {
            int index = 0;
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    MemoryCard card = _cards[index];
                    Button button = CreateCardButton(card, row, col);
                    _cardButtons[row, col] = button;
                    CardGrid.Children.Add(button);
                    index++;
                }
            }

            ConfigureGamepadNavigation();
        }

        /// <summary>
        /// Erzeugt einen einzelnen Karten-Button an der angegebenen Grid-Position.
        /// </summary>
        /// <param name="card">Die zugehörige Karte.</param>
        /// <param name="row">Die Zeile im Raster.</param>
        /// <param name="col">Die Spalte im Raster.</param>
        /// <returns>Der konfigurierte Button.</returns>
        private Button CreateCardButton(MemoryCard card, int row, int col)
        {
            var button = new Button
            {
                Content = "?",
                FontSize = 48,
                Width = 120,
                Height = 120,
                Margin = new Thickness(6),
                Tag = card,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            button.Click += CardButton_Click;

            Grid.SetRow(button, row);
            Grid.SetColumn(button, col);

            return button;
        }

        /// <summary>
        /// Konfiguriert die Gamepad-Navigation (D-Pad) zwischen den Karten im Raster.
        /// </summary>
        private void ConfigureGamepadNavigation()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Button button = _cardButtons[row, col];

                    if (row > 0)
                        button.XYFocusUp = _cardButtons[row - 1, col];
                    if (row < Rows - 1)
                        button.XYFocusDown = _cardButtons[row + 1, col];
                    if (col > 0)
                        button.XYFocusLeft = _cardButtons[row, col - 1];
                    if (col < Columns - 1)
                        button.XYFocusRight = _cardButtons[row, col + 1];
                }
            }

            NewGameButton.XYFocusUp = _cardButtons[Rows - 1, 1];

            _cardButtons[3, 0].XYFocusDown = NewGameButton;
        }

        /// <summary>
        /// Behandelt das Klicken/Auslösen eines Karten-Buttons.
        /// </summary>
        /// <param name="sender">Der geklickte Button.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isBusy)
                return;

            var button = sender as Button;
            var card = button?.Tag as MemoryCard;

            if (card == null || card.IsMatched || card.IsRevealed)
                return;

            if (_firstCard != null && _secondCard != null)
                return;

            FlipCardUp(button, card);

            if (_firstCard == null)
            {
                _firstCard = card;
                _firstButton = button;
                StartGameTimerIfNeeded();
            }
            else
            {
                _secondCard = card;
                _secondButton = button;
                _moves++;
                MovesText.Text = _moves.ToString();
                EvaluatePair();
            }
        }

        /// <summary>
        /// Deckt eine Karte auf und zeigt ihr Symbol an.
        /// </summary>
        /// <param name="button">Der Karten-Button.</param>
        /// <param name="card">Die zugehörige Karte.</param>
        private void FlipCardUp(Button button, MemoryCard card)
        {
            card.IsRevealed = true;
            button.Content = card.Symbol;
        }

        /// <summary>
        /// Startet den Spiel-Timer, falls er noch nicht läuft.
        /// </summary>
        private void StartGameTimerIfNeeded()
        {
            if (!_gameTimer.IsEnabled)
            {
                _startTime = DateTime.Now;
                _gameTimer.Start();
            }
        }

        /// <summary>
        /// Wertet das aufgedeckte Kartenpaar aus.
        /// </summary>
        private void EvaluatePair()
        {
            if (_firstCard.Symbol == _secondCard.Symbol)
            {
                _firstCard.IsMatched = true;
                _secondCard.IsMatched = true;
                _pairsFound++;
                PairsText.Text = _pairsFound + " / 8";
                ResetSelection();

                if (_pairsFound == PairCount)
                {
                    HandleWin();
                }
            }
            else
            {
                _isBusy = true;
                _flipBackTimer.Start();
            }
        }

        /// <summary>
        /// Setzt die aktuelle Auswahl zurück.
        /// </summary>
        private void ResetSelection()
        {
            _firstCard = null;
            _secondCard = null;
            _firstButton = null;
            _secondButton = null;
        }

        /// <summary>
        /// Behandelt den Timer-Tick zum Zurückdecken nicht übereinstimmender Karten.
        /// </summary>
        /// <param name="sender">Der Timer.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void FlipBackTimer_Tick(object sender, object e)
        {
            _flipBackTimer.Stop();

            if (_firstButton != null && _firstCard != null)
            {
                _firstCard.IsRevealed = false;
                _firstButton.Content = "?";
            }
            if (_secondButton != null && _secondCard != null)
            {
                _secondCard.IsRevealed = false;
                _secondButton.Content = "?";
            }

            ResetSelection();
            _isBusy = false;
        }

        /// <summary>
        /// Behandelt den Spiel-Timer-Tick zur Anzeige der verstrichenen Zeit.
        /// </summary>
        /// <param name="sender">Der Timer.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void GameTimer_Tick(object sender, object e)
        {
            TimeSpan elapsed = DateTime.Now - _startTime;
            TimerText.Text = (int)elapsed.TotalMinutes + ":" + elapsed.Seconds.ToString("00");
        }

        /// <summary>
        /// Behandelt den Gewinnfall und zeigt die Gewinnmeldung an.
        /// </summary>
        private void HandleWin()
        {
            _gameTimer.Stop();
            WinText.Text = string.Format(
                "Herzlichen Glückwunsch! Du hast alle {0} Paare in {1} Zügen gefunden!",
                PairCount, _moves);
            WinText.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Behandelt das Klicken auf den "Neues Spiel"-Button.
        /// </summary>
        /// <param name="sender">Der geklickte Button.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }
    }
}
