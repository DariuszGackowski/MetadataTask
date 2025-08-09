using FivetranClient.Utilities;

namespace FivetranClient.Models;

public class Data<T>
{
    private List<T> _items = [];
    private string? _nextCursor;

    public List<T> Items
    {
        get => _items;
        set => _items = [.. Guard.NotNullOrContainsNull(value, nameof(Items))];
    }

    public string? NextCursor
    {
        get => _nextCursor;
        set
        {
            if (value != null)
                _nextCursor = Guard.NotNullOrWhiteSpace(value, nameof(NextCursor));
            else
                _nextCursor = null;
        }
    }
}