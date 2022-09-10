# Changelog

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

- Add scheduler feature.
- Add documentation using MkDocs.

## Version 2.1 [2020-09-04]

- Add the Configuration project and package.
- Add Mustache template engine (inspired by Mustache/Handlebars).
- Add Resource template repository.
- Add Elmah & Log4Net projects (logging).
- Basic usage examples.

## Version 2.0 [2020-08-03]

- Removed some application specific patterns from *StringExtensions* class

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