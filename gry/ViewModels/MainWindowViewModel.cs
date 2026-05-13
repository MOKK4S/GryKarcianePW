using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace gry.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isStartMenuVisible = true;
    [ObservableProperty] private bool _isAddPlayerVisible = false;
    [ObservableProperty] private bool _isLoginVisible = false;
    [ObservableProperty] private bool _isGamesMenuVisible = false;
    [ObservableProperty] private bool _isHlMenuVisible = false;
    [ObservableProperty] private bool _isHlInfoVisible = false;
    [ObservableProperty] private bool _isHlGameVisible = false;
    [ObservableProperty] private bool _isHistoryVisible = false;

    private HashSet<string> _registeredPlayers = new();
    private Dictionary<string, List<string>> _hlHistory = new();
    private Dictionary<string, List<string>> _game2History = new();
    private Dictionary<string, List<string>> _game3History = new();

    [ObservableProperty] private string _inputPlayerId = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private string _currentPlayerId = "";

    [ObservableProperty] private ObservableCollection<string> _currentHlResults = new();
    [ObservableProperty] private ObservableCollection<string> _currentGame2Results = new();
    [ObservableProperty] private ObservableCollection<string> _currentGame3Results = new();

    private List<Card> _deck = new();
    [ObservableProperty] private string _currentCardName = "";
    [ObservableProperty] private int _cardsLeft = 0;
    [ObservableProperty] private int _currentScore = 0;
    [ObservableProperty] private int _totalAttempts = 0;
    [ObservableProperty] private string _gameMessage = "";
    private Card? _currentCard;

    [RelayCommand]
    private void GoToAddPlayer() { HideAll(); IsAddPlayerVisible = true; ErrorMessage = ""; InputPlayerId = ""; }

    [RelayCommand]
    private void GoToLogin() { HideAll(); IsLoginVisible = true; ErrorMessage = ""; InputPlayerId = ""; }

    [RelayCommand]
    private void GoToGamesMenu() { HideAll(); IsGamesMenuVisible = true; }

    [RelayCommand]
    private void GoToStartMenu() { HideAll(); IsStartMenuVisible = true; CurrentPlayerId = ""; }

    [RelayCommand]
    private void GoToHlMenu() { HideAll(); IsHlMenuVisible = true; }

    [RelayCommand]
    private void GoToHlInfo() { HideAll(); IsHlInfoVisible = true; }

    [RelayCommand]
    private void GoToHistory()
    {
        HideAll();
        IsHistoryVisible = true;

        CurrentHlResults = new ObservableCollection<string>(_hlHistory.GetValueOrDefault(CurrentPlayerId, new List<string>()));
        CurrentGame2Results = new ObservableCollection<string>(_game2History.GetValueOrDefault(CurrentPlayerId, new List<string>()));
        CurrentGame3Results = new ObservableCollection<string>(_game3History.GetValueOrDefault(CurrentPlayerId, new List<string>()));
    }

    [RelayCommand]
    private void RegisterPlayer()
    {
        if (InputPlayerId.Length >= 3 && InputPlayerId.Length <= 10 && InputPlayerId.All(char.IsDigit))
        {
            if (_registeredPlayers.Add(InputPlayerId))
            {
                _hlHistory[InputPlayerId] = new List<string>();
                _game2History[InputPlayerId] = new List<string>();
                _game3History[InputPlayerId] = new List<string>();
                GoToStartMenu();
            }
            else { ErrorMessage = "Gracz o takim ID już istnieje!"; }
        }
        else { ErrorMessage = "ID musi składać się z 3 do 10 cyfr!"; }
    }

    [RelayCommand]
    private void LoginPlayer()
    {
        if (_registeredPlayers.Contains(InputPlayerId))
        {
            CurrentPlayerId = InputPlayerId;
            GoToGamesMenu();
        }
        else { ErrorMessage = "Nie znaleziono gracza o takim ID!"; }
    }

    [RelayCommand]
    private void StartGame()
    {
        HideAll();
        IsHlGameVisible = true;
        CurrentScore = 0;
        TotalAttempts = 0;
        GameMessage = "Zgadnij: Następna karta będzie wyższa czy niższa?";
        InitializeDeck();
        DrawCard();
    }

    [RelayCommand]
    private void GuessHigher() => MakeGuess(true);

    [RelayCommand]
    private void GuessLower() => MakeGuess(false);

    private void MakeGuess(bool guessHigher)
    {
        if (_deck.Count == 0) return;
        int oldCardValue = _currentCard!.Value;
        DrawCard();
        int newCardValue = _currentCard!.Value;
        TotalAttempts++;

        if (oldCardValue == newCardValue) { GameMessage = "Remis! Gramy dalej."; }
        else if ((guessHigher && newCardValue > oldCardValue) || (!guessHigher && newCardValue < oldCardValue))
        {
            CurrentScore++;
            GameMessage = "Dobrze! Punkt dla Ciebie.";
        }
        else { GameMessage = "Źle! Brak punktu."; }

        if (_deck.Count == 0)
        {
            GameMessage = $"Koniec gry! Twój wynik: {CurrentScore}/{TotalAttempts}";
            _hlHistory[CurrentPlayerId].Add($"{CurrentScore}/{TotalAttempts}");
        }
    }

    private void InitializeDeck()
    {
        _deck.Clear();
        string[] suits = { "♥", "♦", "♣", "♠" };
        string[] names = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jopek", "Dama", "Król", "As" };
        for (int s = 0; s < 4; s++)
        {
            for (int n = 0; n < 13; n++) { _deck.Add(new Card { Name = $"{names[n]} {suits[s]}", Value = n + 2 }); }
        }
        _deck = _deck.OrderBy(x => Guid.NewGuid()).ToList();
    }

    private void DrawCard()
    {
        if (_deck.Count > 0)
        {
            _currentCard = _deck[0];
            _deck.RemoveAt(0);
            CurrentCardName = _currentCard.Name;
            CardsLeft = _deck.Count;
        }
    }

    private void HideAll()
    {
        IsStartMenuVisible = false;
        IsAddPlayerVisible = false;
        IsLoginVisible = false;
        IsGamesMenuVisible = false;
        IsHlMenuVisible = false;
        IsHlInfoVisible = false;
        IsHlGameVisible = false;
        IsHistoryVisible = false;
    }
}

public class Card
{
    public string Name { get; set; } = "";
    public int Value { get; set; }
}