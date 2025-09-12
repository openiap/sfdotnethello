## C#/.NET Coding Conventions

- Always use **double quotes** for strings.
- When generating C#/.NET code, **follow these conventions** strictly.
- Use proper C# naming conventions: PascalCase for classes, methods, and properties; camelCase for local variables and parameters.
- Use `var` for local variable declarations when the type is obvious.
- Prefer explicit async/await patterns over blocking calls.

## Data Access

All data must be stored in **OpenIAP's MongoDB database** using the official .NET client library.

Start by declaring and connecting the client:

```csharp
using OpenIAP;
using System.Threading.Tasks;

var client = new Client();
await client.ConnectAsync();
```

## Connection Lifecycle

If you're writing code that needs to reinitialize state when reconnecting (e.g., registering a queue or setting up a watch), wrap it in an `OnConnected` method and hook into the client event stream:
This **MUST** be called after ConnectAsync(), do not worry, you will still get the initial `Connected` and `SignedIn`

```csharp
await client.ConnectAsync();

client.OnClientEvent += OnClientEvent;

private void OnClientEvent(ClientEvent clientEvent)
{
    if (clientEvent != null && clientEvent.Event == "SignedIn")
    {
        Task.Run(OnConnectedAsync);
    }
}

private async Task OnConnectedAsync()
{
    try
    {
        var queueName = await client.RegisterQueueAsync("", HandleQueueMessage);
    }
    catch (Exception error)
    {
        client.Error(error.Message);
    }
}

private async Task HandleQueueMessage(QueueEvent queueEvent)
{
    // Handle incoming event
}
```

## API Reference

Use the following methods when interacting with the OpenIAP platform:

### Database Operations

```csharp
await client.QueryAsync(collectionName: "", query: "", projection: "", orderBy: "", skip: 0, top: 100, queryAs: "", explain: false);
await client.AggregateAsync(collectionName: "", aggregates: new object[] {}, queryAs: "", hint: "", explain: false);
await client.CountAsync(collectionName: "", query: "", queryAs: "", explain: false);
await client.DistinctAsync(collectionName: "", field: "", query: "", queryAs: "", explain: false);
await client.InsertOneAsync(collectionName: "", item: new {}, w: 1, j: false);
await client.InsertManyAsync(collectionName: "", items: new object[] {}, w: 1, j: false, skipResults: false);
await client.UpdateOneAsync(collectionName: "", item: new {}, w: 1, j: false);
await client.InsertOrUpdateOneAsync(collectionName: "", item: new {}, uniqueness: "_id", w: 1, j: false);
await client.DeleteOneAsync(collectionName: "", id: "");
await client.DeleteManyAsync(collectionName: "", query: "");
```

### Collection & Index Management

```csharp
await client.ListCollectionsAsync();
await client.CreateCollectionAsync(collectionName: "");
await client.DropCollectionAsync(collectionName: "");
await client.GetIndexesAsync(collectionName: "");
await client.CreateIndexAsync(collectionName: "", index: new {}, options: new {}, name: "");
await client.DropIndexAsync(collectionName: "", indexName: "");
```

### Authentication

```csharp
await client.SignInAsync(username: "", password: "", jwt: "", agent: "", version: "", longToken: false, validateOnly: false, ping: false);
await client.ConnectAsync();
await client.DisconnectAsync();
```

### File Transfer

```csharp
await client.UploadAsync(filePath: "", fileName: "", mimeType: "", metadata: new {}, collectionName: "");
await client.DownloadAsync(collectionName: "", id: "", folder: "", fileName: "");
```

### Work Items
Work item queues contains a list of "units of work", something that needs to be processed. Items start in state "new", and when we pop an item, the server updates its state to "processing", therefore it's VITAL that we always update the workitem's state to either "retry" if there was an error, or "successful" if there was no error and call UpdateWorkItemAsync to save it.

```csharp
await client.PushWorkItemAsync(wiq: "", wiqId: "", name: "", payload: "{}", nextRun: 0, successWiqId: "", failedWiqId: "", successWiq: "", failedWiq: "", priority: 2, files: new string[] {});
await client.PopWorkItemAsync(wiq: "", wiqId: "", downloadFolder: ".");
await client.UpdateWorkItemAsync(workItem: new {}, ignoreMaxRetries: false, files: new string[] {});
await client.DeleteWorkItemAsync(id: "");
```

### Events & Messaging

```csharp
// Register a watch (change stream), and calls callback everytime an object is inserted, updated or deleted
await client.WatchAsync(collectionName: "", paths: new string[] {}, callback: WatchCallback);  // callback(WatchEvent watchEvent, int eventCounter)
await client.UnwatchAsync(watchId: "");

// Register a queue and handle incoming messages, returns queuename used for receiving messages
await client.RegisterQueueAsync(queueName: "", callback: QueueCallback);  // callback(QueueEvent queueEvent)

// Register an exchange and handle incoming messages, returns queuename used for receiving messages
await client.RegisterExchangeAsync(exchangeName: "", algorithm: "", routingKey: "", addQueue: true, callback: ExchangeCallback);  // callback(QueueEvent queueEvent)

await client.UnregisterQueueAsync(queueName: "");

// Sends a message to a message queue or exchange, queuename or exchangename is mandatory, so is data
await client.QueueMessageAsync(queueName: "", data: new {}, replyTo: "", exchangeName: "", correlationId: "", routingKey: "", stripToken: false, expiration: 0);

// Sends a message to a message queue or exchange, and waits for a reply, and returns it
await client.RpcAsync(queueName: "", data: new {}, stripToken: false);
```

### Helpers

```csharp
// call custom commands, not added to the official API yet, these might change over time and will not be backward compatible 
await client.CustomCommandAsync(command: "", id: "", name: "", data: "");  // data must be a JSON string or ""

client.OnClientEvent += callback;
client.OffClientEvent -= callback;

client.UniqueId();
client.FormatBytes(bytesValue, decimals: 2);  // used to format a number to b/MB/GB etc.
client.Stringify(obj);  // Better error messages than JsonSerializer.Serialize when input is malformed

// Create an observable gauge, that can be used to create custom graphs in grafana, like keeping track of items processed or users online etc.
// the client will automatically keep reporting the last set name, until you call DisableObservableGauge
client.SetF64ObservableGauge(name: "", value: 0.0, description: "");
client.SetU64ObservableGauge(name: "", value: 0, description: "");
client.SetI64ObservableGauge(name: "", value: 0, description: "");
client.DisableObservableGauge(name: "");

client.EnableTracing("openiap=info");  // Always call this early to enable logging, other options are openiap=error, openiap=debug or openiap=trace
client.DisableTracing();

client.Info(...);  // Use this instead of Console.WriteLine()
client.Error(...);  // Use this instead of Console.WriteLine() for errors
client.Verbose(...);  // Use this instead of Console.WriteLine() for debug
client.Trace(...);
```

## Logging

**Never use** `Console.WriteLine()` or standard logging functions.

Instead, use the OpenIAP logging functions:

- `client.Info(...)`
- `client.Warn(...)`
- `client.Error(...)`
- `client.Verbose(...)`
- `client.Trace(...)`

## Error Handling

Always wrap OpenIAP operations in try-catch blocks:

```csharp
try
{
    var result = await client.QueryAsync(collectionName: "users", query: "{}");
    client.Info($"Query returned {result.Length} results");
}
catch (Exception error)
{
    client.Error($"Query failed: {error.Message}");
    // Handle error appropriately
}
```

## Async/Await Patterns

All OpenIAP operations are asynchronous. Always use `await` and ensure your methods are marked as `async`:

```csharp
async Task Main()
{
    var client = new Client();
    try
    {
        await client.ConnectAsync();
        // Your code here
    }
    catch (Exception error)
    {
        client.Error($"Connection failed: {error.Message}");
    }
    finally
    {
        await client.DisconnectAsync();
    }
}
```

## Type Safety

Use proper C# types and nullable reference types for better code safety:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public async Task<List<Dictionary<string, object>>?> ProcessDataAsync(List<Dictionary<string, object>> items)
{
    try
    {
        var result = await client.InsertManyAsync(collectionName: "data", items: items.ToArray());
        return result?.ToList();
    }
    catch (Exception error)
    {
        client.Error($"Failed to process data: {error.Message}");
        return null;
    }
}
```
