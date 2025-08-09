using FivetranClient.Utilities;

namespace FivetranClient.Models;

public class Table
{
    private string _nameInDestination = string.Empty;

    public string NameInDestination
    {
        get => _nameInDestination;
        set => _nameInDestination = Guard.NotNullOrWhiteSpace(value, nameof(NameInDestination));
    }
}