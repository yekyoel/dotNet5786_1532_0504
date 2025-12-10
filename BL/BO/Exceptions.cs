namespace BO;

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

[Serializable] // Indicate that the class can be serialized
/// Exception thrown when a requested data access layer (DAL) entity already exists.
public class DalXMLFileLoadCreateException : Exception
{
    public DalXMLFileLoadCreateException(string? message) : base(message) { }
}

[Serializable] // Indicate that the class can be serialized
/// Exception thrown when a requested business logic operation is temporarily unavailable.
public class BLTemporaryNotAvailableException : Exception
{
    public BLTemporaryNotAvailableException(string? message) : base(message) { }
}

[Serializable] // Indicate that the class can be serialized
public class BLDoesNotExistException : Exception
{
    public BLDoesNotExistException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown by BL when an order is invalid for creation.
/// </summary>
[Serializable]
public class BLInvalidOrderException : Exception
{
    public BLInvalidOrderException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown by BL when an order cannot be deleted due to active deliveries or other constraints.
/// </summary>
[Serializable]
public class BLCannotDeleteOrderException : Exception
{
    public BLCannotDeleteOrderException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown by BL when a required object or value is null.
/// Use this for BL-level null/argument checks so callers receive a consistent error type.
/// </summary>
[Serializable]
public class BLNullReferenceException : Exception
{
    public BLNullReferenceException(string? message) : base(message) { }
}