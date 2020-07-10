# EF Core 3.1.x Context Pool Multiplexer

Context pooling is used by EF Core to keep connections persistently open, and not be constantly opening and closing them. Normally, registering a context to a context pool gives the pool control over the context configuration, so that all contexts of the same type are connected to the same database, with the same configuration

This library allows using the same strongly typed context to have multiple configurations (i.e. having the context defined once but balancing a multi-tenant application across multiple databases).

## How it Works

The internals of the behavior in this package still adhere to the rules of EF Core's context pooling: all instances of one type behave the same. To accomodate that, this takes the context type and uses reflection to create child classes at registration time, so each time you register the context, it is internally a different type and is pooled independently.

## Usage

### Install via NuGet

To install EntityFrameworkCore.DbContextPoolMultiplexer, run the following command in the Package Manager Console:

[![Nuget](https://img.shields.io/nuget/v/EntityFrameworkCore.DbContextPoolMultiplexer)](https://github.com/matt-claycomb/EntityFrameworkCore.DbContextPoolMultiplexer)

```powershell
PM> Install-Package EntityFrameworkCore.DbContextPoolMultiplexer
```

You can also view the [package page](http://www.nuget.org/packages/EntityFrameworkCore.DbContextPoolMultiplexer/) on NuGet.

### Configuration

All of the setup is completed in the ConfigureServices method, before the service provider is built:

```csharp
public void Configureservices(IServiceCollection services) {
	services.BeginRegisteringDbContextPoolMultiplexerService<CustomerDbContext>().
		.AddConnectionDetails("tenant-a", (provider, builder) => builder.UseSqlServer("Data Source=SQLSRV1; Initial Catalog=tenant-a;"))
		.AddConnectionDetails("tenant-b", (provider, builder) => builder.UseSqlServer("Data Source=SQLSRV2; Initial Catalog=tenant-b;"))
		.AddConnectionDetails("tenant-c", (provider, builder) => builder.UseSqlServer("Data Source=SQLSRV1; Initial Catalog=tenant-c;"))
		.AddConnectionDetails("tenant-d", (provider, builder) => builder.UseSqlServer("Data Source=SQLSRV2; Initial Catalog=tenant-d;"))
	.FinishRegisteringDbContextPoolMultiplierService();
}
```

All methods are fluent, so after the call to `FinishRegisteringDbContextPoolMultiplierService()`, you can tack on additional service registrations, as needed.

### Implementation

To access the context pools, simply type-hint the service (`DbContextPoolMultiplexerService<CustomerDbContext>`) in a class or page, or request it manually from the service provider (if you have access to it).

Once you have an instance of the service, there are multiple ways to obtain a database context:

```csharp
CustomerDbContext tenantContext = dbContextPoolMultipexerService.GetDbContext("tenant-a")
```

```csharp
foreach (string contextName in dbContextPoolMultipexerService.GetContextNames())
{
	CustomerDbContext tenantContext = dbContextPoolMultipexerService.GetDbContext(contextName)
}
```

```csharp
Dictionary<string, CustomerDbContext> tenantContexts = dbContextPoolMultipexerService.GetAllContexts();
```


