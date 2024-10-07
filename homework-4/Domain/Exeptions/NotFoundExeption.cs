namespace Domain.Exeptions
{
    public class NotFoundExeption : Exception
    {
        public NotFoundExeption(string message = "Товар с данным id не найден")
        : base(message) { }
    }
}
