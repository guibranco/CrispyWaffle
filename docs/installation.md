# Installation

## About

The Crispy Waffle project is divided into some packages:

| Package           | Description                                                                                                                                 |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| **CrispyWaffle**  | The core package, with main features.                                                                                                       |
| **Configuration** | Configuration abstractions.                                                                                                                 |
| **ElasticSearch** | The [ElasticSearch](https://www.elastic.co/) extension package.<br /> Implements the Elastic Search client and log features.                |
| **ELMAH**         | The [ELMAH](https://elmah.github.io/) exception handler.<br /> Redirects the exceptions messages (from LogConsumer.Handle method) to ELMAH. |
| **EventLog**      | The event log extension package.<br /> Implements the log classes to Windows event log.                                                     |
| **Log4Net**       | The [Log4Net](https://github.com/apache/logging-log4net) log provider.<br /> Redirects the log output to Log4Net.                           |
| **RabbitMQ**      | The [RabbitMQ](https://www.rabbitmq.com/) extension package.<br /> Implements the RabbitMQ message broker client and log features.          |
| **Redis**         | The [Redis](https://redis.io) extension package.<br /> Implements the Redis cache, log and telemetry features.                              |
| **Utils**         | The utility extension package.<br /> Implements the utility classes (communications - FTP client, SMTP mailer).                             |

### Language packages

The following language (i18n) packages are available:

| Package   | Description                  |
| --------- | ---------------------------- |
| **Pt-Br** | Brazilian Portuguese (PT-BR) |

## Github Releases

[![GitHub last release](https://img.shields.io/github/release-date/guibranco/CrispyWaffle.svg?style=flat)](https://github.com/guibranco/CrispyWaffle) [![Github All Releases](https://img.shields.io/github/downloads/guibranco/CrispyWaffle/total.svg?style=flat)](https://github.com/guibranco/CrispyWaffle)

Download the latest zip file from the [Release](https://github.com/GuiBranco/CrispyWaffle/releases) page.

## Nuget package manager

| Package                        |                                                                                       Version                                                                                       |                                                                                       Downloads                                                                                        |
| ------------------------------ | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: |
| **CrispyWaffle**               |                      [![CrispyWaffle NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle/)                      |                      [![CrispyWaffle NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle/)                      |
| **CrispyWaffle.Configuration** | [![CrispyWaffle Configuration NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Configuration.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Configuration/) | [![CrispyWaffle Configuration NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Configuration.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Configuration/) |
| **CrispyWaffle.ElasticSearch** | [![CrispyWaffle ElasticSearch NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.ElasticSearch.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.ElasticSearch/) | [![CrispyWaffle ElasticSearch NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.ElasticSearch.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.ElasticSearch/) |
| **CrispyWaffle.Elmah**         |             [![CrispyWaffle ELMAH NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Elmah.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Elmah/)             |             [![CrispyWaffle ELMAH NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Elmah.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Elmah/)             |
| **CrispyWaffle.EventLog**      |             [![CrispyWaffle EventLog NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.EventLog.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.EventLog/)             |             [![CrispyWaffle EventLog NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.EventLog.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.EventLog/)             |
| **CrispyWaffle.Log4Net**       |          [![CrispyWaffle Log4Net NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Log4Net.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Log4Net/)          |          [![CrispyWaffle Log4Net NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Log4Net.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Log4Net/)          |
| **CrispyWaffle.RabbitMQ**      |        [![CrispyWaffle RabbitMQ NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.RabbitMQ.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.RabbitMQ/)         |        [![CrispyWaffle RabbitMQ NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.RabbitMQ.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.RabbitMQ/)         |
| **CrispyWaffle.Redis**         |             [![CrispyWaffle Redis NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Redis.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Redis/)             |             [![CrispyWaffle Redis NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Redis.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Redis/)             |
| **CrispyWaffle.Utils**         |             [![CrispyWaffle Utils NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Utils.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Utils/)             |             [![CrispyWaffle Utils NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Utils.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Utils/)             |

### Language packages (i18n)

| Package                    | Version                                                                                                                                                                               | Downloads                                                                                                                                                                                             |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **CrispyWaffle.I18n.PtBr** | [![CrispyWaffle i18n pt-br NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.I18n.PtBr.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.I18n.PtBr/) | [![CrispyWaffle i18n pt-br NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.I18n.PtBr.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.I18n.PtBr/) |

## Package Manager Console (manual)

**CrispyWaffle**

```ps
Install-Package CrispyWaffle
```

**Configuration**

```ps
Install-Package CrispyWaffle.Configuration
```

**ElasticSearch**

```ps
Install-Package CrispyWaffle.ElasticSearch
```

**ELMAH**

```ps
Install-Package CrispyWaffle.Elmah
```

**EventLog**

```ps
Install-Package CrispyWaffle.EventLog
```

**Log4Net**

```ps
Install-Package CrispyWaffle.Log4Net
```

**RabbitMQ**

```ps
Install-Package CrispyWaffle.RabbitMQ
```

**Redis**

```ps
Install-Package CrispyWaffle.Redis
```

**Utils**

```ps
Install-Package CrispyWaffle.Utils
```
### Language packages

**PT-BR**

```ps
Install-Package CrispyWaffle.I18n.PtBr
```
