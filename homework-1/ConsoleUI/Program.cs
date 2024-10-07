using Infrastructure;
using Sales.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var sales = new SalesService(new SalesRepository());
        Ui(sales);
    }

    private static void Ui(SalesService sales)
    {
        Console.WriteLine($"Добро пожаловать в приложение для расчета среднедневных продаж, прогноза продаж и потребности к закупке! {Environment.NewLine}" +
        $"Введите команду в формате: <что_рассчитать> <ID товара> [количество_дней] {Environment.NewLine}" +
        $"Примеры команд: {Environment.NewLine}" +
        $"ads 123 {Environment.NewLine}" +
        $"prediction 456 45 {Environment.NewLine}" +
        $"demand 678 14 {Environment.NewLine}" +
        $"Остановить работу приложения вы можете при помощи команды : STOP{Environment.NewLine}" +
        "-------------------------------------------------------------");

        var command = Console.ReadLine();
        while (command != "STOP")
        {
            try
            {
                Console.WriteLine($"Результат расчета: {sales.ChooceCommand(command)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
            command = Console.ReadLine();
        }
        Console.WriteLine("Спасибо, что использовали данное приложение!");
    }
}