using CsvHelper;
using Domain.DAL.Models;
using System.Runtime.CompilerServices;

namespace Domain.DAL.Repository.Interfaces
{
    public interface IProductRepository
    {
        public void SetStreamWriter(StreamWriter streamWriter);
        public Task WriteResultAsync(ResultEntity result, CancellationToken cancellationToken);
        public IAsyncEnumerable<ProductEntity> ReadProductsAsync(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken);
        public StreamWriter OpenOutputFile(string filePath);
    }
}
