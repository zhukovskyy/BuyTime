using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;

namespace BuyTime_Infrastructure.Repositories;

public class TeacherRepository(BuyTimeDbContext context) 
    : Repository<Teacher>(context), ITeacherRepository
{
    
}