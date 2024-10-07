public class ValidateException : ArgumentException
{
    public ValidateException(string message = "Неверный формат входных данных")
    : base(message)
    { }
}