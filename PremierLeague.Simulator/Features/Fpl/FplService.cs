using System.Text.Json;

namespace PremierLeague.Simulator.Features.Fpl;

public class FplService(HttpClient httpClient)
{
    public async Task<Dictionary<string, FplTeam>> GetTeamsAsync()
    {
        var data = await GetDataAsync("https://fantasy.premierleague.com/api/bootstrap-static/");
        var jsonDoc = JsonDocument.Parse(data);
        var teams = jsonDoc.RootElement.GetProperty("teams");

        var result = new Dictionary<string, FplTeam>();
        foreach (var team in teams.EnumerateArray())
        {
            var teamName = team.GetProperty("name").GetString();
            var id = team.GetProperty("id").GetInt32();

            result[teamName] = new FplTeam
            {
                Id = id,
                Name = team.GetProperty("name").GetString() ?? string.Empty,
                Squad = await GetSquadAsync(id) 
            };
        }

        return result;
    }
    
    private async Task<string> GetDataAsync(string endpoint)
    {
        var response = await httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    
    private async Task<List<FplPlayer>> GetSquadAsync(int teamId)
    {
        var data = await GetDataAsync("https://fantasy.premierleague.com/api/bootstrap-static/");
        var jsonDoc = JsonDocument.Parse(data);
        var players = jsonDoc.RootElement.GetProperty("elements");

        var result = new List<FplPlayer>();
        
        foreach (var player in players.EnumerateArray())
        {
            if (player.GetProperty("team").GetInt32() == teamId)
            {
                result.Add(new FplPlayer
                {
                    Id = player.GetProperty("id").GetInt32(),
                    FirstName = player.GetProperty("first_name").GetString() ?? string.Empty,
                    LastName = player.GetProperty("second_name").GetString() ?? string.Empty,
                });
            }
        }

        return result;
    }
}