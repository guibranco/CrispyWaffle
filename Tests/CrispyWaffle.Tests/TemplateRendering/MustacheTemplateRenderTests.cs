using System.Collections.Generic;
using CrispyWaffle.TemplateRendering.Engines;
using FluentAssertions;
using Xunit;

namespace CrispyWaffle.Tests.TemplateRendering;

public sealed class MustacheTemplateRenderTests
{
    [Fact]
    public void RenderNullPropertyValuesReturnsEmptyStrings()
    {
        var renderer = new MustacheTemplateRender();

        renderer.Render("x{{n}}", new { N = (string)null }).Should().Be("x");
        renderer.Render("{{#with n}}{{x}}{{/with}}", new { N = (object)null }).Should().BeEmpty();
        renderer.Render("{{#each n}}{{this}}{{/each}}", new { N = (string[])null }).Should().BeEmpty();
        renderer
            .Render("{{#each items}}{{Name}}{{/each}}", new { Items = new[] { new { Name = (string)null } } })
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void RenderSimpleLoopHandlesNonStringArraysAndLists()
    {
        var renderer = new MustacheTemplateRender();

        renderer.Render("{{#each n}}{{this}}{{/each}}", new { N = new[] { 1, 2 } }).Should().Be("12");
        renderer
            .Render("{{#each n}}{{this}}{{/each}}", new { N = new List<string> { "a", "b" } })
            .Should()
            .Be("ab");
    }

    [Fact]
    public void RenderEvaluatesNestedConditionals()
    {
        var renderer = new MustacheTemplateRender();

        renderer
            .Render("{{#a}}A{{#b}}B{{/b}}{{/a}}", new { A = true, B = false })
            .Should()
            .Be("A");
        renderer
            .Render("{{#a}}A{{#b}}B{{#else}}C{{/b}}{{/a}}", new { A = true, B = false })
            .Should()
            .Be("AC");
    }

    [Fact]
    public void RenderSimpleLoopPreservesDollarSequencesLiterally()
    {
        var renderer = new MustacheTemplateRender();

        renderer
            .Render("{{#each n}}<{{this}}>{{/each}}", new { N = new[] { "$0", "$$", "b" } })
            .Should()
            .Be("<$0><$$><b>");
    }

    [Fact]
    public void RenderTreatsNonNullObjectsAndNonEmptyCollectionsAsTruthy()
    {
        var renderer = new MustacheTemplateRender();

        renderer
            .Render("{{#n}}yes{{#else}}no{{/n}}", new { N = new { X = 1 } })
            .Should()
            .Be("yes");
        renderer
            .Render("{{#n}}yes{{#else}}no{{/n}}", new { N = new[] { "x" } })
            .Should()
            .Be("yes");
        renderer
            .Render("{{#n}}yes{{#else}}no{{/n}}", new { N = System.Array.Empty<string>() })
            .Should()
            .Be("no");
    }
}
