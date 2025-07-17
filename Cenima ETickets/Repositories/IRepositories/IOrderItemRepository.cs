namespace Cinema_ETickets.Repositories.IRepositories
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task<bool> CreateRangeAsync(List<OrderItem> entities);

    }
}
