using Domain.Entities;

namespace Domain.DTO.Requests
{
    public class GetProductsWithFiltersModel
    {
        public ProductType? ProductType { get; set; }
        public DateTime? DateTime { get; set; }
        public int? WarehouseId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
