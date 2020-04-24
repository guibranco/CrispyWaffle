# CrispyWaffle

The CrispyWaffle project - a toolkit for dotNet (both Core & Framework) projects

![Crispy Waffle logo](https://raw.githubusercontent.com/guibranco/CrispyWaffle/master/logo.png)

## CI/CD

[![Build status](https://ci.appveyor.com/api/projects/status/dr93gad0na076ng3?svg=true)](https://ci.appveyor.com/project/guibranco/crispywaffle)
[![CrispyWaffle NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle/)
[![CrispyWaffle NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle/)
[![Github All Releases](https://img.shields.io/github/downloads/guibranco/CrispyWaffle/total.svg?style=flat)](https://github.com/guibranco/CrispyWaffle)
[![Last release](https://img.shields.io/github/release-date/guibranco/CrispyWaffle.svg?style=flat)](https://github.com/guibranco/CrispyWaffle)

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

Download the latest zip file from the [Release](https://github.com/GuiBranco/CrispyWaffle/releases) pages or simple install from [NuGet](https://www.nuget.org/packages/CrispyWaffle) package manager

NuGet URL: https://www.nuget.org/packages/CrispyWaffle

NuGet installation via *Package Manager Console*:

```ps
Install-Package CrispyWaffle
```

---

## Changelog

Version 1.2 (2020-03-28) [@guibranco](https://github.com/guibranco)

- Add some unit tests
- Update appveyor.yml with build enhancements and test coverages.
- Update readme template

Version 1.1 (2019-09-27) [@guibranco](https://github.com/guibranco)

- Add Math Extensions (CrispyWaffle.Extensions.MathExtensions namespace)
- Add Personal Data Validations (CrispyWaffle.Validations.PersonalDataValidations)
- Rename method *FormatDocument* to *FormatBrazilianDocument* (CrispyWaffle.Extensions.ConversionExtensions)
- Rename method *ParsePhoneNumber* to *arseBrazilianPhoneNumber* (CrispyWaffle.Extensions.ConversionExtensions)
- Removed *CleanListItems* (CrispyWaffle.Extensions.ConversionExtensions)
- Rename method *TryParsePhoneNumber* to *TryParseBrazilianPhoneNumber* (CrispyWaffle.Extensions.ConversionExtensions)
- Removed *CleanListItems* and *ToListString* (CrispyWaffle.Extensions.ConversionExtensions) **(Specific to application patterns)**

---
