using Domain.DTO.Requests;
using Domain.Entities;
using Domain.Repository.Interfaces;
using Domain.Services.Interfaces;

namespace Domain.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public long Add(AddProductModel addProductRequestDTO)
        {
            return _productRepository.Add(new ProductEntity()
            {
                Name = addProductRequestDTO.Name,
                Price = addProductRequestDTO.Price,
                Weight = addProductRequestDTO.Weight,
                ProductType = addProductRequestDTO.ProductType,
                CreatedDate = addProductRequestDTO.CreatedDate,
                WarehouseId = addProductRequestDTO.WarehouseId
            });
        }

        public ProductEntity GetById(GetProductModel getProductRequestDTO)
        {
            return _productRepository.GetById(getProductRequestDTO.Id);
        }

        public List<ProductEntity> GetProductsByFilter(GetProductsWithFiltersModel productsWithFiltersRequestDTO)
        {           
            return _productRepository.GetProductsByFilter(new FilterEntity()
            {
                ProductType = productsWithFiltersRequestDTO.ProductType,
                DateTime = productsWithFiltersRequestDTO.DateTime,
                WarehouseId = productsWithFiltersRequestDTO.WarehouseId,
                PageNumber = productsWithFiltersRequestDTO.PageNumber,
                PageSize = productsWithFiltersRequestDTO.PageSize
            });
        }

        public string UpdatePrice(UpdateProductPriceModel updateProductPriceRequestDTO)
        {
            return _productRepository.UpdatePrice(updateProductPriceRequestDTO.Id, updateProductPriceRequestDTO.Price);
        }

    }
}
