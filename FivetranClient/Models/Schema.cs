using FivetranClient.Utilities;

namespace FivetranClient.Models;

public class Schema
{
    private string _nameInDestination = string.Empty;
    private bool? _enabled;
    private Dictionary<string, Table> _tables = [];

    public string NameInDestination
    {
        get => _nameInDestination;
        set => _nameInDestination = Guard.NotNullOrWhiteSpace(value, nameof(NameInDestination));
    }

    public bool? Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    public Dictionary<string, Table> Tables
    {
        get => _tables;
        set => _tables = Guard.NotNull(value, nameof(Tables));
    }
}