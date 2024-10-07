namespace Domain.Entities
{
    public enum ProductType
    {
        General,
        HouseholdChemical,
        Electronic,
        Grocerie
    }

    public class ProductEntity
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public double Weight { get; set; }
        public ProductType ProductType { get; set; }
        public DateTime CreatedDate { get; set; }
        public int WarehouseId { get; set; }

    }
}
