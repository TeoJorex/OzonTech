using CsvHelper.Configuration.Attributes;

namespace Domain.DAL.Models
{
    public class ResultEntity
    {
        [Name("Id")]
        public int Id { get; set; }

        [Name("Demand")]
        public int Demand { get; set; }
    }
}
