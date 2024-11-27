namespace BuyTime_Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; } 
    public string Email { get; set; }
    
    public ICollection<Feedback>? Feedbacks { get; set; }
    // public Teacher Teacher { get; set; }
}