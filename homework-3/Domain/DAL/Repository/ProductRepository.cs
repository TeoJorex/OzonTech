using CsvHelper;
using Domain.DAL.Models;
using Domain.DAL.Repository.Interfaces;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Domain.DAL.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly SemaphoreSlim _fileWriteSemaphore = new SemaphoreSlim(1, 1);
        private StreamWriter _streamWriter;

        public void SetStreamWriter(StreamWriter streamWriter)
        {
            _streamWriter = streamWriter;
        }

        public async Task WriteResultAsync(ResultEntity result, CancellationToken cancellationToken)
        {
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                using (var csvWriter = new CsvWriter(stringWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteRecord(result);
                    csvWriter.NextRecord();
                }
            }

            await _fileWriteSemaphore.WaitAsync(cancellationToken);
            try
            {
                await _streamWriter.WriteAsync(stringBuilder.ToString());
            }
            finally
            {
                _fileWriteSemaphore.Release();
            }
        }

        public async IAsyncEnumerable<ProductEntity> ReadProductsAsync(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var product = csv.GetRecord<ProductEntity>();
                yield return product;
            }
        }

        public StreamWriter OpenOutputFile(string filePath)
        {
            return new StreamWriter(filePath);
        }
    }
}

