using Domain.DTO.Requests;
using Domain.Entities;

namespace Domain.Services.Interfaces
{
    public interface IProductService
    {
        public long Add(AddProductModel addProductRequestDTO);
        public ProductEntity GetById(GetProductModel getProductRequestDTO);
        public List<ProductEntity> GetProductsByFilter(GetProductsWithFiltersModel productsWithFiltersRequestDTO);
        public string UpdatePrice(UpdateProductPriceModel updateProductPriceRequestDTO);
    }
}
