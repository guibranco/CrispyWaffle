# Template rendering

The template rendering module fills text templates with data. It has two engines: `MustacheTemplateRender`, a small regex-based engine with a Mustache-like syntax for plain text and HTML (for example, email bodies), and `XmlTemplateRender`, which applies an XSLT stylesheet to an XML document. It also has a template repository abstraction, with an implementation that reads templates from `.resx` resources.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.TemplateRendering.Engines`, `CrispyWaffle.TemplateRendering.Repositories` |

## How it works

Both engines implement `ITemplateRender.Render(string template, object data)` and return the rendered string. There are two implementations, so register the one you want with the service locator instead of relying on auto-registration:

```csharp
ServiceLocator.Register<ITemplateRender, MustacheTemplateRender>();
```

### Mustache engine pipeline

`MustacheTemplateRender.Render` runs these steps in this order. Each step works on the output of the previous one:

1. Read all public properties of `data` (instance and static) into a dictionary keyed by the lower-case property name. Fields are not read.
2. Evaluate conditional blocks `{{#name}}...{{/name}}`.
3. Expand `{{#with name}}...{{/with}}` blocks.
4. Expand `{{#each name}}...{{/each}}` loops.
5. Replace every remaining `{{...}}` with a top-level property value.

Rules that apply to all steps:

- Tag keywords and property names are case-insensitive. `{{NAME}}`, `{{Name}}` and `{{name}}` are the same.
- Only top-level properties of `data` are available. There is no dotted path syntax (`{{address.city}}` does not work). Use `{{#with}}` to reach into a nested object.
- Whitespace inside the braces is not trimmed. `{{ name }}` looks for a property called `" name "` and renders as an empty string.
- Values are inserted with `ToString()`, using the current culture. Nothing is HTML-escaped.
- A placeholder whose name does not match any property renders as an empty string. This also removes any leftover or unknown tags.
- A placeholder that matches a property whose value is `null` throws `NullReferenceException`. Make sure every referenced property has a value, or wrap it in a conditional.
- `Render` throws `ArgumentNullException` when `data` is `null`.
- The engine stores the parsed properties in an instance field during `Render`, so do not share one instance across threads at the same time.

## Template syntax (Mustache engine)

### Placeholders

```text
Hello {{name}}!
```

`{{name}}` is replaced with `data.Name.ToString()`. Unknown names produce an empty string.

### Conditionals

```text
{{#name}}shown when name is truthy{{/name}}
{{#name}}shown when truthy{{#else}}shown otherwise{{/name}}
```

- The closing tag must repeat the opening name exactly (case-insensitive): `{{#company}}...{{/company}}`.
- `{{#else}}` splits the block. There is no separate closing tag for it; the else part ends at `{{/name}}`. A stray `{{/else}}` is harmless, because step 5 removes it as an unknown placeholder.
- When the condition is false and there is no `{{#else}}`, the whole block is removed.
- Conditions are evaluated against the top-level properties only, even when the block sits inside an `{{#each}}` or `{{#with}}` body.

A property is truthy when:

| Value | Result |
|---|---|
| Property does not exist, or value is `null` | False |
| `bool` | The value itself |
| `string` | True if it is not empty or whitespace |
| Any other value type (`int`, `DateTime`, enums, `Guid`, ...), `decimal`, `DateTimeOffset`, `TimeSpan` | Always true, including `0` and default values |
| A reference type with a public parameterless constructor | True |
| A reference type without a public parameterless constructor (arrays, anonymous types, many records) | False |

Nested conditionals are not evaluated. In `{{#a}}A{{#b}}B{{/b}}{{/a}}`, when `a` is true the inner `{{#b}}` and `{{/b}}` tags are removed as unknown placeholders and `B` is always rendered, even when `b` is false. Repeating the same block side by side (`{{#a}}1{{/a}}-{{#a}}2{{/a}}`) works.

### `with` blocks

```text
{{#with address}}{{street}}, {{number}} - {{city}}{{/with}}
```

The body is rendered by a new `MustacheTemplateRender` using the `address` object as data, so it supports the full syntax (conditionals, loops, placeholders) against that object's properties. Top-level properties are not available inside the body.

- If the property does not exist or its value is a simple type (value types, `string`, `decimal`, `DateTime`, ...), the whole block is removed.
- If the property value is `null`, `Render` throws `NullReferenceException`.
- Matching is non-greedy, so a `{{#with}}` nested directly inside another `{{#with}}` closes at the first `{{/with}}`. Do not nest `with` blocks.

### Loops

```text
<ul>{{#each items}}<li>{{this}}</li>{{/each}}</ul>
<ul>{{#each people}}<li>{{firstName}} {{lastName}}</li>{{/each}}</ul>
```

The property must be an array. Other collections, such as `List<T>`, are not expanded: the block is removed. A missing property also removes the block, and a `null` value throws `NullReferenceException`.

- Array of strings: the body is repeated for each item and `{{this}}` is replaced with the item. Other placeholders in the body are left for step 5, so they render top-level properties. Item values go through `Regex.Replace`, so `$` sequences in an item (such as `$0`) are treated as substitution patterns.
- Array of other simple types (`int[]`, `DateTime[]`, ...): throws `InvalidCastException`. Convert to `string[]` first.
- Array of objects (reference types): for each item, every `{{name}}` in the body is replaced with that item's property value. Unknown names, `{{this}}` and top-level properties render as empty strings. A `null` item property throws `NullReferenceException`. Conditionals and nested loops inside the body are not evaluated per item.
- Like `with`, loops do not nest: the first `{{/each}}` closes the block.

### Not supported

- Partials and imports. An `{{>import ...}}` tag is removed as an unknown placeholder.
- Comments, triple braces, HTML escaping, inverted sections (`{{^name}}`), dotted names.

## XSLT engine

`XmlTemplateRender.Render(template, data)` treats `template` as an XSLT stylesheet and `data` as an `XNode` (`XDocument` or `XElement`). It loads the stylesheet with `XslCompiledTransform` (default settings), transforms `data.CreateReader()` with no XSLT arguments, and returns the output written by an `XmlTextWriter`.

- Throws `ArgumentNullException` when `template` or `data` is `null`.
- Throws `InvalidCastException` when `data` is not an `XNode`.
- Stylesheet load or transformation errors (`XsltException`, `XmlException`) propagate to the caller.

## Getting started

```csharp
using System;
using CrispyWaffle.Composition;
using CrispyWaffle.TemplateRendering.Engines;

ServiceLocator.Register<ITemplateRender, MustacheTemplateRender>();

var template = "Hello {{name}}{{#company}}, from {{company}}{{/company}}!";
var data = new { Name = "Jane Doe", Company = "Contoso" };

var result = ServiceLocator.Resolve<ITemplateRender>().Render(template, data);

Console.WriteLine(result); // Hello Jane Doe, from Contoso!
```

## Reference

### `ITemplateRender`

`public interface ITemplateRender` — contract for a template engine.

| Member | Description |
|---|---|
| `string Render(string template, object data)` | Renders `template` with values from `data` and returns the result. |

### `MustacheTemplateRender`

`public class MustacheTemplateRender : ITemplateRender` — regex-based engine with the syntax described in [Template syntax](#template-syntax-mustache-engine).

| Member | Description |
|---|---|
| `MustacheTemplateRender()` | Default constructor. |
| `Render(string template, object data)` | Renders the template. Throws `ArgumentNullException` when `data` is `null`. Throws `NullReferenceException` for `null` values referenced by placeholders, `with` blocks or loops, and `InvalidCastException` for loops over non-string simple-type arrays. Each regex has a 5-second match timeout, so a pathological template can throw `RegexMatchTimeoutException`. |

### `XmlTemplateRender`

`public sealed class XmlTemplateRender : ITemplateRender` — applies an XSLT stylesheet to XML data. See [XSLT engine](#xslt-engine).

| Member | Description |
|---|---|
| `XmlTemplateRender()` | Default constructor. |
| `Render(string template, object data)` | `template` is the XSLT source. `data` must be an `XNode`. Returns the transformation output. |

### `ITemplateRepository`

`public interface ITemplateRepository` — stores and returns templates by name.

| Member | Description |
|---|---|
| `void RegisterTemplate(string name, string content)` | Adds a template. Behavior for duplicate names is up to the implementation. |
| `string GetTemplateByName(string name)` | Returns the template content. The interface documents `null` for unknown names, but implementations may throw instead. |

### `ResourceTemplateRepository`

`public sealed class ResourceTemplateRepository : ITemplateRepository` — read-only repository backed by a `ResourceManager` (for example, the generated `Resources.ResourceManager` of a `.resx` file).

| Member | Description |
|---|---|
| `ResourceTemplateRepository(ResourceManager manager)` | Constructor. |
| `RegisterTemplate(string name, string content)` | Always throws `InvalidOperationException` (`The registration of resource at runtime is not allowed.`). |
| `GetTemplateByName(string name)` | Returns `manager.GetString(name, CultureInfo.CurrentCulture)`. Throws `InvalidOperationException` (`Unable to find the template '{name}' in the repository '...'.`) when the resource is not found. |

## Examples

### Conditional with else

```csharp
using CrispyWaffle.TemplateRendering.Engines;

var render = new MustacheTemplateRender();
var template =
    "The name is: {{#name}}{{name}}{{#else}}empty{{/name}}"
    + "{{#company}} works at: {{company}}{{#else}} doesn't work{{/company}}";

// "The name is: Jane doesn't work"
var result = render.Render(template, new { Name = "Jane" });
```

### Loops over strings and objects

```csharp
using CrispyWaffle.TemplateRendering.Engines;

var render = new MustacheTemplateRender();

// "<ul><li>One</li><li>Two</li></ul>"
var list = render.Render(
    "<ul>{{#each li}}<li>{{this}}</li>{{/each}}</ul>",
    new { Li = new[] { "One", "Two" } }
);

// "<ul><li>John Doe</li><li>Jane Doe</li></ul>"
var people = render.Render(
    "<ul>{{#each person}}<li>{{FirstName}} {{LastName}}</li>{{/each}}</ul>",
    new
    {
        Person = new[]
        {
            new { FirstName = "John", LastName = "Doe" },
            new { FirstName = "Jane", LastName = "Doe" },
        },
    }
);
```

### Nested object with `with`

```csharp
using CrispyWaffle.TemplateRendering.Engines;

var render = new MustacheTemplateRender();
var template = "{{NAME}} lives in {{#with ADDRESS}}{{STREET}}, {{NUMBER}} at {{CITY}}{{/with}}";
var data = new
{
    Name = "John Doe",
    Address = new { Street = "Avenida Paulista", Number = 1000, City = "São Paulo" },
};

// "John Doe lives in Avenida Paulista, 1000 at São Paulo"
var result = render.Render(template, data);
```

### Templates from resources

`Templates` is the class generated for a `Templates.resx` file in your project.

```csharp
using CrispyWaffle.TemplateRendering.Engines;
using CrispyWaffle.TemplateRendering.Repositories;

ITemplateRepository repository = new ResourceTemplateRepository(Templates.ResourceManager);
ITemplateRender render = new MustacheTemplateRender();

var body = render.Render(
    repository.GetTemplateByName("WelcomeEmail"),
    new { Name = "Jane" }
);
```

### XSLT

```csharp
using System.Xml.Linq;
using CrispyWaffle.TemplateRendering.Engines;

var xslt = """
    <xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
      <xsl:output method="text" />
      <xsl:template match="/customer">Hello <xsl:value-of select="name" />!</xsl:template>
    </xsl:stylesheet>
    """;

var data = XDocument.Parse("<customer><name>Jane</name></customer>");

var result = new XmlTemplateRender().Render(xslt, data); // "Hello Jane!"
```

## See also

- [Service locator](service-locator.md)
- [Serialization](serialization.md)
