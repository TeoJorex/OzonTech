using Domain.BLL.Services.Interfaces;
using Domain.DAL.Models;
using Domain.DAL.Repository;
using Domain.DAL.Repository.Interfaces;
using Domain.DTO.Responses;

namespace Domain.BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private int _degreeOfParallelism;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private long _linesRead = 0;
        private long _productsProcessed = 0;
        private long _resultsWritten = 0;

        private readonly object _lockObject = new object();
        private StreamWriter _streamWriter;

        public event Action<long, long, long> ProgressChanged;

        public ProductService(int degreeOfParallelism)
        {
            _repository = new ProductRepository();
            _degreeOfParallelism = degreeOfParallelism;
            _semaphore = new SemaphoreSlim(_degreeOfParallelism);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void UpdateDegreeOfParallelism(int newDegree)
        {
            lock (_lockObject)
            {
                if (newDegree <= 0)
                {
                    return;
                }

                int difference = newDegree - _degreeOfParallelism;

                if (difference > 0)
                {
                    _semaphore.Release(difference);
                }
                else if (difference < 0)
                {
                    for (int i = 0; i < -difference; i++)
                    {
                        _semaphore.Wait();
                    }
                }
                _degreeOfParallelism = newDegree;
            }
        }

        public void CancelProcessing()
        {
            _cancellationTokenSource.Cancel();
        }

        public async Task<ProcessFileResponse> ProcessFileAsync(string inputFilePath, string outputFilePath)
        {
            var cancellationToken = _cancellationTokenSource.Token;
            var response = new ProcessFileResponse();

            _streamWriter = new StreamWriter(outputFilePath);
            _repository.SetStreamWriter(_streamWriter);

            await _streamWriter.WriteLineAsync("Id,Demand");

            var tasks = new List<Task>();

            try
            {
                var products = _repository.ReadProductsAsync(inputFilePath, cancellationToken);

                await foreach (var productEntity in products.WithCancellation(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Interlocked.Increment(ref _linesRead);

                    await _semaphore.WaitAsync(cancellationToken);

                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            Interlocked.Increment(ref _productsProcessed);

                            SimulateComplexCalculation(cancellationToken);

                            var demand = CalculateDemand(productEntity);

                            var result = new ResultEntity
                            {
                                Id = productEntity.Id,
                                Demand = demand
                            };

                            await _repository.WriteResultAsync(result, cancellationToken);
                            Interlocked.Increment(ref _resultsWritten);

                            ProgressChanged?.Invoke(_linesRead, _productsProcessed, _resultsWritten);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при обработке продукта {productEntity.Id}: {ex.Message}");
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }, cancellationToken);

                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                response.Success = true;
                response.Message = "Обработка завершена успешно";
                response.LinesRead = _linesRead;
                response.ProductsProcessed = _productsProcessed;
                response.ResultsWritten = _resultsWritten;
            }
            catch (OperationCanceledException)
            {
                response.Success = false;
                response.Message = "Обработка была отменена";
                response.LinesRead = _linesRead;
                response.ProductsProcessed = _productsProcessed;
                response.ResultsWritten = _resultsWritten;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка при обработке файла: {ex.Message}");
                response.Success = false;
                response.Message = $"Ошибка: {ex.Message}";
            }
            finally
            {
                await _streamWriter.FlushAsync();
                _streamWriter.Close();
            }

            return response;
        }

        private int CalculateDemand(ProductEntity product)
        {
            return Math.Max(0, product.Prediction - product.Stock);
        }

        private void SimulateComplexCalculation(CancellationToken cancellationToken)
        {
            //Thread.Sleep(10000);
            for (int i = 0; i < 1000000; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                double result = Math.Sqrt(i);
            }
        }
    }
}
