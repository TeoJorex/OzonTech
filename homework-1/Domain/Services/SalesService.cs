using Domain.Repository;
using Domain.Services.IServices;
using DTO.Domain;

namespace Sales.Services
{
    public class SalesService : ISaleService
    {
        private enum OperationName
        {
            ads,
            prediction,
            demand
        }

        private const int MaxNumberOfDayes = 30;

        private readonly ISalesRepository _salesRepository;

        public SalesService(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        private List<InfoDaySale> GetIdToSaleMap(int productId)
        {
            var productEntities = _salesRepository.GetData().Where(s => s.Id == productId);

            if (!productEntities.Any())
                throw new Exception($"Не найден товар с данным Id : {productId}");

            var salesNoteOfproduct = new List<InfoDaySale>();
            foreach (var entity in productEntities)
            {
                salesNoteOfproduct.Add(new InfoDaySale
                {
                    Date = entity.Date,
                    Sales = entity.Sales,
                    Stock = entity.Stock,
                });
            }
            return salesNoteOfproduct;
        }

        public ProductCount ADSCalculate(int productId)
        {
            var productSales = GetIdToSaleMap(productId);
            double totalSales = productSales.Sum(s => s.Sales);
            int daysInStock = productSales.Count;

            return new ProductCount { Quantity = totalSales / daysInStock };
        }

        public ProductCount PredictionCalculate(int productId, int numberOfdays)
        {
            if (numberOfdays <= 0)
                throw new Exception("Неправильно задано количество дней");

            if (numberOfdays > MaxNumberOfDayes)
                throw new Exception($"В запросе указано количество дней, превышающее количество записей о продаже {Environment.NewLine}" +
                    $"Максимально возможное число для запроса: {MaxNumberOfDayes}");

            double ADS = ADSCalculate(productId).Quantity;

            return new ProductCount { Quantity = ADS * numberOfdays };
        }

        public ProductCount DemandCalculate(int productId, int numberOfdays)
        {
            var stoks = GetIdToSaleMap(productId).OrderBy(s => s.Date).ToList();
            double totalStock = stoks.Last().Stock;

            if (numberOfdays > stoks.Count())
                throw new Exception("В базе данных не информации для обработки запроса");

            double salesPrediction = PredictionCalculate(productId, numberOfdays).Quantity;
            return new ProductCount { Quantity = salesPrediction - totalStock };
        }

        public string ChooceCommand(string command)
        {
            var commandKeyWords = command.Split(' ');
            var date = 0;
            if ((commandKeyWords.Count() != 2 && commandKeyWords.Count() != 3) ||
                !Enum.TryParse(commandKeyWords[0], out OperationName nameOfOperation) ||
                !int.TryParse(commandKeyWords[1], out int id) ||
                ((nameOfOperation == OperationName.demand || nameOfOperation == OperationName.prediction) && commandKeyWords.Count() == 2) ||
                (commandKeyWords.Count() == 3 && !int.TryParse(commandKeyWords[2], out date)))
                throw new ValidateException();

            switch (nameOfOperation)
            {
                case OperationName.ads:
                    return ADSCalculate(id).Quantity.ToString();

                case OperationName.prediction:                   
                    return PredictionCalculate(id, date).Quantity.ToString();

                case OperationName.demand:
                    return DemandCalculate(id, date).Quantity.ToString();

                default:
                    throw new Exception("Что-то пошло не так,обратитесь в службу поддержки");
            }
        }
    }
}
