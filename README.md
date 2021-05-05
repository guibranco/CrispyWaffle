# CrispyWaffle

The CrispyWaffle project - a toolkit for .NET (Core & Framework) projects. 

[![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle)
[![Time tracker](https://wakatime.com/badge/github/guibranco/CrispyWaffle.svg)](https://wakatime.com/badge/github/guibranco/CrispyWaffle)

![Crispy Waffle logo](https://raw.githubusercontent.com/guibranco/CrispyWaffle/master/logo.png)

Documentation of the project: [Read the Docs](https://guibranco.github.io/CrispyWaffle/)

## CI/CD

| Branch | Build status | Last commit | Tests |
|--------|--------------|-------------|-------|
| Master | [![Build status](https://ci.appveyor.com/api/projects/status/dr93gad0na076ng3/branch/master?svg=true)](https://ci.appveyor.com/project/guibranco/crispywaffle/branch/master) | [![GitHub last commit](https://img.shields.io/github/last-commit/guibranco/CrispyWaffle/master)](https://github.com/guibranco/CrispyWaffle) | ![AppVeyor tests (branch)](https://img.shields.io/appveyor/tests/guibranco/crispywaffle/master?compact_message) |
| Develop | [![Build status](https://ci.appveyor.com/api/projects/status/dr93gad0na076ng3/branch/develop?svg=true)](https://ci.appveyor.com/project/guibranco/crispywaffle/branch/develop) | [![GitHub last commit](https://img.shields.io/github/last-commit/guibranco/CrispyWaffle/develop)](https://github.com/guibranco/CrispyWaffle) | ![AppVeyor tests (branch)](https://img.shields.io/appveyor/tests/guibranco/crispywaffle/develop?compact_message) |

## Code Quality

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/f9e814c726bb4ccb8b0380b1fd882f4b)](https://www.codacy.com/manual/guilherme_9/CrispyWaffle?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=guibranco/CrispyWaffle&amp;utm_campaign=Badge_Grade)
[![codecov](https://codecov.io/gh/guibranco/CrispyWaffle/branch/master/graph/badge.svg)](https://codecov.io/gh/guibranco/CrispyWaffle)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=alert_status)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=coverage)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)

[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=ncloc)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=sqale_index)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=duplicated_lines_density)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=security_rating)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=code_smells)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=bugs)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)

---

## Installation

### Github Releases

[![GitHub last release](https://img.shields.io/github/release-date/guibranco/CrispyWaffle.svg?style=flat)](https://github.com/guibranco/CrispyWaffle) [![Github All Releases](https://img.shields.io/github/downloads/guibranco/CrispyWaffle/total.svg?style=flat)](https://github.com/guibranco/CrispyWaffle)

Download the latest zip file from the [Release](https://github.com/GuiBranco/CrispyWaffle/releases) page.

### Nuget package manager

| Package | Version | Downloads |
|------------------|:-------:|:-------:|
| **CrispyWaffle** | [![CrispyWaffle NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle/) | [![CrispyWaffle NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle/) |
| **CrispyWaffle.Configuration** | [![CrispyWaffle Configuration NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Configuration.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Configuration/) | [![CrispyWaffle Configuration NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Configuration.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Configuration/) |
| **CrispyWaffle.Elmah** | [![CrispyWaffle ELMAH NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Elmah.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Elmah/) | [![CrispyWaffle ELMAH NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Elmah.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Elmah/) |
| **CrispyWaffle.Log4Net** | [![CrispyWaffle Log4Net NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Log4Net.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Log4Net/) | [![CrispyWaffle Log4Net NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Log4Net.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Log4Net/) |
| **CrispyWaffle.RabbitMQ** | [![CrispyWaffle.RabbitMQ NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.RabbitMQ.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.RabbitMQ/) | [![CrispyWaffle.RabbitMQ NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.RabbitMQ.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.RabbitMQ/) |
| **CrispyWaffle.Redis** | [![CrispyWaffle Redis NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Redis.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Redis/) | [![CrispyWaffle Redis NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Redis.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Redis/) |

More information avaiable [here](https://guibranco.github.io/CrispyWaffle/installation/).

---

## User guide

User guide is available [here](https://guibranco.github.io/CrispyWaffle/user-guide/basic-usage/).

---

## Changelog

Changelog is available [here](https://guibranco.github.io/CrispyWaffle/changelog/).
