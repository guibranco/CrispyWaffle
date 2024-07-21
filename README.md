# Crispy Waffle

🧰 🛠️ Crispy Waffle project - a toolkit for .NET projects.

[![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle)
[![Time tracker](https://wakatime.com/badge/github/guibranco/CrispyWaffle.svg)](https://wakatime.com/badge/github/guibranco/CrispyWaffle)

![Crispy Waffle logo](https://raw.githubusercontent.com/guibranco/CrispyWaffle/main/logo.png)

Documentation: [Read the Docs](https://guibranco.github.io/CrispyWaffle/)

---

## Table of Contents

- [CI/CD](#cicd): Current project status in the build pipeline (AppVeyor).
- [Code Quality](#code-quality): Metrics from some tools about code quality.
- [Installation](#installation): How to install/download this tool.
- [User guide](#user-guide): How to set up, configure and use this tool.
- [Change log](#changelog): Changelog containing the changes that were done in this project.
- [Support](#support): How to get support.
- [Contributing](#contributing): How to contribute.

---

## CI/CD

| Build status                                                                                                                                                             | Last commit                                                                                                                               | Tests                                                                                                                                                                                     | Coverage                                                                                                                                                                  | Code Smells                                                                                                                                                                     | LoC                                                                                                                                                                         |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [![Build status](https://ci.appveyor.com/api/projects/status/dr93gad0na076ng3/branch/main?svg=true)](https://ci.appveyor.com/project/guibranco/crispywaffle/branch/main) | [![GitHub last commit](https://img.shields.io/github/last-commit/guibranco/CrispyWaffle/main)](https://github.com/guibranco/CrispyWaffle) | [![AppVeyor tests (branch)](https://img.shields.io/appveyor/tests/guibranco/crispywaffle/main?compact_message)](https://ci.appveyor.com/project/guibranco/crispywaffle/branch/main/tests) | [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=coverage)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle) | [![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=code_smells)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle) | [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=ncloc)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle) |

---

## Code Quality

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/cdac433295dc4d39b4a5377d147f50fc)](https://www.codacy.com/gh/guibranco/CrispyWaffle/dashboard?utm_source=github.com&utm_medium=referral&utm_content=guibranco/CrispyWaffle&utm_campaign=Badge_Grade)
[![Codacy Badge](https://app.codacy.com/project/badge/Coverage/cdac433295dc4d39b4a5377d147f50fc)](https://www.codacy.com/gh/guibranco/CrispyWaffle/dashboard?utm_source=github.com&utm_medium=referral&utm_content=guibranco/CrispyWaffle&utm_campaign=Badge_Coverage)

[![codecov](https://codecov.io/gh/guibranco/CrispyWaffle/branch/main/graph/badge.svg)](https://codecov.io/gh/guibranco/CrispyWaffle)
[![CodeFactor](https://www.codefactor.io/repository/github/guibranco/CrispyWaffle/badge)](https://www.codefactor.io/repository/github/guibranco/CrispyWaffle)

[![Maintainability](https://api.codeclimate.com/v1/badges/fdaf045297f48946696a/maintainability)](https://codeclimate.com/github/guibranco/CrispyWaffle/maintainability)
[![Test Coverage](https://api.codeclimate.com/v1/badges/fdaf045297f48946696a/test_coverage)](https://codeclimate.com/github/guibranco/CrispyWaffle/test_coverage)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=alert_status)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)

[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=sqale_index)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=duplicated_lines_density)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=security_rating)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)

[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=bugs)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=guibranco_CrispyWaffle&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=guibranco_CrispyWaffle)

[![DeepSource](https://app.deepsource.com/gh/guibranco/CrispyWaffle.svg/?label=active+issues&show_trend=true&token=r3XGa8MQHGZERdIhKB5EZXfL)](https://app.deepsource.com/gh/guibranco/CrispyWaffle/?ref=repository-badge)

---

## Installation

### Github Releases

[![GitHub last release](https://img.shields.io/github/release-date/guibranco/CrispyWaffle.svg?style=flat)](https://github.com/guibranco/CrispyWaffle) [![Github All Releases](https://img.shields.io/github/downloads/guibranco/CrispyWaffle/total.svg?style=flat)](https://github.com/guibranco/CrispyWaffle)

Download the latest zip file from the [Release](https://github.com/GuiBranco/CrispyWaffle/releases) page.

### Nuget package manager

| Package                        |                                                                                       Version                                                                                        |                                                                                       Downloads                                                                                        |
| ------------------------------ | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: |
| **CrispyWaffle**               |                      [![CrispyWaffle NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle/)                       |                      [![CrispyWaffle NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle/)                      |
| **CrispyWaffle.Configuration** | [![CrispyWaffle Configuration NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Configuration.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Configuration/)  | [![CrispyWaffle Configuration NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Configuration.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Configuration/) |
| **CrispyWaffle.ElasticSearch** | [![CrispyWaffle ElasticSearch NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.ElasticSearch.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.ElasticSearch/)  | [![CrispyWaffle ElasticSearch NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.ElasticSearch.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.ElasticSearch/) |
| **CrispyWaffle.Elmah**         |             [![CrispyWaffle ELMAH NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Elmah.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Elmah/)              |             [![CrispyWaffle ELMAH NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Elmah.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Elmah/)             |
| **CrispyWaffle.EventLog**         |             [![CrispyWaffle Event Log NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.EventLog.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.EventLog/) |             [![CrispyWaffle Event Log NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.EventLog.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.EventLog/)             |
| **CrispyWaffle.Log4Net**       |          [![CrispyWaffle Log4Net NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Log4Net.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Log4Net/)           |          [![CrispyWaffle Log4Net NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Log4Net.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Log4Net/)          |
| **CrispyWaffle.RabbitMQ**      |        [![CrispyWaffle.RabbitMQ NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.RabbitMQ.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.RabbitMQ/)          |        [![CrispyWaffle.RabbitMQ NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.RabbitMQ.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.RabbitMQ/)         |
| **CrispyWaffle.Redis**         |             [![CrispyWaffle Redis NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Redis.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Redis/)              |             [![CrispyWaffle Redis NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Redis.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Redis/)             |
| **CrispyWaffle.Utils**         |             [![CrispyWaffle Utils NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Utils.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Utils/)              |             [![CrispyWaffle Utils NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Utils.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Utils/)             |

#### Language packages

| Package   |                                                                        Version                                                                         |                                                                         Downloads                                                                         |
| --------- | :----------------------------------------------------------------------------------------------------------------------------------------------------: | :-------------------------------------------------------------------------------------------------------------------------------------------------------: |
| **CrispyWaffle.I18n.PtBr** | [![PT-BR NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.I18n.PtBr.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.I18n.PtBr/) | [![PT-BR NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.I18n.PtBr.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.I18n.PtBr/) |

More information is available [here](https://guibranco.github.io/CrispyWaffle/installation/).

---

## User guide

The user guide is available [here](https://guibranco.github.io/CrispyWaffle/user-guide/basic-usage/).

---

## Changelog

The changelog is available [here](https://guibranco.github.io/CrispyWaffle/changelog/).

---

## Support

Please [open an issue](https://github.com/guibranco/crispywaffle/issues/new) for support.

---

## Contributing

Refer to [CONTRIBUTING.md](CONTRIBUTING.md) to learn how to contribute to this project!

### Contributors

<!-- readme: collaborators,contributors,snyk-bot/-,guistracini-outsurance-ie/-,codefactor-io/- -start -->
<table>
	<tbody>
		<tr>
            <td align="center">
                <a href="https://github.com/guibranco">
                    <img src="https://avatars.githubusercontent.com/u/3362854?v=4" width="100;" alt="guibranco"/>
                    <br />
                    <sub><b>Guilherme Branco Stracini</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/Looseling">
                    <img src="https://avatars.githubusercontent.com/u/69507148?v=4" width="100;" alt="Looseling"/>
                    <br />
                    <sub><b>Batyrkhan Akzholov</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/joaovaladares">
                    <img src="https://avatars.githubusercontent.com/u/42593399?v=4" width="100;" alt="joaovaladares"/>
                    <br />
                    <sub><b>João Vítor Valadares</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/SimranGil">
                    <img src="https://avatars.githubusercontent.com/u/111714647?v=4" width="100;" alt="SimranGil"/>
                    <br />
                    <sub><b>Simran Gill</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/thiagochfc">
                    <img src="https://avatars.githubusercontent.com/u/36862932?v=4" width="100;" alt="thiagochfc"/>
                    <br />
                    <sub><b>Thiago Christopher</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/tiagoschaffer">
                    <img src="https://avatars.githubusercontent.com/u/12534089?v=4" width="100;" alt="tiagoschaffer"/>
                    <br />
                    <sub><b>Tiago Schäffer</b></sub>
                </a>
            </td>
		</tr>
		<tr>
            <td align="center">
                <a href="https://github.com/viktoriussuwandi">
                    <img src="https://avatars.githubusercontent.com/u/68414300?v=4" width="100;" alt="viktoriussuwandi"/>
                    <br />
                    <sub><b>Viktorius Suwandi</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/Yousef-Majidi">
                    <img src="https://avatars.githubusercontent.com/u/28239685?v=4" width="100;" alt="Yousef-Majidi"/>
                    <br />
                    <sub><b>Yousef</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/akhtareimon">
                    <img src="https://avatars.githubusercontent.com/u/15952495?v=4" width="100;" alt="akhtareimon"/>
                    <br />
                    <sub><b>akhtareimon</b></sub>
                </a>
            </td>
		</tr>
	<tbody>
</table>
<!-- readme: collaborators,contributors,snyk-bot/-,guistracini-outsurance-ie/-,codefactor-io/- -end -->

### Bots

<!-- readme: snyk-bot,codefactor-io,bots -start -->
<table>
	<tbody>
		<tr>
            <td align="center">
                <a href="https://github.com/snyk-bot">
                    <img src="https://avatars.githubusercontent.com/u/19733683?v=4" width="100;" alt="snyk-bot"/>
                    <br />
                    <sub><b>Snyk bot</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/codefactor-io">
                    <img src="https://avatars.githubusercontent.com/u/11671095?v=4" width="100;" alt="codefactor-io"/>
                    <br />
                    <sub><b>CodeFactor</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/dependabot[bot]">
                    <img src="https://avatars.githubusercontent.com/in/29110?v=4" width="100;" alt="dependabot[bot]"/>
                    <br />
                    <sub><b>dependabot[bot]</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/github-actions[bot]">
                    <img src="https://avatars.githubusercontent.com/in/15368?v=4" width="100;" alt="github-actions[bot]"/>
                    <br />
                    <sub><b>github-actions[bot]</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/deepsource-autofix[bot]">
                    <img src="https://avatars.githubusercontent.com/in/57168?v=4" width="100;" alt="deepsource-autofix[bot]"/>
                    <br />
                    <sub><b>deepsource-autofix[bot]</b></sub>
                </a>
            </td>
            <td align="center">
                <a href="https://github.com/stack-file[bot]">
                    <img src="https://avatars.githubusercontent.com/in/408123?v=4" width="100;" alt="stack-file[bot]"/>
                    <br />
                    <sub><b>stack-file[bot]</b></sub>
                </a>
            </td>
		</tr>
		<tr>
            <td align="center">
                <a href="https://github.com/codefactor-io[bot]">
                    <img src="https://avatars.githubusercontent.com/in/25603?v=4" width="100;" alt="codefactor-io[bot]"/>
                    <br />
                    <sub><b>codefactor-io[bot]</b></sub>
                </a>
            </td>
		</tr>
	<tbody>
</table>
<!-- readme: snyk-bot,codefactor-io,bots -end -->

