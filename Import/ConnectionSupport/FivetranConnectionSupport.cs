using FivetranClient;
using Import.Helpers.Fivetran;
using System.Text;
using System.Collections.Concurrent;

namespace Import.ConnectionSupport;

// equivalent of database is group in Fivetran terminology
public class FivetranConnectionSupport : IConnectionSupport
{
    public const string ConnectorTypeCode = "FIVETRAN";
    private record FivetranConnectionDetailsForSelection(string ApiKey, string ApiSecret);

    public object? GetConnectionDetailsForSelection()
    {
        Console.Write("Provide your Fivetran API Key: ");
        string? apiKey = ReadSecureLine() ?? throw new ArgumentNullException(nameof(apiKey));
        Console.Write("Provide your Fivetran API Secret: ");
        string? apiSecret = ReadSecureLine() ?? throw new ArgumentNullException(nameof(apiSecret));

        return new FivetranConnectionDetailsForSelection(apiKey, apiSecret);
    }
    private static string ReadSecureLine()
    {
        var password = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Length--;
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        return password.ToString();
    }

    public object? GetConnection(object? connectionDetails, string? selectedToImport)
    {
        if (connectionDetails is not FivetranConnectionDetailsForSelection details)
        {
            throw new ArgumentException("Invalid connection details provided.");
        }

        var restApiManager = new RestApiManager(details.ApiKey, details.ApiSecret, TimeSpan.FromSeconds(40));

        return new RestApiManagerWrapper(restApiManager,
            selectedToImport ?? throw new ArgumentNullException(nameof(selectedToImport)));
    }

    public void CloseConnection(object? connection)
    {
        if (connection is IDisposable disposable)
        {
            disposable.Dispose();
        }
        else
        {
            throw new ArgumentException("Invalid connection type provided.");
        }
    }
    public string SelectToImport(object? connectionDetails)
    {
        if (connectionDetails is not FivetranConnectionDetailsForSelection details)
        {
            throw new ArgumentException("Invalid connection details provided.", nameof(connectionDetails));
        }

        using var restApiManager = new RestApiManager(details.ApiKey, details.ApiSecret, TimeSpan.FromSeconds(40));

        var groups = restApiManager.GetGroupsAsync(CancellationToken.None).ToBlockingEnumerable().ToList();

        if (!groups.Any())
        {
            throw new InvalidOperationException("No groups found in Fivetran account.");
        }

        var selectedGroup = SelectGroup(groups);
        return selectedGroup.Id;
    }

    private static FivetranClient.Models.Group SelectGroup(IReadOnlyList<FivetranClient.Models.Group> groups)
    {
        Console.WriteLine("Available groups in Fivetran account:");

        for (int i = 0; i < groups.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {groups[i].Name} (ID: {groups[i].Id})");
        }

        Console.Write("Please select a group to import from (by number): ");
        var input = Console.ReadLine();

        if (!int.TryParse(input, out var selectedIndex) || selectedIndex < 1 || selectedIndex > groups.Count)
        {
            throw new ArgumentException("Invalid group selection. Please enter a valid number from the list.");
        }

        return groups[selectedIndex - 1];
    }

    public void RunImport(object? connection)
    {
        if (connection is not RestApiManagerWrapper restApiManagerWrapper)
        {
            throw new ArgumentException("Invalid connection type provided.", nameof(connection));
        }

        var restApiManager = restApiManagerWrapper.RestApiManager;
        var groupId = restApiManagerWrapper.GroupId;

        var connectors = restApiManager.GetConnectorsAsync(groupId, CancellationToken.None).ToBlockingEnumerable().ToList();
        if (!connectors.Any())
        {
            throw new InvalidOperationException("No connectors found in the selected group.");
        }

        var allMappingsBuffer = new ConcurrentBag<string>();
        Parallel.ForEach(connectors, connector =>
        {
            var connectorSchemas = restApiManager.GetConnectorSchemasAsync(connector.Id, CancellationToken.None).Result;

            if (connectorSchemas?.Schemas == null) return;

            foreach (var schema in connectorSchemas.Schemas)
            {
                if (schema.Value?.Tables == null) continue;

                foreach (var table in schema.Value.Tables)
                {
                    var mappingLine = $"  {connector.Id}: {schema.Key}.{table.Key} -> {schema.Value.NameInDestination}.{table.Value.NameInDestination}";
                    allMappingsBuffer.Add(mappingLine);
                }
            }
        });

        Console.WriteLine("Lineage mappings:");
        foreach (var line in allMappingsBuffer)
        {
            Console.WriteLine(line);
        }
    }
}