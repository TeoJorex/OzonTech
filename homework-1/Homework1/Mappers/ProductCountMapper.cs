using DTO.Domain;
using DTO.Models;

namespace DTO.Mappers
{
    public static class ProductCountMapper
    {
        public static ProductCountModel ToModel(this ProductCount domain)
        {
            return new ProductCountModel
            {
                Quantity = domain.Quantity
            };
        }
    }
}
