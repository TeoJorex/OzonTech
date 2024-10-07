using Domain.BLL.Services;
using Domain.DTO.Requests;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine($"Добро пожаловать в программу обработки товаров! {Environment.NewLine}" +
             $"Число потоков по умолчанию 4 {Environment.NewLine}" +
             $"Введите команду set [Новое число потоков] - чтобы изменить число потоков {Environment.NewLine}" +
             $"Введите команду cancel - чтобы отменить выполнение программы {Environment.NewLine}" +
             $"Удачи в использовании приложения {Environment.NewLine}");

        var request = new ProcessFileRequest
        {
            InputFilePath = "inputExel.csv",
            OutputFilePath = "outputExel.csv",
            DegreeOfParallelism = 4
        };

        var productService = new ProductService(request.DegreeOfParallelism);

        productService.ProgressChanged += (linesRead, productsProcessed, resultsWritten) =>
        {
            Console.WriteLine($"Прочитано строк: {linesRead}, Обработано товаров: {productsProcessed}, Записано результатов: {resultsWritten}");
        };

        var processingTask = productService.ProcessFileAsync(request.InputFilePath, request.OutputFilePath);

        var inputTask = Task.Run(() =>
        {
            while (!processingTask.IsCompleted)
            {
                var input = Console.ReadLine();

                if (input.Equals("cancel", StringComparison.OrdinalIgnoreCase))
                {
                    productService.CancelProcessing();
                    break;
                }
                else if (input.StartsWith("set", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = input.Split(' ');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int newDegree))
                    {
                        if (newDegree < 0)
                            Console.WriteLine("Количество потоков должно быть больше 0");
                        if(newDegree > 100)
                            Console.WriteLine("Слишком большое количество потоков, число должно быть меньше 100");

                        productService.UpdateDegreeOfParallelism(newDegree);
                        Console.WriteLine($"Степень параллелизма обновлена до {newDegree}");
                    }
                    else
                    {
                        Console.WriteLine("Неверная команда. Используйте 'set [число]'.");
                    }
                }
                else 
                {
                    Console.WriteLine("Неизвестная команда");
                }
            }
        });

        try
        {
            var response = await processingTask;
            Console.WriteLine(response.Message);
            Console.WriteLine($"Итог: Прочитано строк: {response.LinesRead}, Обработано товаров: {response.ProductsProcessed}, Записано результатов: {response.ResultsWritten}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
    }
}