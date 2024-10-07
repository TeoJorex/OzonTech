using Domain.Entities;

namespace Domain.DTO.Requests
{
    public class AddProductModel
    {
        public string Name { get; set; }

        public double Price { get; set; }

        public double Weight { get; set; }

        public ProductType ProductType { get; set; }

        public DateTime CreatedDate { get; set; }

        public int WarehouseId { get; set; }
    }
}
