namespace WebYTE.Application.DTOs.Admin;

public class UserStatisticsDto
{
    public int TotalUsers { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public int TotalStaff { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
}
