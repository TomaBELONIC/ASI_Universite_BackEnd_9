namespace UniversiteDomain.Exceptions.NoteAEtudiantExceptions;

[Serializable]
public class DuplicateNotePourUePourEtudiantException : Exception
{
    public DuplicateNotePourUePourEtudiantException() : base() { }
    public DuplicateNotePourUePourEtudiantException(string message) : base(message) { }
    public DuplicateNotePourUePourEtudiantException(string message, Exception inner) : base(message, inner) { }
}