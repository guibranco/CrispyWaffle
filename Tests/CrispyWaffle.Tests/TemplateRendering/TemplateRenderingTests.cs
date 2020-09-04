// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-04-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-04-2020
// ***********************************************************************
// <copyright file="TemplateRenderingTests.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using CrispyWaffle.Composition;
using CrispyWaffle.TemplateRendering.Engines;
using System;
using Xunit;

namespace CrispyWaffle.Tests.TemplateRendering
{
    /// <summary>
    /// Class TemplateRenderingTests.
    /// Implements the <see cref="Xunit.IClassFixture{BootstrapFixture}" />
    /// </summary>
    /// <seealso cref="Xunit.IClassFixture{BootstrapFixture}" />
    public class TemplateRenderingTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateRenderingTests"/> class.
        /// </summary>
        public TemplateRenderingTests()
        {
            ServiceLocator.Register<ITemplateRender, MustacheTemplateRender>();
        }

        /// <summary>
        /// Defines the test method ValidateSimpleTemplate.
        /// </summary>
        [Fact]
        public void ValidateSimpleTemplate()
        {
            var template = "Hello world, {{NAME}}!";
            var data = new { Name = "Guilherme Branco Stracini" };
            var expected = $"Hello world, {data.Name}!";

            var result = ServiceLocator.Resolve<ITemplateRender>().Render(template, data);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Defines the test method ValidateConditionalTemplate.
        /// </summary>
        [Fact]
        public void ValidateConditionalTemplate()
        {
            var template = "Today is {{DATE}}{{#NAME}} and {{NAME}} is working{{/NAME}}{{#COMPANY}} at {{COMPANY}}{{/COMPANY}}{{#EMPTY}}to be removed{{/EMPTY}}";
            var data = new { Date = DateTime.Now.DayOfWeek, Company = "Guilherme Branco Stracini ME." };
            var expected = $"Today is {data.Date} at {data.Company}";

            var result = ServiceLocator.Resolve<ITemplateRender>().Render(template, data);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Defines the test method ValidateInnerConditionalTemplate.
        /// </summary>
        [Fact]
        public void ValidateInnerConditionalTemplate()
        {
            var template = "Today is {{#DATE}}{{DATE}}{{#NAME}} and {{NAME}} is working{{/NAME}}{{/DATE}}{{#COMPANY}} at {{COMPANY}}{{#DATE}} on {{DATE}}{{/DATE}}{{/COMPANY}}{{#EMPTY}}to be removed{{/EMPTY}}";
            var data = new { Date = DateTime.Now.DayOfWeek, Name = "Guilherme Branco Stracini" };
            var expected = $"Today is {data.Date} and {data.Name} is working";

            var result = ServiceLocator.Resolve<ITemplateRender>().Render(template, data);

            Assert.Equal(expected, result);
        }


        /// <summary>
        /// Defines the test method ValidateConditionalNegativeTemplate.
        /// </summary>
        [Fact]
        public void ValidateConditionalNegativeTemplate()
        {
            var template = "The name is: {{#name}}{{name}}{{#else}}empty{{/name}}{{#company}} works at: {{company}}{{#else}} don't work{{/company}}";
            var dataOne = new
            {
                Name = "Guilherme",
                Company = "Guilherme Branco Stracini ME."
            };
            var dataTwo = new { dataOne.Company };
            var dataThree = new { dataOne.Name };

            var expectedOne = $"The name is: {dataOne.Name} works at: {dataOne.Company}";
            var expectedTwo = $"The name is: empty works at: {dataTwo.Company}";
            var expectedThree = $"The name is: {dataThree.Name} don't work";

            var render = ServiceLocator.Resolve<ITemplateRender>();

            var resultOne = render.Render(template, dataOne);
            var resultTwo = render.Render(template, dataTwo);
            var resultThree = render.Render(template, dataThree);

            Assert.Equal(expectedOne, resultOne);
            Assert.Equal(expectedTwo, resultTwo);
            Assert.Equal(expectedThree, resultThree);
        }

        /// <summary>
        /// Defines the test method ValidateLoopTemplate.
        /// </summary>
        [Fact]
        public void ValidateLoopTemplate()
        {
            var template = "<ul>{{#each li}}<li>{{this}}</li>{{/each}}</ul>";
            var data = new { Li = new[] { "One", "Two", "Three", "Four", "Five" } };
            var expected = $"<ul><li>{data.Li[0]}</li><li>{data.Li[1]}</li><li>{data.Li[2]}</li><li>{data.Li[3]}</li><li>{data.Li[4]}</li></ul>";

            var result = ServiceLocator.Resolve<ITemplateRender>().Render(template, data);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Defines the test method ValidateComplexLoopTemplate.
        /// </summary>
        [Fact]
        public void ValidateComplexLoopTemplate()
        {
            var template = "<ul>{{#each person}}<li>{{FirstName}} {{LastName}}</li>{{/each}}</ul>";
            var data = new
            {
                Person = new[]
                {
                    new
                    {
                        FirstName = "John",
                        LastName = "Doe"
                    },
                    new
                    {
                        FirstName = "Jane",
                        LastName = "Doe"
                    },
                    new
                    {
                        FirstName = "Joe",
                        LastName = "Public"
                    },
                }
            };
            var expected = $"<ul><li>{data.Person[0].FirstName} {data.Person[0].LastName}</li><li>{data.Person[1].FirstName} {data.Person[1].LastName}</li><li>{data.Person[2].FirstName} {data.Person[2].LastName}</li></ul>";

            var result = ServiceLocator.Resolve<ITemplateRender>().Render(template, data);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Defines the test method ValidateWithTemplate.
        /// </summary>
        [Fact]
        public void ValidateWithTemplate()
        {
            var template = "{{NAME}} lives in {{#with ADDRESS}}{{STREET}}, {{NUMBER}} at {{CITY}}{{/with}}";
            var data = new
            {
                Name = "John Doe",
                Address = new
                {
                    Street = "Avenida Paulista",
                    Number = 1000,
                    City = "São Paulo"
                }
            };
            var expected = $"{data.Name} lives in {data.Address.Street}, {data.Address.Number} at {data.Address.City}";

            var result = ServiceLocator.Resolve<ITemplateRender>().Render(template, data);

            Assert.Equal(expected, result);
        }

    }
}
