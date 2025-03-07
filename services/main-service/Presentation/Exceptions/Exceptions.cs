public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string message) : base(message) { }
}

public class DatabaseException : Exception
{
    public DatabaseException(string message) : base(message) { }
}



