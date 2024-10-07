using Domain.Repository;
using DTO.Entities;

namespace Infrastructure
{
    public class SalesRepository : ISalesRepository
    {
        private List<ProductEntity> productEntities;

        public List<ProductEntity> GetData()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory; 
            string filePath = Path.Combine(basePath, @"..\..\..\..\Infrastructure\Test.txt"); 
            string fullFilePath = Path.GetFullPath(filePath); 

            if (productEntities != null)
                return productEntities;

            var productEntityCollection = new List<ProductEntity>();

            using (StreamReader sr = new StreamReader(fullFilePath))
            {
                foreach (var line in File.ReadLines(fullFilePath))
                {
                    var productStringFormat = line.Split(',');

                    if (productStringFormat[0] == "id")
                        continue;

                    var noteSale = CreateNoteSale(productStringFormat);

                    productEntityCollection.Add(noteSale);
                }
            }
            productEntities = productEntityCollection;

            return productEntities;
        }

        private ProductEntity CreateNoteSale(string[] noteSaleStringFormat)
        {
            try
            {
                return new ProductEntity
                {
                    Id = StringToIdConverter(noteSaleStringFormat[0]),
                    Date = StringToDateOnlyConverter(noteSaleStringFormat[1]),
                    Sales = StringToSalesConverter(noteSaleStringFormat[2]),
                    Stock = StringToStockConverter(noteSaleStringFormat[3])
                };
            }
            catch
            {
                throw new Exception("Ошибка содержимого базы данных");
            }

        }

        #region Converters

        private int StringToIdConverter(string note)
        {
            if (!int.TryParse(note, out var id) || id < 0)
                throw new Exception($"Ошибка в базе данных : Id неврного формата : {note}");

            return id;
        }

        private DateOnly StringToDateOnlyConverter(string note)
        {
            if (!DateOnly.TryParseExact(note, "yyyy-MM-dd", out var date))
                throw new Exception($"Ошибка в базе данных : Дата неверного формата : {note}");

            return date;
        }

        private int StringToSalesConverter(string note)
        {
            if (!int.TryParse(note, out var sales) || sales < 0)
                throw new Exception($"Ошибка в базе данных : Stock неврного формата : {note}");

            return sales;
        }

        private int StringToStockConverter(string note)
        {
            if (!int.TryParse(note, out var stock) || stock < 0)
                throw new Exception($"Ошибка в базе данных : Sales неврного формата : {note}");

            return stock;
        }

        #endregion
    }
}
