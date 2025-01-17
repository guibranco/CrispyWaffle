## Version 1.1.0 [Unreleased]

- Added support for YAML serialization and deserialization.
- Introduced `YamlSerializer` class in `CrispyWaffle.Serialization`.
- Updated documentation with usage examples for YAML serialization.

# Changelog

## Version 8.2 [2024-09-11]

- Implement CouchDB cache - [issue #499](https://github.com/guibranco/CrispyWaffle/issues/499) and [pull request #544](https://github.com/guibranco/CrispyWaffle/pull/544) by [@Mohammad-Haris](https://github.com/Mohammad-Haris)

## Version 8.1 [2024-08-08]

- Implement rolling text file log adapter - [issue #37](https://github.com/guibranco/CrispyWaffle/issues/37) and [pull request #479](https://github.com/guibranco/CrispyWaffle/pull/479) by [@Mohammad-Haris](https://github.com/Mohammad-Haris)

## Version 8.0 [2024-07-28]

- Replace NEST and ElasticSearch.Net with Elastic.Clients.Elasticsearch - [issue #444](https://github.com/guibranco/CrispyWaffle/issues/444) and [pull request #470](https://github.com/guibranco/CrispyWaffle/pull/470) by [@Mohammad-Haris](https://github.com/Mohammad-Haris)
- Remove binary serializer - [issue #462](https://github.com/guibranco/CrispyWaffle/issues/462) and [pull request #463](https://github.com/guibranco/CrispyWaffle/pull/463) by [@Mohammad-Haris](https://github.com/Mohammad-Haris)

## Version 7.0 [2024-07-21]

- Rename LifeStyle to Lifetime - [pull request #445](https://github.com/guibranco/CrispyWaffle/pull/445)
- Fix build warnings

## Version 6.0 [2024-05-20]

- Create i18n package - PT-BR - [issue #315](https://github.com/guibranco/CrispyWaffle/issues/315)

## Version 5.0 [2024-01-31]

- Replaced MD5CryptoServiceProvider to MD5.Create - [issue #155](https://github.com/guibranco/CrispyWaffle/issues/155) by [@tiagoschaffer](https://github.com/tiagoschaffer)
- Add System.Text.Json serialization - [issue #192](https://github.com/guibranco/CrispyWaffle/issues/192) by [@Looseling](https://github.com/Looseling)

## Version 4.3 [2023-10-28]

- Remove retry rules from `FtpClient` - [issue #211](https://github.com/guibranco/CrispyWaffle/issues/211) by [@Yousef-Majidi](https://github.com/Yousef-Majidi)
- Create `BaseSerializerAdapter` - [issue #185](https://github.com/guibranco/CrispyWaffle/issues/185)
- Adjust namespaces
- Fix some code smells

## Version 4.2 [2023-07-24]

- Fix SonarCloud reports - [issue #182](https://github.com/guibranco/CrispyWaffle/issues/182) by [@viktoriussuwandi](https://github.com/viktoriussuwandi)
- Fix security in GitHub Actions and AppVeyor pipeline
- Add CSharpier (Linter)
- Add DeepSource scanner

## Version 4.1 [2023-03-22]

- Add Utils project - [issue #150](https://github.com/guibranco/CrispyWaffle/issues/150)

## Version 4.0 [2023-03-22]

- Add NuGet README.md - [issue #139](https://github.com/guibranco/CrispyWaffle/issues/139)

## Version 3.1 [2022-09-10]

- Add CrispyWaffle.ElasticSearch project & package - [issue #119](https://github.com/guibranco/CrispyWaffle/issues/119).

## Version 3.0 [2022-09-09]

- Upgrade dependencies and increase unit test coverage.

## Version 2.5 [2020-09-07]

- Add CrispyWaffle.RabbitMQ project & package  - [issue #92](https://github.com/guibranco/CrispyWaffle/issues/92).

## Version 2.4 [2020-09-06]

- Add CrispyWaffle.Redis project & package.
- Add documentation for caching.

## Version 2.3 [2020-09-06]

- Add FailoverExceptionHandler class - [issue #73](https://github.com/guibranco/CrispyWaffle/issues/73).
- Add Environment Helper class - [issue #75](https://github.com/guibranco/CrispyWaffle/issues/75).

## Version 2.2 [2020-09-05]

- Add a scheduler feature.
- Add documentation using MkDocs.

## Version 2.1 [2020-09-04]

- Add the Configuration project and package.
- Add Mustache template engine (inspired by Mustache/Handlebars).
- Add Resource template repository.
- Add Elmah & Log4Net projects (logging).
- Basic usage examples.

## Version 2.0 [2020-08-03]

- Removed some application-specific patterns from the *StringExtensions* class

## Version 1.3 [2020-07-24]

- Add EvenLogProvider and EventLogAdapter.
- Add log Trace methods that support exceptions.
- Add Fatal log level.

## Version 1.2 [2020-03-28]

- Add some unit tests.
- Update appveyor.yml with build enhancements and test coverages.
- Update readme template.

## Version 1.1 [2019-09-27]

- Add Math Extensions (CrispyWaffle.Extensions.MathExtensions namespace).
- Add Personal Data Validations (CrispyWaffle.Validations.PersonalDataValidations).
- Rename method *FormatDocument* to *FormatBrazilianDocument* (CrispyWaffle.Extensions.ConversionExtensions).
- Rename method *ParsePhoneNumber* to *arseBrazilianPhoneNumber* (CrispyWaffle.Extensions.ConversionExtensions).
- Removed *CleanListItems* (CrispyWaffle.Extensions.ConversionExtensions).
- Rename method *TryParsePhoneNumber* to *TryParseBrazilianPhoneNumber* (CrispyWaffle.Extensions.ConversionExtensions).
- Removed *CleanListItems* and *ToListString* (CrispyWaffle.Extensions.ConversionExtensions) **(Specific to application patterns)**.

---
