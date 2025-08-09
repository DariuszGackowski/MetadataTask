using FivetranClient.Utilities;

namespace FivetranClient.Models;

public class Connector
{
    private string _id = string.Empty;
    private string _service = string.Empty;
    private string _schema = string.Empty;
    private bool? _paused;

    public string Id
    {
        get => _id;
        set => _id = Guard.NotNullOrWhiteSpace(value, nameof(Id));
    }

    public string Service
    {
        get => _service;
        set => _service = Guard.NotNullOrWhiteSpace(value, nameof(Service));
    }

    public string Schema
    {
        get => _schema;
        set => _schema = Guard.NotNullOrWhiteSpace(value, nameof(Schema));
    }

    public bool? Paused
    {
        get => _paused;
        set => _paused = value;
    }
}