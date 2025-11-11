
namespace DO;

/// <summary>
/// Exception thrown when a requested data access layer (DAL) entity does not exist or it alr exists.
/// </summary>

[Serializable] // Indicate that the class can be serialized

/// Exception thrown when a requested data access layer (DAL) entity does not exist.
public class DalDoesNotExistException : Exception
{
    public DalDoesNotExistException(string? message) : base(message) { }
}

[Serializable] // Indicate that the class can be serialized
/// Exception thrown when a requested data access layer (DAL) entity already exists.
public class DalAlreadyExistExceptions : Exception
{
    public DalAlreadyExistExceptions(string? message) : base(message) { }
}

/// Exception thrown when a requested data access layer (DAL) entity is unchangeable when the vlaue is a "running vlaue".
public class DalIsUnchangeableExceptions : Exception
{
    public DalIsUnchangeableExceptions(string? message) : base(message) { }
}

/// Exception thrown when a requested data access layer (DAL) entity is null but it shouldn't be.
public class DalCanNotBeNullException : Exception
{
    public DalCanNotBeNullException(string? message) : base(message) { }
}

