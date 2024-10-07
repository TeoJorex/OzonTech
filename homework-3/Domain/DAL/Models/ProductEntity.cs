using CsvHelper.Configuration.Attributes;

namespace Domain.DAL.Models
{
    public class ProductEntity
    {
        [Name("Id")]
        public int Id { get; set; }

        [Name("Prediction")]
        public int Prediction { get; set; }

        [Name("Stock")]
        public int Stock { get; set; }
    }
}
