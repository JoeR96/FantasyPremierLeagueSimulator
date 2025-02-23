using System.Collections.Concurrent;

namespace PremierLeague.Simulator;

public class PremierLeagueSimulator
{
    private static readonly Dictionary<string, List<string>> TeamSquads = new()
    {
        ["Arsenal"] = new() { "Ramsdale", "White", "Saliba", "Gabriel", "Zinchenko", "Partey", "Odegaard", "Saka", "Martinelli", "Jesus", "Trossard" },
        ["Aston Villa"] = new() { "Martinez", "Cash", "Konsa", "Mings", "Digne", "Luiz", "McGinn", "Bailey", "Buendia", "Watkins", "Coutinho" },
        ["Bournemouth"] = new() { "Travers", "Smith", "Mepham", "Kelly", "Zemura", "Lerma", "Cook", "Billing", "Christie", "Solanke", "Brooks" },
        ["Brentford"] = new() { "Raya", "Jansson", "Pinnock", "Ajer", "Henry", "Norgaard", "Jensen", "Canos", "Mbeumo", "Toney", "Wissa" },
        ["Brighton"] = new() { "Sanchez", "Veltman", "Dunk", "Webster", "Estupinan", "Caicedo", "Mac Allister", "March", "Mitoma", "Trossard", "Welbeck" },
        ["Burnley"] = new() { "Muric", "Roberts", "Beyer", "Harwood-Bellis", "Maatsen", "Cullen", "Brownhill", "Gudmundsson", "Zaroury", "Rodriguez", "Barnes" },
        ["Chelsea"] = new() { "Kepa", "James", "Silva", "Badiashile", "Chilwell", "Kante", "Enzo", "Mount", "Sterling", "Havertz", "Felix" },
        ["Crystal Palace"] = new() { "Guaita", "Ward", "Andersen", "Guehi", "Mitchell", "Doucoure", "Eze", "Olise", "Ayew", "Zaha", "Edouard" },
        ["Everton"] = new() { "Pickford", "Coleman", "Tarkowski", "Coady", "Mykolenko", "Gueye", "Onana", "Iwobi", "McNeil", "Gray", "Calvert-Lewin" },
        ["Fulham"] = new() { "Leno", "Tete", "Adarabioyo", "Ream", "Robinson", "Palhinha", "Reed", "Pereira", "Wilson", "Mitrovic", "Willian" },
        ["Liverpool"] = new() { "Alisson", "Alexander-Arnold", "Van Dijk", "Konate", "Robertson", "Fabinho", "Thiago", "Henderson", "Salah", "Nunez", "Diaz" },
        ["Luton Town"] = new() { "Horvath", "Bree", "Lockyer", "Bradley", "Bell", "Campbell", "Clark", "Berry", "Morris", "Adebayo", "Cornick" },
        ["Man City"] = new() { "Ederson", "Walker", "Dias", "Laporte", "Cancelo", "Rodri", "De Bruyne", "Bernardo", "Mahrez", "Haaland", "Foden" },
        ["Man United"] = new() { "De Gea", "Dalot", "Varane", "Martinez", "Shaw", "Casemiro", "Fernandes", "Eriksen", "Sancho", "Rashford", "Weghorst" },
        ["Newcastle"] = new() { "Pope", "Trippier", "Botman", "Schar", "Burn", "Bruno", "Willock", "Longstaff", "Almiron", "Wilson", "Isak" },
        ["Nottingham Forest"] = new() { "Henderson", "Williams", "Worrall", "Niakhate", "Toffolo", "Yates", "Freuler", "Johnson", "Lingard", "Awoniyi", "Gibbs-White" },
        ["Sheffield United"] = new() { "Foderingham", "Bogle", "Egan", "Ahmedhodzic", "Lowe", "Norwood", "Berge", "Fleck", "Ndiaye", "McBurnie", "Sharp" },
        ["Tottenham"] = new() { "Lloris", "Emerson", "Romero", "Dier", "Perisic", "Hojbjerg", "Bentancur", "Kulusevski", "Richarlison", "Kane", "Son" },
        ["West Ham"] = new() { "Fabianski", "Coufal", "Zouma", "Aguerd", "Cresswell", "Rice", "Soucek", "Bowen", "Paqueta", "Benrahma", "Antonio" },
        ["Wolves"] = new() { "Sa", "Semedo", "Collins", "Kilman", "Bueno", "Neves", "Moutinho", "Nunes", "Podence", "Cunha", "Jimenez" }
    };

    private readonly ConcurrentDictionary<string, (int Points, int GoalsFor, int GoalsAgainst, int Wins, int Draws, int Losses)> _teamStats = new();
    private readonly ConcurrentDictionary<string, int> _playerGoals = new();
    private readonly ConcurrentDictionary<string, int> _playerYellowCards = new();
    private readonly ConcurrentDictionary<string, int> _playerRedCards = new();
    private readonly Random _random = new();

    public PremierLeagueSimulator()
    {
        foreach (var team in TeamSquads.Keys)
        {
            _teamStats[team] = (0, 0, 0, 0, 0, 0);
            foreach (var player in TeamSquads[team])
            {
                _playerGoals[player] = 0;
                _playerYellowCards[player] = 0;
                _playerRedCards[player] = 0;
            }
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
            Thread.Sleep(10);

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
        return TeamSquads[team].OrderBy(_ => _random.Next()).Take(11).ToList();
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
}