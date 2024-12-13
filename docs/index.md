# Crispy Waffle Project

A comprehensive toolkit designed to enhance .NET projects with a variety of functionalities.

![Crispy Waffle Logo](https://raw.githubusercontent.com/guibranco/CrispyWaffle/main/logo.png)

This project is proudly maintained by [@guibranco ![GitHub followers](https://img.shields.io/github/followers/guibranco?style=social)](https://github.com/guibranco).

Source code available at: [GitHub ![GitHub stars](https://img.shields.io/github/stars/guibranco/CrispyWaffle?style=social)](https://github.com/guibranco/CrispyWaffle).

Currently opened issues: [![GitHub issues](https://img.shields.io/github/issues/guibranco/crispywaffle)](https://github.com/guibranco/crispywaffle/issues).

## Project Status

| Build Status | Last Commit | Tests | Coverage | Code Smells | Lines of Code |
|:------------:|:-----------:|:-----:|:---------:|:-----------:|:-------------:|
| [![Build status](https://ci.appveyor.com/api/projects/status/dr93gad0na076ng3/branch/main?svg=true)](https://ci.appveyor.com/project/guibranco/crispywaffle/branch/main) | [![GitHub last commit](https://img.shields.io/github/last-commit/guibranco/CrispyWaffle/main)](https://github.com/guibranco/CrispyWaffle) | [![AppVeyor tests (branch)](https://img.shields.io/appveyor/tests/guibranco/crispywaffle/main?compact_message)](https://ci.appveyor.com/project/guibranco/crispywaffle/branch/main/tests) | [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=coverage)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle) | [![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=code_smells)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle) | [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=ncloc)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle) |

## Features

Crispy Waffle provides a rich set of features to enhance your .NET applications:

### Caching
Efficiently cache data to improve performance.

**Example Usage:**
```csharp
var cache = new CacheProvider();
cache.Set("key", "value", TimeSpan.FromMinutes(10));
var value = cache.Get<string>("key");
```

### Communication
#### FTP Client
Manage file transfers using the FTP protocol.

**Example Usage:**
```csharp
var ftpClient = new FtpClient("ftp://example.com", "username", "password");
ftpClient.Upload("localFile.txt", "remoteFile.txt");
```

#### SMTP Mailer Client
Send emails easily via SMTP.

**Example Usage:**
```csharp
var mailer = new SmtpMailer("smtp.example.com", "username", "password");
mailer.Send("to@example.com", "Subject", "Email body");
```

### Conversion Extensions
Simplify data type conversions.

**Example Usage:**
```csharp
int number = "123".ToInt();
```

### Events Dispatching
Trigger and manage application events.

**Example Usage:**
```csharp
var eventDispatcher = new EventDispatcher();
eventDispatcher.Subscribe<MyEvent>(e => Console.WriteLine(e.Message));
eventDispatcher.Dispatch(new MyEvent("Hello, World!"));
```

### Logging
Capture logs for debugging and monitoring.

**Example Usage:**
```csharp
var logger = new Logger();
logger.Log("This is a log message.");
```

### Messaging
Implement messaging patterns for inter-component communication.

**Example Usage:**
```csharp
var messageBus = new MessageBus();
messageBus.Publish(new MyMessage("Hello!"));
```

### Scheduled Task Execution
Automate execution of tasks at intervals.

**Example Usage:**
```csharp
var scheduler = new TaskScheduler();
scheduler.Schedule(() => Console.WriteLine("Task executed!"), TimeSpan.FromMinutes(1));
```

### Serialization Helpers
Serialize and deserialize data in JSON and XML formats.

**Example Usage:**
```csharp
var jsonData = JsonSerializer.Serialize(new { Name = "Crispy Waffle" });
var obj = JsonSerializer.Deserialize<MyClass>(jsonData);
```

### Service Locator
Acts as a Dependency Injection and IoC container.

**Example Usage:**
```csharp
var serviceLocator = new ServiceLocator();
serviceLocator.Register<IService, ServiceImpl>();
var service = serviceLocator.Get<IService>();
```

### String Extensions
Enhance string manipulation capabilities.

**Example Usage:**
```csharp
string example = "Crispy Waffle";
bool containsWaffle = example.Contains("Waffle");
```

## Examples

Most methods are thoroughly tested, and usage examples can be found in the [test project source code](https://github.com/guibranco/CrispyWaffle). This resource provides practical demonstrations of each feature in action.

## Class Diagram

The following class diagram illustrates the architecture and relationships within the Crispy Waffle project:

<script src="https://zoomhub.net/alVEz.js?width=800px&height=auto&border=none"></script>
