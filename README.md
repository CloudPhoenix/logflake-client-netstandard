# LogFlake Client .NET Standard ![Version](https://img.shields.io/badge/version-1.5.2-blue.svg?cacheSeconds=2592000)

> This repository contains the sources for the client-side components of the LogFlake product suite for applications logs and performance collection for .NET applications.

### 🏠 [LogFlake Website](https://logflake.io) |  🔥 [CloudPhoenix Website](https://cloudphoenix.it)

## Downloads

|NuGet Package Name|Version|Downloads|
|:-:|:-:|:-:|
| [LogFlake.Client.NetStandard](https://www.nuget.org/packages/LogFlake.Client.NetStandard) | ![NuGet Version](https://img.shields.io/nuget/v/logflake.client.netstandard) | ![NuGet Downloads](https://img.shields.io/nuget/dt/logflake.client.netstandard) |

## Usage
1. Retrieve your _application-key_ from Application Settings in LogFlake UI;
2. Add in your `secrets.json` file the following section:
```json
"LogFlake": {
  "AppId": "application-key",
  "Endpoint": "https://logflake-instance-here"  // optional, if missing uses production endpoint
}
```
3. Implement and register as Sigleton the interface `IVersionService`;
4. In your `Program.cs` files, register the LogFlake-related services:
```csharp
// configuration is an instance of IConfiguration
services.AddLogFlake(configuration);
```
5. In your services, simply require `ILogFlakeService` as a dependency;
```csharp
public class SimpleService : ISimpleService
{
    private readonly ILogFlakeService _logFlakeService;

    public SimpleService(ILogFlakeService logFlakeService)
    {
        _logFlakeService = logFlakeService ?? throw new ArgumentNullException(nameof(logFlakeService));
    }
}

```
6. Use it in your service
```csharp
// SimpleService.cs

public void MyMethod()
{
    try 
    {
        doSomething();
        _logFlakeService.WriteLog(LogLevels.INFO, "Hello World", "correlation");
    }
    catch (MeaningfulException ex)
    {
        _logFlakeService.WriteException(ex, "correlation");
    }
}
```
