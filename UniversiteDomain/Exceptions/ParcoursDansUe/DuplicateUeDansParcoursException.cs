namespace UniversiteDomain.Exceptions.ParcoursDansUe;

[Serializable]
public class DuplicateParcoursDansUeException : Exception
{
    public DuplicateParcoursDansUeException() : base() { }
    public DuplicateParcoursDansUeException(string message) : base(message) { }
    public DuplicateParcoursDansUeException(string message, Exception inner) : base(message, inner) { }
}