using System;
using System.Collections.Generic;
using System.Linq;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class ValidatorExtensions_RequiredFieldsAreNullOrWhiteSpace_Tests
    {
        [Fact]
        public void Should_Return_True_And_Errors_For_Null_Collection()
        {
            IEnumerable<(string FieldName, string Value)> requiredValues = null!;
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var errors);

            Assert.True(result);
            Assert.NotNull(errors);
            Assert.NotEmpty(errors);
            Assert.All(errors, error => ValidationErrorAssert.IsRequired(error, entityName: "User"));
        }

        [Fact]
        public void Should_Return_True_And_Errors_When_All_Fields_Are_Null()
        {
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", null!),
                ("Email", null!)
            };
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var errors);
            var errorList = errors.ToList();

            Assert.True(result);
            Assert.Equal(2, errorList.Count);
            Assert.Collection(
                errorList,
                error => ValidationErrorAssert.IsRequired(error, fieldName: "Username", entityName: "User"),
                error => ValidationErrorAssert.IsRequired(error, fieldName: "Email", entityName: "User"));
        }

        [Fact]
        public void Should_Return_True_When_All_Fields_Are_Empty()
        {
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", string.Empty),
                ("Email", string.Empty)
            };
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var errors);

            Assert.True(result);
            Assert.All(errors, error => ValidationErrorAssert.IsRequired(error, entityName: "User"));
        }

        [Fact]
        public void Should_Return_True_When_All_Fields_Are_Whitespace()
        {
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", "   "),
                ("Email", "   ")
            };
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var errors);

            Assert.True(result);
            Assert.All(errors, error => ValidationErrorAssert.IsRequired(error, entityName: "User"));
        }

        [Fact]
        public void Should_Return_True_And_Single_Error_When_One_Field_Is_Invalid()
        {
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", "validuser"),
                ("Email", string.Empty)
            };
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var errors);
            var errorList = errors.ToList();

            Assert.True(result);
            Assert.Single(errorList);
            ValidationErrorAssert.IsRequired(errorList[0], fieldName: "Email", entityName: "User");
        }

        [Fact]
        public void Should_Return_False_And_No_Errors_When_All_Fields_Are_Valid()
        {
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", "validuser"),
                ("Email", "test@example.com")
            };
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var errors);

            Assert.False(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_Return_False_And_No_Errors_For_Empty_Collection()
        {
            var requiredValues = new List<(string FieldName, string Value)>();
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var errors);

            Assert.False(result);
            Assert.Empty(errors);
        }
    }
}
