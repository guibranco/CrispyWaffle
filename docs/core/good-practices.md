# Good practices

The `CrispyWaffle.GoodPractices` namespace holds a few shared exception contracts. Two interfaces describe exceptions that carry the request and response of a failed remote call, so logging and error-handling code can read them without knowing the concrete exception type. One concrete exception reports an invalid telephone number.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.GoodPractices` |

## How it works

`IRestException` and `IXmlServiceException` are contracts only: the library does not implement or catch them. Implement one on your own exception class when it wraps an HTTP or XML/SOAP call, and check for the interface where you log or report errors.

`InvalidTelephoneNumberException` is thrown by `ParseBrazilianPhoneNumber` in the `CrispyWaffle.I18n.PtBr` package (see [I18n PtBr](../packages/i18n-ptbr.md)). Its message contains the input with every non-digit character removed.

## Getting started

```csharp
using System;
using CrispyWaffle.GoodPractices;

try
{
    throw new PaymentGatewayException("POST /payments {\"amount\":10}", "HTTP 502 Bad Gateway");
}
catch (Exception e) when (e is IRestException rest)
{
    Console.WriteLine($"Request: {rest.Request}");
    Console.WriteLine($"Response: {rest.Response}");
}

public sealed class PaymentGatewayException : Exception, IRestException
{
    public PaymentGatewayException(string request, string response)
        : base("The payment gateway call failed.")
    {
        Request = request;
        Response = response;
    }

    public string Request { get; }

    public string Response { get; }
}
```

## Reference

### `IRestException`

`public interface IRestException` — an exception raised during an HTTP REST request/response cycle.

| Member | Description |
|---|---|
| `string Request` | The request that caused the failure, such as the URL, headers or body. |
| `string Response` | The response received, such as the status code and error body. |

### `IXmlServiceException`

`public interface IXmlServiceException` — an exception raised during an XML service call. The source file is `IXMLServiceException.cs`, but the type name is `IXmlServiceException`.

| Member | Description |
|---|---|
| `XmlDocument Request` | The XML document that was sent. |
| `XmlDocument Response` | The XML document that was received. |

### `InvalidTelephoneNumberException`

`[Serializable] public class InvalidTelephoneNumberException : Exception` — thrown when a value is not a valid telephone number.

The message is "The value '{telephoneNumber}' isn't a valid telephone number.".

| Member | Description |
|---|---|
| `InvalidTelephoneNumberException(string telephoneNumber)` | Creates the exception for the given number. |
| `protected InvalidTelephoneNumberException(SerializationInfo info, StreamingContext context)` | Serialization constructor. |

## Examples

### Catch an invalid Brazilian phone number

`ParseBrazilianPhoneNumber` throws `InvalidTelephoneNumberException` with the digits of the input. This pattern comes from the `CrispyWaffle.I18n.PtBr` test suite.

```csharp
using System;
using CrispyWaffle.GoodPractices;
using CrispyWaffle.I18n.PtBr;

try
{
    var phone = "abc123def456".ParseBrazilianPhoneNumber();
}
catch (InvalidTelephoneNumberException e)
{
    Console.WriteLine(e.Message); // The value '123456' isn't a valid telephone number.
}
```

### Report the payloads of a failed XML service call

```csharp
using System;
using System.Xml;
using CrispyWaffle.GoodPractices;

var request = new XmlDocument();
request.LoadXml("<getQuote><symbol>ABC</symbol></getQuote>");

var response = new XmlDocument();
response.LoadXml("<fault><code>404</code></fault>");

try
{
    throw new QuoteServiceException(request, response);
}
catch (Exception e) when (e is IXmlServiceException xml)
{
    Console.WriteLine(xml.Request.OuterXml);
    Console.WriteLine(xml.Response.OuterXml);
}

public sealed class QuoteServiceException : Exception, IXmlServiceException
{
    public QuoteServiceException(XmlDocument request, XmlDocument response)
        : base("The quote service returned a fault.")
    {
        Request = request;
        Response = response;
    }

    public XmlDocument Request { get; }

    public XmlDocument Response { get; }
}
```

## See also

- [I18n PtBr](../packages/i18n-ptbr.md)
- [Logging](logging.md)
- [Validations](validations.md)
