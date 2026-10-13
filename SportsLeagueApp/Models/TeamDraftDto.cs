public class TeamDraftDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? SportCategory { get; set; }
    public string? City { get; set; }
    public string? CoachName { get; set; }
    public int? FoundedYear { get; set; }
    public string? HomeStadium { get; set; }
    public int[] SelectedPlayerIds { get; set; } = Array.Empty<int>();
}