namespace BlImplementation;

using BlApi;
internal class Bl : IBl
{
    /// <summary>
    /// Gets the courier service used to deliver messages or packages.
    /// </summary>
    public ICourier Courier { get; } = new CourierImplementation();

    /// <summary>
    /// Gets the current order associated with this instance.
    /// </summary>
    public IOrder Order { get; } = new OrderImplementation();

    /// <summary>
    /// Gets the administrative interface for managing advanced system operations.
    /// </summary>
    /// <remarks>Use this property to access administrative features such as user management, configuration,
    /// or system-level actions. The returned interface provides methods intended for users with elevated privileges.
    /// Thread safety and available operations depend on the specific implementation of the administrative
    /// interface.</remarks>
    public IAdmin Admin { get; } = new AdminImplementation();

}
