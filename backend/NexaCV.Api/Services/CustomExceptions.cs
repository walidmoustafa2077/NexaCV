namespace NexaCV.Api.Services;

public class TooManyRegenerationsException : Exception
{
    public TooManyRegenerationsException()
        : base("Regeneration limit reached for this section.") { }

    public TooManyRegenerationsException(string message)
        : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
