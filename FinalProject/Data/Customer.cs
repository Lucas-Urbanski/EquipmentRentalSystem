namespace FinalProject.Data;

public class Customer
{
    public int CustomerID { get; set; }
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool Banned { get; set; }

    public Customer() { }

    public async Task<int> AddCustomerAsync(Broker broker)
    {
        ArgumentNullException.ThrowIfNull(broker);
        var newId = await broker.InsertCustomerAsync(this);
        if (newId > 0) CustomerID = newId;
        return newId;
    }

    public async Task<bool> UpdateCustomerAsync(Broker broker)
    {
        ArgumentNullException.ThrowIfNull(broker);
        if (CustomerID == 0) throw new InvalidOperationException("Cannot update customer with ID = 0");
        return await broker.UpdateCustomerAsync(this);
    }

    public async Task<bool> RemoveCustomerAsync(Broker broker)
    {
        ArgumentNullException.ThrowIfNull(broker);
        if (CustomerID == 0) throw new InvalidOperationException("Cannot delete customer with ID = 0");
        return await broker.DeleteCustomerAsync(CustomerID);
    }

    public static async Task<Customer?> SearchCustomerByIdAsync(Broker broker, int id)
    {
        ArgumentNullException.ThrowIfNull(broker);
        return await broker.LoadCustomerByIdAsync(id);
    }
}
