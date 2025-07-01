using System.Linq;
using System.Linq.Expressions;
using Cenima_ETickets.Data;
using Cenima_ETickets.Models;
using Cenima_ETickets.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Cenima_ETickets.Repositories
{
    public class CinemaRepository : Repository<Cenima>, ICinemaRepository
    {

        public CinemaRepository(ApplicationDbContext context) : base(context)
        {            
        }
    }
}
