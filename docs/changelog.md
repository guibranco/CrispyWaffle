# Changelog

## Version 10.4 [2026-10-16]

- Add TOML serializer support using Tomlyn -
  [issue #431][issue-431] and [pull request #981][pr-981] by
  [@rodri-oliveira-dev][rodri]
- Add a features section to the README, document .NET 10 support and the
  packages not yet on NuGet, and fill in the missing changelog entries

## Version 10.3 [2026-10-16]

- Add YAML serializer support using YamlDotNet -
  [issue #432][issue-432] and [pull request #980][pr-980] by
  [@rodri-oliveira-dev][rodri]
- Rewrite the documentation site, with a page for every core module and
  integration package plus architecture diagrams -
  [pull request #1007][pr-1007] and [pull request #1008][pr-1008]

## Version 10.2 [2026-07-04]

- Add the `CrispyWaffle.BackgroundJobs` package (hosted background job
  processing, not yet published to NuGet) -
  [pull request #745][pr-745] by [@ducdaiii][ducdaiii]
- Add Redis integration tests -
  [pull request #753][pr-753] by [@Looseling][looseling]
- Add .NET 10 target framework - [pull request #971][pr-971]

## Version 10.1 [2026-07-04]

- **Breaking:** make `CacheManager` and the cache repositories asynchronous -
  [pull request #715][pr-715] by [@HarryKambo][harrykambo]
- **Breaking:** migrate the RabbitMQ wrapper, message receiver and log provider
  to async methods and upgrade `RabbitMQ.Client` to 7.0 -
  [pull request #953][pr-953]
- Add SHA3-256, SHA3-384 and SHA3-512 hashing on .NET 8 or later -
  [pull request #901][pr-901] by [@christopher-conley][christopher-conley]
- Fix the inverted logic in `ExtractQuerystring` -
  [pull request #881][pr-881] by [@Sak3th2004][sak3th2004]
- Replace `XXXX` placeholders in XML documentation with real return
  descriptions - [pull request #882][pr-882] by [@Sak3th2004][sak3th2004]

## Version 10.0 [2025-02-22]

- Drop support to .NET 6 - [pull request #667][pr-667]
- Add the `CrispyWaffle.HttpClient` package (typed HTTP client wrapper with
  retries, not yet published to NuGet) -
  [pull request #742][pr-742] by [@ducdaiii][ducdaiii]

## Version 9.1 [2024-11-26]

- Add .NET 9 target framework - [pull request #631][pr-631]

## Version 9.0 [2024-11-12]

- Refactor tests to use `async`/`await` instead of `Thread.Sleep` -
  [pull request #548][pr-548] by [@mf0zz13][mf0zz13]
- Run integration tests against Docker Compose services in CI -
  [pull request #552][pr-552]
- Document the serializer factory - [pull request #593][pr-593]
- Simplify constructors with expression-bodied syntax -
  [pull request #599][pr-599]

[issue-431]: https://github.com/guibranco/CrispyWaffle/issues/431
[issue-432]: https://github.com/guibranco/CrispyWaffle/issues/432
[pr-981]: https://github.com/guibranco/CrispyWaffle/pull/981
[pr-980]: https://github.com/guibranco/CrispyWaffle/pull/980
[pr-1007]: https://github.com/guibranco/CrispyWaffle/pull/1007
[pr-1008]: https://github.com/guibranco/CrispyWaffle/pull/1008
[pr-745]: https://github.com/guibranco/CrispyWaffle/pull/745
[pr-753]: https://github.com/guibranco/CrispyWaffle/pull/753
[pr-971]: https://github.com/guibranco/CrispyWaffle/pull/971
[pr-715]: https://github.com/guibranco/CrispyWaffle/pull/715
[pr-953]: https://github.com/guibranco/CrispyWaffle/pull/953
[pr-901]: https://github.com/guibranco/CrispyWaffle/pull/901
[pr-881]: https://github.com/guibranco/CrispyWaffle/pull/881
[pr-882]: https://github.com/guibranco/CrispyWaffle/pull/882
[pr-667]: https://github.com/guibranco/CrispyWaffle/pull/667
[pr-742]: https://github.com/guibranco/CrispyWaffle/pull/742
[pr-631]: https://github.com/guibranco/CrispyWaffle/pull/631
[pr-548]: https://github.com/guibranco/CrispyWaffle/pull/548
[pr-552]: https://github.com/guibranco/CrispyWaffle/pull/552
[pr-593]: https://github.com/guibranco/CrispyWaffle/pull/593
[pr-599]: https://github.com/guibranco/CrispyWaffle/pull/599
[christopher-conley]: https://github.com/christopher-conley
[ducdaiii]: https://github.com/ducdaiii
[harrykambo]: https://github.com/HarryKambo
[looseling]: https://github.com/Looseling
[mf0zz13]: https://github.com/mf0zz13
[rodri]: https://github.com/rodri-oliveira-dev
[sak3th2004]: https://github.com/Sak3th2004

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
