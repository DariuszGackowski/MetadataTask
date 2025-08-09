using FivetranClient.Utilities;

namespace FivetranClient.Models;

public class Group
{
    private string _id = string.Empty;
    private string _name = string.Empty;

    public string Id
    {
        get => _id;
        set => _id = Guard.NotNullOrWhiteSpace(value, nameof(Id));
    }

    public string Name
    {
        get => _name;
        set => _name = Guard.NotNullOrWhiteSpace(value, nameof(Name));
    }
}