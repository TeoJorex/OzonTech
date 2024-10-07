using Domain.Entities;
using Domain.Exeptions;
using Domain.Repository.Interfaces;

namespace Domain.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly Dictionary<long, ProductEntity> _products = new();
        private readonly object _lock = new();
        private long _guid = 1;

        public long Add(ProductEntity product)
        {
            lock (_lock)
            {
                var index = _guid;
                _guid++;
                _products.Add(index, product);

                return index;
            }
        }

        public ProductEntity GetById(long id)
        {
            if (!_products.ContainsKey(id))
                throw new NotFoundExeption();

            return _products[id];
        }

        public List<ProductEntity> GetProductsByFilter(FilterEntity filterEntity)
        {
            var query = _products.Values.AsQueryable();

            if (filterEntity.DateTime.HasValue)
                query = query.Where(p => p.CreatedDate == filterEntity.DateTime);

            if (filterEntity.ProductType.HasValue)
                query = query.Where(p => p.ProductType == filterEntity.ProductType);

            if (filterEntity.WarehouseId.HasValue)
                query = query.Where(p => p.WarehouseId == filterEntity.WarehouseId);

            return query
                .Skip((filterEntity.PageNumber - 1) * filterEntity.PageSize)
                .Take(filterEntity.PageSize)
                .ToList();
        }

        public string UpdatePrice(long id,double newPrice)
        {
            lock (_lock)
            {
                if (!_products.ContainsKey(id))
                    throw new NotFoundExeption();

                _products[id].Price = newPrice;
                return "Успешно";
            }
        }
    }
}
