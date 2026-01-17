namespace BuyTime_Domain.Entities;

public class Specialization
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public ICollection<User> Experts { get; set; }
}