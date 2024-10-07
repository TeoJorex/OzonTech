using Domain.Entities;

namespace Domain.Repository.Interfaces
{
    public interface IProductRepository
    {
        long Add(ProductEntity productDTO);
        ProductEntity GetById(long id);
        string UpdatePrice(long id, double newPrice);
        List<ProductEntity> GetProductsByFilter(FilterEntity filterEntity);

    }
}
