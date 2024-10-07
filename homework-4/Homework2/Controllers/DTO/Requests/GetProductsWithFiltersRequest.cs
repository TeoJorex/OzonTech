using Domain.Entities;

namespace Homework2.Controllers.DTO.Requests
{
    public class GetProductsWithFiltersRequest
    {
        public ProductType? ProductType {  get; set; }
        public DateTime? DateTime {  get; set; }
        public int? WarehouseId { get; set; }
        public int PageNumber {get; set; }
        public int PageSize { get; set; }
    }
}
