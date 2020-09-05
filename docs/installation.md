# Installation

## About

The Crispy Waffle project is divided into some packages:

| Package | Description |
|---------|-------------|
| **CrispyWaffle** | The core package, with main features. |
| **Configuration** | Configuration abstractions. |
| **ELMAH** | The [ELMAH](https://elmah.github.io/) exception handler.<br /> Redirects the exceptions messages (from LogConsumer.Handle method) to ELMAH. |
| **Log4Net** | The [Log4Net](https://github.com/apache/logging-log4net) log provider.<br /> Redirects the log output to Log4Net. |

## Github Releases

[![GitHub last release](https://img.shields.io/github/release-date/guibranco/CrispyWaffle.svg?style=flat)](https://github.com/guibranco/CrispyWaffle) [![Github All Releases](https://img.shields.io/github/downloads/guibranco/CrispyWaffle/total.svg?style=flat)](https://github.com/guibranco/CrispyWaffle)

Download the latest zip file from the [Release](https://github.com/GuiBranco/CrispyWaffle/releases) page.

## Nuget package manager

| Package | Version | Downloads |
|------------------|:-------:|:-------:|
| **CrispyWaffle** | [![CrispyWaffle NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle/) | [![CrispyWaffle NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle/) |
| **CrispyWaffle.Configuration** | [![CrispyWaffle Configuration NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Configuration.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Configuration/) | [![CrispyWaffle Configuration NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Configuration.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Configuration/) |
| **CrispyWaffle.Elmah** | [![CrispyWaffle ELMAH NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Elmah.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Elmah/) | [![CrispyWaffle ELMAH NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Elmah.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Elmah/) |
| **CrispyWaffle.Log4Net** | [![CrispyWaffle Log4Net NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Log4Net.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Log4Net/) | [![CrispyWaffle Log4Net NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Log4Net.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Log4Net/) |


## Package Manager Console (manual):

**CrispyWaffle**
```ps
Install-Package CrispyWaffle
```
**Configuration**
```ps
Install-Package CrispyWaffle.Configuration
```

**ELMAH**
```ps
Install-Package CrispyWaffle.Elmah
```

**Log4Net**
```ps
Install-Package CrispyWaffle.Log4Net
```