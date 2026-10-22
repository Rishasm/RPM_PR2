using HotelAPI.Models;

namespace HotelAPI.Repositories;

public interface IProductReservationRepository
{
    Task<List<ProductReservation>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<int> CountAsync(CancellationToken ct);
    Task<ProductReservation?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(ProductReservation entity, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
