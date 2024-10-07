using Domain.Entities;

namespace Homework2.Controllers.DTO.Responses
{
    public class GetProductsWithFiltersResponse
    {
        public List<GetProductsWithFiltersModel> getProductsWithFiltersModels = new List<GetProductsWithFiltersModel>();
    }

    public class GetProductsWithFiltersModel
    {
        public string Name { get; set; }

        public double Price { get; set; }

        public double Weight { get; set; }

        public ProductType ProductType { get; set; }

        public DateTime CreatedDate { get; set; }

        public int WarehouseId { get; set; }
    }
}
