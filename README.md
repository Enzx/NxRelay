# NxRelay
![.NET](https://img.shields.io/badge/.NET-8.0-blue)
NxRelay is a small messaging library that provides two patterns:

* **Events** – a lightweight event aggregator that allows publishing messages to any number of subscribers.
* **Mediator** – a request/response dispatcher similar to the mediator pattern used in CQRS style applications.

Both patterns can be used independently or together. The project targets **.NET 8** and contains extension methods for registering handlers with the `Microsoft.Extensions.DependencyInjection` container.

## Building

The repository contains a Visual Studio solution and can be built with the .NET SDK:

```bash
dotnet build
```

## Running Tests

Unit tests are written with **NUnit**. Run them using:

```bash
dotnet test
```

## Usage

Register messaging services in your application startup:

```csharp
var services = new ServiceCollection();
// Optionally pass assemblies that contain your handlers
services.AddMessaging(typeof(MyHandler).Assembly);
```

Inject `Events` or `IMediator` where needed and register handlers or publish messages.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
