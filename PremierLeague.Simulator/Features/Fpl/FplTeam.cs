public class FplTeam
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<FplPlayer> Squad { get; set; }
}