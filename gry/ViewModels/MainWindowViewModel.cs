using Avalonia.Remote.Protocol.Viewport;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gry.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // --- WIDOCZNOŚĆ PANELI (Nawigacja) ---
    [ObservableProperty] private bool _isStartMenuVisible = true;
    [ObservableProperty] private bool _isAddPlayerVisible = false;
    [ObservableProperty] private bool _isLoginVisible = false;
    [ObservableProperty] private bool _isGamesMenuVisible = false;
    [ObservableProperty] private bool _isHlMenuVisible = false;
    [ObservableProperty] private bool _isHlInfoVisible = false;
    [ObservableProperty] private bool _isHlGameVisible = false;

    // --- DANE GRACZY I LOGOWANIE ---
    private HashSet<string> _registeredPlayers = new();
    private Dictionary<string, string> _playerStats = new();

    [ObservableProperty] private string _inputPlayerId = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private string _currentPlayerId = "";
    [ObservableProperty] private string _lobbyStats = "Zaloguj się, aby zagrać.";

    // --- DANE GRY HIGHER/LOWER ---
    private List<Card> _deck = new();
    [ObservableProperty] private string _currentCardName = "";
    [ObservableProperty] private int _cardsLeft = 0;
    [ObservableProperty] private int _currentScore = 0;
    [ObservableProperty] private int _totalAttempts = 0;
    [ObservableProperty] private string _gameMessage = "";
    private Card? _currentCard;

    // --- KOMENDY NAWIGACJI ---
    [RelayCommand]
    private void GoToAddPlayer() { HideAll(); IsAddPlayerVisible = true; ErrorMessage = ""; InputPlayerId = ""; }

    [RelayCommand]
    private void GoToLogin() { HideAll(); IsLoginVisible = true; ErrorMessage = ""; InputPlayerId = ""; }

    [RelayCommand]
    private void GoToGamesMenu()
    {
        HideAll();
        IsGamesMenuVisible = true;
        LobbyStats = _playerStats.ContainsKey(CurrentPlayerId)
            ? $"Gracz {CurrentPlayerId} Statystyki: {_playerStats[CurrentPlayerId]}"
            : $"Gracz {CurrentPlayerId} Statystyki: Brak gier";
    }

    [RelayCommand]
    private void GoToStartMenu() { HideAll(); IsStartMenuVisible = true; CurrentPlayerId = ""; }

    [RelayCommand]
    private void GoToHlMenu() { HideAll(); IsHlMenuVisible = true; }

    [RelayCommand]
    private void GoToHlInfo() { HideAll(); IsHlInfoVisible = true; }

    // --- KOMENDY AKCJI (LOGOWANIE / REJESTRACJA) ---
    [RelayCommand]
    private void RegisterPlayer()
    {
        if (InputPlayerId.Length >= 3 && InputPlayerId.Length <= 10 && InputPlayerId.All(char.IsDigit))
        {
            if (_registeredPlayers.Add(InputPlayerId))
            {
                GoToStartMenu();
            }
            else
            {
                ErrorMessage = "Gracz o takim ID już istnieje!";
            }
        }
        else
        {
            ErrorMessage = "ID musi składać się z 3 do 10 cyfr!";
        }
    }

    [RelayCommand]
    private void LoginPlayer()
    {
        if (_registeredPlayers.Contains(InputPlayerId))
        {
            CurrentPlayerId = InputPlayerId;
            GoToGamesMenu();
        }
        else
        {
            ErrorMessage = "Nie znaleziono gracza o takim ID!";
        }
    }

    // --- LOGIKA GRY HIGHER/LOWER ---
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

        if (oldCardValue == newCardValue)
        {
            GameMessage = "Remis! Gramy dalej.";
        }
        else if ((guessHigher && newCardValue > oldCardValue) || (!guessHigher && newCardValue < oldCardValue))
        {
            CurrentScore++;
            GameMessage = "Dobrze! Punkt dla Ciebie.";
        }
        else
        {
            GameMessage = "Źle! Brak punktu.";
        }

        if (_deck.Count == 0)
        {
            GameMessage = $"Koniec gry! Twój wynik: {CurrentScore}/{TotalAttempts}";
            _playerStats[CurrentPlayerId] = $"{CurrentScore}/{TotalAttempts} pkt.";
        }
    }

    private void InitializeDeck()
    {
        _deck.Clear();
        string[] suits = { "♥", "♦", "♣", "♠" };
        string[] names = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jopek", "Dama", "Król", "As" };

        for (int s = 0; s < 4; s++)
        {
            for (int n = 0; n < 13; n++)
            {
                _deck.Add(new Card { Name = $"{names[n]} {suits[s]}", Value = n + 2 });
            }
        }
        // Tasowanie
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

    // --- POMOCNICZE ---
    private void HideAll()
    {
        IsStartMenuVisible = false;
        IsAddPlayerVisible = false;
        IsLoginVisible = false;
        IsGamesMenuVisible = false;
        IsHlMenuVisible = false;
        IsHlInfoVisible = false;
        IsHlGameVisible = false;
    }
}

// Klasa pomocnicza dla Karty (umieszczona na dole tego samego pliku dla wygody)
public class Card
{
    public string Name { get; set; } = "";
    public int Value { get; set; }
}