namespace FileSaverBot.Exceptions;

public class AccessDeniedException : Exception
{
    public AccessDeniedException(string? message)
     : base(message)
    { }
}
