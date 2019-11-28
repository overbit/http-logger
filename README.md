# OverApps Services Logging

This library is extending the ILogger of .NET core in order to log events against a logger REST API

## Instructions

1. Install OverApps.Logging nuget package
2. Add the project specific configuration

    ```json
    //Example of appsettings.json
    {
        "Logging": {
            "LogLevel": {
                "Default": "Warning",
                "Microsoft": "Warning",
                "Microsoft.Hosting.Lifetime": "Information"
            },
            "ApplicationName": "my-application",
            "ServiceMonitoring": {
                "LoggingEndpoint": "https://domain.com/api/logger/"
            }
        }
    }
    ```

3. Add the new logger in the Program class

    ```csharp
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            // IMPORTANT BIT >>>
            .ConfigureLogging((hostBuilderContext, builder) => {
                builder.AddOverAppsLogging(hostBuilderContext.Configuration, new HttpClient());
            })
            // <<< IMPORTANT BIT
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    ```

4. Log as usual in .NET Core [https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.0]

## Tech specs

- Targets .NET Standard 2.1 / .NET Core 3.0

## Build locally

There are multiple options to build this project locally:

- Visual Studio 2019+ (That supports .NET Standards 2.1)
- CLI

### CLI Useful commands

```bash
# Build solution
dotnet build -o build/ OverApps.Logging.sln

# Run tests
dotnet test OverApps.Logging.Logging.sln

# Package into a nuget package
dotnet pack -o . OverApps.Logging.Logging.sln
```