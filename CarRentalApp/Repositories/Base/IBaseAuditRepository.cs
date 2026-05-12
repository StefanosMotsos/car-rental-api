namespace CarRentalApp.Repositories.Base
{
    public interface IBaseAuditRepository<T> : IBaseRepository<T>
    {
        Task<bool> SoftDeleteAsync(int id);

        Task<T?> GetByUuidAsync(Guid uuid);
    }
}
