namespace ApplicationServices.Exceptions
{
    public class DuplicateOrderException(string orderId) : Exception($"Order '{orderId}' already exists.");
}