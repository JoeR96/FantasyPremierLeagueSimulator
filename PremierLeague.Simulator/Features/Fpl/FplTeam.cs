namespace PremierLeague.Simulator.Features.Fpl;

public record FplTeam
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<FplPlayer> Squad { get; set; }
}