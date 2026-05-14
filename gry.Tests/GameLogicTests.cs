using System.Collections.Generic;
using gry.ViewModels;
using Xunit;

namespace gry.Tests;

public class CardValueTests
{
    private readonly MainWindowViewModel _vm = new();
    private static Card C(int v) => new() { Name = "X", Value = v };

    [Fact] public void Empty_Returns0() => Assert.Equal(0, _vm.GetHandValue(new List<Card>()));
    [Fact] public void TwoAces_Returns12() => Assert.Equal(12, _vm.GetHandValue(new List<Card> { C(14), C(14) }));
    [Fact] public void AcePlusNine_Returns20() => Assert.Equal(20, _vm.GetHandValue(new List<Card> { C(14), C(9) }));
    [Fact] public void AcePlusNinePlusFive_Returns15() => Assert.Equal(15, _vm.GetHandValue(new List<Card> { C(14), C(9), C(5) }));
    [Fact] public void FourAces_Returns14() => Assert.Equal(14, _vm.GetHandValue(new List<Card> { C(14), C(14), C(14), C(14) }));
    [Fact] public void JackPlusQueen_Returns20() => Assert.Equal(20, _vm.GetHandValue(new List<Card> { C(11), C(12) }));
    [Fact] public void Blackjack_Returns21() => Assert.Equal(21, _vm.GetHandValue(new List<Card> { C(11), C(12), C(14) }));
    [Fact] public void Bust_Returns25() => Assert.Equal(25, _vm.GetHandValue(new List<Card> { C(11), C(12), C(5) }));
}

public class HigherLowerTests
{
    private static MainWindowViewModel CreateLoggedInVm()
    {
        var vm = new MainWindowViewModel();
        vm.InputPlayerId = "12345";
        vm.RegisterPlayerCommand.Execute(null);
        vm.InputPlayerId = "12345";
        vm.LoginPlayerCommand.Execute(null);
        return vm;
    }

    [Fact]
    public void StartGame_InitializesState()
    {
        var vm = CreateLoggedInVm();
        vm.StartGameCommand.Execute(null);
        Assert.Equal(51, vm.CardsLeft);
        Assert.NotEmpty(vm.CurrentCardName);
        Assert.Equal(0, vm.CurrentScore);
        Assert.Equal(0, vm.TotalAttempts);
    }

    [Fact]
    public void Guess_AlwaysIncrementsAttempts()
    {
        var vm = CreateLoggedInVm();
        vm.StartGameCommand.Execute(null);
        vm.GuessHigherCommand.Execute(null);
        Assert.Equal(1, vm.TotalAttempts);
    }

    [Fact]
    public void FullGame_ShowsGameOverMessage()
    {
        var vm = CreateLoggedInVm();
        vm.StartGameCommand.Execute(null);
        for (int i = 0; i < 51; i++)
            vm.GuessHigherCommand.Execute(null);
        Assert.Contains("Koniec gry", vm.GameMessage);
    }
}

public class PlayerManagementTests
{
    [Fact]
    public void Register_ValidId_Succeeds()
    {
        var vm = new MainWindowViewModel();
        vm.InputPlayerId = "12345";
        vm.RegisterPlayerCommand.Execute(null);
        Assert.Equal("", vm.ErrorMessage);
    }

    [Fact]
    public void Register_DuplicateId_ShowsError()
    {
        var vm = new MainWindowViewModel();
        vm.InputPlayerId = "12345";
        vm.RegisterPlayerCommand.Execute(null);
        vm.InputPlayerId = "12345";
        vm.RegisterPlayerCommand.Execute(null);
        Assert.Equal("Gracz o takim ID już istnieje!", vm.ErrorMessage);
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345678901")]
    [InlineData("abc")]
    [InlineData("12a45")]
    public void Register_InvalidId_ShowsError(string id)
    {
        var vm = new MainWindowViewModel();
        vm.InputPlayerId = id;
        vm.RegisterPlayerCommand.Execute(null);
        Assert.Equal("ID musi składać się z 3 do 10 cyfr!", vm.ErrorMessage);
    }

    [Fact]
    public void Login_KnownId_SetsCurrentPlayer()
    {
        var vm = new MainWindowViewModel();
        vm.InputPlayerId = "12345";
        vm.RegisterPlayerCommand.Execute(null);
        vm.InputPlayerId = "12345";
        vm.LoginPlayerCommand.Execute(null);
        Assert.Equal("12345", vm.CurrentPlayerId);
    }

    [Fact]
    public void Login_UnknownId_ShowsError()
    {
        var vm = new MainWindowViewModel();
        vm.InputPlayerId = "99999";
        vm.LoginPlayerCommand.Execute(null);
        Assert.Equal("Nie znaleziono gracza o takim ID!", vm.ErrorMessage);
    }
}

public class HistoryTests
{
    [Fact]
    public void HlHistory_RecordedAfterFullGame()
    {
        var vm = new MainWindowViewModel();
        vm.InputPlayerId = "77777";
        vm.RegisterPlayerCommand.Execute(null);
        vm.InputPlayerId = "77777";
        vm.LoginPlayerCommand.Execute(null);
        vm.StartGameCommand.Execute(null);
        for (int i = 0; i < 51; i++)
            vm.GuessHigherCommand.Execute(null);

        vm.GoToHistoryCommand.Execute(null);
        Assert.Single(vm.CurrentHlResults);
        Assert.Matches(@"^\d+/\d+$", vm.CurrentHlResults[0]);
    }
}
