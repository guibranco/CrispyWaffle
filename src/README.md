# Custom IoC Container Integration

This project demonstrates the integration of a custom Inversion of Control (IoC) container using the `.NET` method `HostBuilder.UseServiceProviderFactory`. The custom service provider factory allows the application to use a tailor-made IoC container, aligning with standards from established IoC libraries without relying on external dependencies.

## Key Features

- **Enhance Dependency Management**: Advanced techniques for managing dependencies, including modular registrations and custom lifecycles.
- **Increase Flexibility**: Customize how services are registered and resolved, accommodating complex scenarios.
- **Align with Best Practices**: Follow established patterns from well-known IoC containers, improving code maintainability and scalability.
- **Avoid External Dependencies**: Achieve the benefits of popular IoC libraries without adding external library dependencies to the project.

## Usage

1. **Create Host Builder**: Modify the application's entry point to use the custom service provider factory.

    ```csharp
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new CustomServiceProviderFactory())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    ```
