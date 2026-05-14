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
    [ObservableProperty] private bool _isBlackjackMenuVisible = false;
    [ObservableProperty] private bool _isBlackjackGameVisible = false;

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

    private List<Card> _bjDeck = new();
    private List<Card> _bjPlayerHand = new();
    private List<Card> _bjDealerHand = new();
    [ObservableProperty] private ObservableCollection<Card> _bjPlayerCards = new();
    [ObservableProperty] private ObservableCollection<Card> _bjDealerCards = new();
    [ObservableProperty] private int _playerHandValue = 0;
    [ObservableProperty] private string _bjMessage = "";
    [ObservableProperty] private bool _bjCanAct = false;
    [ObservableProperty] private bool _bjGameOver = false;

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
    private void GoToBlackjackMenu() { HideAll(); IsBlackjackMenuVisible = true; }

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

    [RelayCommand]
    private void StartBlackjack()
    {
        HideAll();
        IsBlackjackGameVisible = true;
        _bjDeck = GenerateBjDeck();
        _bjPlayerHand.Clear();
        _bjDealerHand.Clear();
        _bjPlayerHand.Add(DrawBjCard());
        _bjDealerHand.Add(DrawBjCard());
        _bjPlayerHand.Add(DrawBjCard());
        _bjDealerHand.Add(DrawBjCard());
        BjGameOver = false;
        BjCanAct = true;
        UpdateBjDisplay(hideDealer: true);
        if (GetHandValue(_bjPlayerHand) == 21)
            BjStand();
        else
            BjMessage = "Hit (dobierz) czy Stand (zostań)?";
    }

    [RelayCommand]
    private void BjHit()
    {
        _bjPlayerHand.Add(DrawBjCard());
        int value = GetHandValue(_bjPlayerHand);
        if (value > 21)
        {
            FinishBlackjack();
        }
        else if (value == 21)
        {
            BjStand();
        }
        else
        {
            UpdateBjDisplay(hideDealer: true);
            BjMessage = "Hit (dobierz) czy Stand (zostań)?";
        }
    }

    [RelayCommand]
    private void BjStand()
    {
        BjCanAct = false;
        while (GetHandValue(_bjDealerHand) < 17)
            _bjDealerHand.Add(DrawBjCard());
        FinishBlackjack();
    }

    private void FinishBlackjack()
    {
        BjCanAct = false;
        BjGameOver = true;
        UpdateBjDisplay(hideDealer: false);
        int pv = GetHandValue(_bjPlayerHand);
        int dv = GetHandValue(_bjDealerHand);
        if (pv > 21)
            BjMessage = $"Bust! Przegrałeś. (Ty: {pv}, Krupier: {dv})";
        else if (dv > 21)
            BjMessage = $"Krupier się spalił! Wygrałeś. (Ty: {pv}, Krupier: {dv})";
        else if (pv > dv)
            BjMessage = $"Wygrałeś! (Ty: {pv}, Krupier: {dv})";
        else if (pv < dv)
            BjMessage = $"Przegrałeś. (Ty: {pv}, Krupier: {dv})";
        else
            BjMessage = $"Remis! (Ty: {pv}, Krupier: {dv})";

        if (!_game2History.ContainsKey(CurrentPlayerId))
            _game2History[CurrentPlayerId] = new List<string>();
        _game2History[CurrentPlayerId].Add(BjMessage);
    }

    private List<Card> GenerateBjDeck()
    {
        var deck = new List<Card>();
        string[] suits = { "♥", "♦", "♣", "♠" };
        string[] names = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jopek", "Dama", "Król", "As" };
        for (int s = 0; s < 4; s++)
            for (int n = 0; n < 13; n++)
                deck.Add(new Card { Name = $"{names[n]} {suits[s]}", Value = n + 2 });
        return deck.OrderBy(_ => Guid.NewGuid()).ToList();
    }

    private Card DrawBjCard()
    {
        var card = _bjDeck[0];
        _bjDeck.RemoveAt(0);
        return card;
    }

    internal int GetHandValue(List<Card> hand)
    {
        int total = 0, aces = 0;
        foreach (var card in hand)
        {
            if (card.Value >= 11 && card.Value <= 13) total += 10;
            else if (card.Value == 14) { total += 11; aces++; }
            else total += card.Value;
        }
        while (total > 21 && aces > 0) { total -= 10; aces--; }
        return total;
    }

    private void UpdateBjDisplay(bool hideDealer)
    {
        BjPlayerCards = new ObservableCollection<Card>(_bjPlayerHand);
        PlayerHandValue = GetHandValue(_bjPlayerHand);
        if (hideDealer && _bjDealerHand.Count > 0)
            BjDealerCards = new ObservableCollection<Card>(new[] { _bjDealerHand[0], new Card { Name = "???", Value = 0 } });
        else
            BjDealerCards = new ObservableCollection<Card>(_bjDealerHand);
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
        IsBlackjackMenuVisible = false;
        IsBlackjackGameVisible = false;
    }
}

public class Card
{
    public string Name { get; set; } = "";
    public int Value { get; set; }
}