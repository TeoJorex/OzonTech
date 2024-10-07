namespace Domain.Entities
{
    public class FilterEntity
    {
        public ProductType? ProductType { get; set; }
        public DateTime? DateTime { get; set; }
        public int? WarehouseId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
