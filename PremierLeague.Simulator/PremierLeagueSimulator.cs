using System.Collections.Concurrent;
using PremierLeague.Simulator.Features.Fpl;

namespace PremierLeague.Simulator;

public class PremierLeagueSimulator(FplService fplService)
{
    private static readonly Dictionary<string, List<FplPlayer>> TeamSquads = new();

    private readonly ConcurrentDictionary<string, (int Points, int GoalsFor, int GoalsAgainst, int Wins, int Draws, int Losses)> _teamStats = new();
    private readonly ConcurrentDictionary<string, int> _playerGoals = new();
    private readonly ConcurrentDictionary<string, int> _playerYellowCards = new();
    private readonly ConcurrentDictionary<string, int> _playerRedCards = new();
    private readonly Random _random = new();

    public async Task PrepareSeason()
    {
        await LoadTeamSquads();

        foreach (var team in TeamSquads.Keys)
        {
            _teamStats[team] = (0, 0, 0, 0, 0, 0);
            foreach (var player in TeamSquads[team])
            {
                _playerGoals[player.FullName] = 0;
                _playerYellowCards[player.FullName] = 0;
                _playerRedCards[player.FullName] = 0;
            }
        }
    }

    private async Task LoadTeamSquads()
    {
        var teams = await fplService.GetTeamsAsync();
        foreach (var (id, team) in teams)
        {
            TeamSquads[team.Name] = team.Squad;
        }
    }

    public void SimulateSeason()
    {
        Console.WriteLine("Starting Premier League Season Simulation...\n");
        for (int week = 1; week <= 38; week++)
        {
            Console.WriteLine($"Matchweek {week} kicked off!\n");
            SimulateMatchWeek();
            Console.WriteLine($"Matchweek {week} completed!\n");
            DisplayLeagueTable();
            Console.WriteLine("Press Enter to continue to the next matchweek...");
            Console.ReadLine();
        }
        DisplayTopPlayers();
    }

    private void SimulateMatchWeek()
    {
        var fixtures = GenerateMatchWeekFixtures();
        Parallel.ForEach(fixtures, fixture =>
        {
            SimulateMatch(fixture.Home, fixture.Away);
        });
    }

    private List<(string Home, string Away)> GenerateMatchWeekFixtures()
    {
        var fixtures = new List<(string, string)>();
        var shuffledTeams = TeamSquads.Keys.OrderBy(_ => _random.Next()).ToList();
        for (int i = 0; i < shuffledTeams.Count; i += 2)
        {
            fixtures.Add((shuffledTeams[i], shuffledTeams[i + 1]));
        }
        return fixtures;
    }

    private void SimulateMatch(string homeTeam, string awayTeam)
    {
        int homeGoals = 0, awayGoals = 0;
        var homeLineup = GetStartingLineup(homeTeam);
        var awayLineup = GetStartingLineup(awayTeam);

        Console.WriteLine($"{homeTeam} vs {awayTeam} has kicked off!");

        for (int minute = 0; minute <= 90; minute++)
        {
            Thread.Sleep(100);

            if (_random.Next(100) < 3)
            {
                if (_random.Next(2) == 0)
                {
                    homeGoals++;
                    var scorer = homeLineup[_random.Next(homeLineup.Count)];
                    _playerGoals[scorer]++;
                    Console.WriteLine($"{minute}' GOAL! {homeTeam}: {scorer} scores!");
                }
                else
                {
                    awayGoals++;
                    var scorer = awayLineup[_random.Next(awayLineup.Count)];
                    _playerGoals[scorer]++;
                    Console.WriteLine($"{minute}' GOAL! {awayTeam}: {scorer} scores!");
                }
            }

            if (_random.Next(100) < 2)
            {
                if (_random.Next(2) == 0)
                {
                    var booked = homeLineup[_random.Next(homeLineup.Count)];
                    _playerYellowCards[booked]++;
                    Console.WriteLine($"{minute}' YELLOW CARD! {homeTeam}: {booked}");
                }
                else
                {
                    var booked = awayLineup[_random.Next(awayLineup.Count)];
                    _playerYellowCards[booked]++;
                    Console.WriteLine($"{minute}' YELLOW CARD! {awayTeam}: {booked}");
                }
            }

            if (_random.Next(200) < 1)
            {
                if (_random.Next(2) == 0)
                {
                    var sentOff = homeLineup[_random.Next(homeLineup.Count)];
                    _playerRedCards[sentOff]++;
                    Console.WriteLine($"{minute}' RED CARD! {homeTeam}: {sentOff}");
                }
                else
                {
                    var sentOff = awayLineup[_random.Next(awayLineup.Count)];
                    _playerRedCards[sentOff]++;
                    Console.WriteLine($"{minute}' RED CARD! {awayTeam}: {sentOff}");
                }
            }
        }

        UpdateTeamStats(homeTeam, awayTeam, homeGoals, awayGoals);
        Console.WriteLine($"FULL-TIME: {homeTeam} {homeGoals}-{awayGoals} {awayTeam}\n");
    }

    private List<string> GetStartingLineup(string team)
    {
        return TeamSquads[team].OrderBy(_ => _random.Next())
            .Take(11)
            .Select(p => p.FullName)
            .ToList();
    }

    private void UpdateTeamStats(string home, string away, int homeGoals, int awayGoals)
    {
        var homeStats = _teamStats[home];
        var awayStats = _teamStats[away];

        homeStats.GoalsFor += homeGoals;
        homeStats.GoalsAgainst += awayGoals;
        awayStats.GoalsFor += awayGoals;
        awayStats.GoalsAgainst += homeGoals;

        if (homeGoals > awayGoals)
        {
            homeStats.Points += 3;
            homeStats.Wins++;
            awayStats.Losses++;
        }
        else if (homeGoals < awayGoals)
        {
            awayStats.Points += 3;
            awayStats.Wins++;
            homeStats.Losses++;
        }
        else
        {
            homeStats.Points += 1;
            awayStats.Points += 1;
            homeStats.Draws++;
            awayStats.Draws++;
        }

        _teamStats[home] = homeStats;
        _teamStats[away] = awayStats;
    }

    private void DisplayLeagueTable()
    {
        Console.WriteLine("\nPremier League Table:");
        var sortedTeams = _teamStats.OrderByDescending(t => t.Value.Points).ThenByDescending(t => t.Value.GoalsFor - t.Value.GoalsAgainst);
        Console.WriteLine("Team\t\tPoints\tWins\tDraws\tLosses\tGD\tGF\tGA");
        foreach (var (team, stats) in sortedTeams)
        {
            int goalDifference = stats.GoalsFor - stats.GoalsAgainst;
            Console.WriteLine($"{team,-15} {stats.Points,6} {stats.Wins,6} {stats.Draws,6} {stats.Losses,7} {goalDifference,4} {stats.GoalsFor,4} {stats.GoalsAgainst,4}");
        }
    }

    private void DisplayTopPlayers()
    {
        Console.WriteLine("\nTop Scorers:");
        foreach (var player in _playerGoals.OrderByDescending(p => p.Value).Take(10))
        {
            Console.WriteLine($"{player.Key}: {player.Value} goals");
        }

        Console.WriteLine("\nMost Yellow Cards:");
        foreach (var player in _playerYellowCards.OrderByDescending(p => p.Value).Take(10))
        {
            Console.WriteLine($"{player.Key}: {player.Value} yellow cards");
        }

        Console.WriteLine("\nMost Red Cards:");
        foreach (var player in _playerRedCards.OrderByDescending(p => p.Value).Take(10))
        {
            Console.WriteLine($"{player.Key}: {player.Value} red cards");
        }
    }

    private class Player
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}