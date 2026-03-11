namespace InvestEasy.Models;

public class UserProgressModel
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public int CurrentLevel { get; set; } = 1;

    public bool Level1Completed { get; set; }
    public bool Level2Completed { get; set; }
    public bool Level3Completed { get; set; }
}