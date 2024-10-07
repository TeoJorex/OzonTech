using DTO.Domain;

namespace Domain.Services.IServices
{
    public interface ISaleService
    {
        public ProductCount ADSCalculate(int productId);
        public ProductCount DemandCalculate(int productId, int NumberOfdays);
        public ProductCount PredictionCalculate(int productId, int NumberOfdays);  
    }
}
