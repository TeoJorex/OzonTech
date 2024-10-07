using DTO.Entities;

namespace Domain.Repository
{
    public interface ISalesRepository
    {
        public List<ProductEntity> GetData();
    }
}
