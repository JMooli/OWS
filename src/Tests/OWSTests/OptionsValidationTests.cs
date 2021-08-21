﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OWSShared.Extensions;
using System.Linq;
using Xunit;
using System.ComponentModel.DataAnnotations;

namespace OWSTests
{
    public class TestOptions
    {
        public const string SectionName = "TestOptions";
    }

    public class TestOptionsRequired : TestOptions
    {
        [Required(ErrorMessage = "Field Is Required")]
        public string Field { get; set; }
    }

    public class TestOptionsType : TestOptions
    {
        [DataType(DataType.Text)]
        public string Field { get; set; }
    }

    public class TestOptionsRequiredFailed : TestOptions
    {
        [Required(ErrorMessage = "Field Is Required")]
        public string FieldRequired { get; set; }
    }

    public class TestOptionsTypeFailed : TestOptions
    {
        [Required(ErrorMessage = "Field Is Required")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Field is not numeric")]
        public string Field { get; set; }
    }

    public class TestOptionsMutipleErrors : TestOptions
    {
        [Required(ErrorMessage = "Field1 Is Required")]
        public string Field1 { get; set; }

        [Required(ErrorMessage = "Field2 Is Required")]
        public string Field2 { get; set; }
    }

    public class OptionsValidationTests
    {
        private readonly IConfigurationRoot _configuration;
        private readonly ServiceCollection _serviceCollection;
        private IServiceProvider _serviceProvider;

        public OptionsValidationTests()
        {
            var configBuilder = new ConfigurationBuilder().AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            _configuration = configBuilder.Build();
            _serviceCollection = new ServiceCollection();
            _serviceCollection.AddOptions();
        }

        [Fact]
        public void Option_Field_Is_Required()
        {
            IEnumerable<string> errors = new List<string> { };
            _serviceCollection.Configure<TestOptionsRequired>(_configuration.GetSection(TestOptions.SectionName)).PostConfigure<TestOptionsRequired>(settings =>
            {
                errors = settings.ValidationErrors().ToArray();
            });
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            IOptions<TestOptionsRequired> testOptionsRequired = _serviceProvider.GetService<IOptions<TestOptionsRequired>>();

            Assert.NotNull(testOptionsRequired.Value.Field);
            Assert.Equal("Value", testOptionsRequired.Value.Field);
            Assert.Empty(errors);
        }

        [Fact]
        public void Option_Field_Is_Of_Type()
        {
            IEnumerable<string> errors = new List<string> { };
            _serviceCollection.Configure<TestOptionsType>(_configuration.GetSection(TestOptions.SectionName)).PostConfigure<TestOptionsType>(settings =>
            {
                errors = settings.ValidationErrors().ToArray();
            });
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            IOptions<TestOptionsType> testOptionsType = _serviceProvider.GetService<IOptions<TestOptionsType>>();

            Assert.NotNull(testOptionsType.Value.Field);
            Assert.Equal("Value", testOptionsType.Value.Field);
            Assert.Empty(errors);
        }

        [Fact]
        public void Option_Field_Is_Required_Failed()
        {
            IEnumerable<string> errors = new List<string> { };
            _serviceCollection.Configure<TestOptionsRequiredFailed>(_configuration.GetSection(TestOptions.SectionName)).PostConfigure<TestOptionsRequiredFailed>(settings =>
            {
                errors = settings.ValidationErrors().ToArray();
            });
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            IOptions<TestOptionsRequiredFailed> testOptionsMissing = _serviceProvider.GetService<IOptions<TestOptionsRequiredFailed>>();

            Assert.Null(testOptionsMissing.Value.FieldRequired);
            Assert.Single(errors);
            Assert.Equal("Field Is Required", errors.First());
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void Option_Field_Is_Of_Type_Failed()
        {
            IEnumerable<string> errors = new List<string> { };
            _serviceCollection.Configure<TestOptionsTypeFailed>(_configuration.GetSection(TestOptions.SectionName)).PostConfigure<TestOptionsTypeFailed>(settings =>
            {
                errors = settings.ValidationErrors().ToArray();
            });
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            IOptions<TestOptionsTypeFailed> testOptionsTypeFailed = _serviceProvider.GetService<IOptions<TestOptionsTypeFailed>>();

            Assert.Equal("Value", testOptionsTypeFailed.Value.Field);
            Assert.Single(errors);
            Assert.Equal("Field is not numeric", errors.First());
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void Option_Field_Mutiple_Errors()
        {
            IEnumerable<string> errors = new List<string> { };
            _serviceCollection.Configure<TestOptionsMutipleErrors>(_configuration.GetSection(TestOptions.SectionName)).PostConfigure<TestOptionsMutipleErrors>(settings =>
            {
                errors = settings.ValidationErrors().ToArray();
            });
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            IOptions<TestOptionsMutipleErrors> testOptionsMutipleErrors = _serviceProvider.GetService<IOptions<TestOptionsMutipleErrors>>();

            Assert.Null(testOptionsMutipleErrors.Value.Field1);
            Assert.Null(testOptionsMutipleErrors.Value.Field2);
            Assert.Equal(2, errors.Count());
            Assert.Equal("Field1 Is Required", errors.ElementAt(0));
            Assert.Equal("Field2 Is Required", errors.ElementAt(1));
            Assert.NotEmpty(errors);
        }
    }
}
