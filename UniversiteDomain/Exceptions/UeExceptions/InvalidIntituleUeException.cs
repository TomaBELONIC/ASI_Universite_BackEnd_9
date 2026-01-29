namespace UniversiteDomain.Exceptions.UeExceptions;

[Serializable]
public class InvalidIntituleUeException : Exception
{
    public InvalidIntituleUeException() : base() { }
    public InvalidIntituleUeException(string message) : base(message) { }
    public InvalidIntituleUeException(string message, Exception inner) : base(message, inner) { }
}