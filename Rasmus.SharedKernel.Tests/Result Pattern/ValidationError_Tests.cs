using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class ValidationError_Tests
    {
        // ── Base ErrorType invariant ──────────────────────────────────────────
        // All factory methods must set ErrorType.Validation on the base class.

        [Theory]
        [MemberData(nameof(AllFactoryInstances))]
        public void All_Factories_Should_Set_ErrorType_To_Validation(ValidationError error)
        {
            Assert.Equal(ErrorType.Validation, error.Type);
        }

        public static IEnumerable<object[]> AllFactoryInstances()
        {
            var ctx = TestErrorContextFactory.Create(fieldName: "Email");

            yield return [ValidationError.Required(ctx)];
            yield return [ValidationError.InvalidFormat(ctx, "name@example.com")];
            yield return [ValidationError.AlreadyExists(ctx)];
            yield return [ValidationError.TooShort(ctx, "3-50")];
            yield return [ValidationError.TooLong(ctx, "3-50")];
            yield return [ValidationError.OutOfRange(ctx, "1-100")];
            yield return [ValidationError.NonMatchingValues(ctx, confirmFieldName: "ConfirmEmail")];
            yield return [ValidationError.Custom(ctx)];
        }

        // ── Required — conditional message (field name vs entity name) ────────
        // The message differs depending on whether FieldName is set on the context.

        [Fact]
        public void Required_Without_FieldName_Should_Mention_EntityName_In_UserMessage()
        {
            var ctx = TestErrorContextFactory.Create(fieldName: null); // EntityName = "User"

            var error = ValidationError.Required(ctx);

            Assert.Contains("User", error.UserMessage);
        }

        [Fact]
        public void Required_With_FieldName_Should_Mention_FieldName_In_UserMessage()
        {
            var ctx = TestErrorContextFactory.Create(fieldName: "Email");

            var error = ValidationError.Required(ctx);

            Assert.Contains("Email", error.UserMessage);
        }

        // ── InvalidFormat — embeds the expected format parameter ──────────────

        [Fact]
        public void InvalidFormat_Should_Include_ExpectedFormat_In_UserMessage()
        {
            var ctx = TestErrorContextFactory.Create(fieldName: "Email");
            const string expectedFormat = "name@example.com";

            var error = ValidationError.InvalidFormat(ctx, expectedFormat);

            Assert.Contains(expectedFormat, error.UserMessage);
        }

        // ── AlreadyExists — mentions the field name ───────────────────────────

        [Fact]
        public void AlreadyExists_Should_Include_FieldName_In_UserMessage()
        {
            var ctx = TestErrorContextFactory.Create(fieldName: "Email");

            var error = ValidationError.AlreadyExists(ctx);

            Assert.Contains("Email", error.UserMessage);
        }

        // ── Range-based factories — embed the range parameter ─────────────────

        [Fact]
        public void TooShort_Should_Include_Range_In_UserMessage()
        {
            var ctx = TestErrorContextFactory.Create(fieldName: "Username");
            const string range = "3-50";

            var error = ValidationError.TooShort(ctx, range);

            Assert.Contains(range, error.UserMessage);
        }

        [Fact]
        public void TooLong_Should_Include_Range_In_UserMessage()
        {
            var ctx = TestErrorContextFactory.Create(fieldName: "Username");
            const string range = "3-50";

            var error = ValidationError.TooLong(ctx, range);

            Assert.Contains(range, error.UserMessage);
        }

        [Fact]
        public void OutOfRange_Should_Include_Range_In_UserMessage()
        {
            var ctx = TestErrorContextFactory.Create(fieldName: "Rating");
            const string range = "1-10";

            var error = ValidationError.OutOfRange(ctx, range);

            Assert.Contains(range, error.UserMessage);
        }

        // ── NonMatchingValues — conditional message ───────────────────────────

        [Fact]
        public void NonMatchingValues_With_Both_Fields_Should_Mention_Both_Fields_In_UserMessage()
        {
            var ctx = TestErrorContextFactory.Create(fieldName: "Password");

            var error = ValidationError.NonMatchingValues(ctx, confirmFieldName: "ConfirmPassword");

            Assert.Contains("Password", error.UserMessage);
            Assert.Contains("ConfirmPassword", error.UserMessage);
        }

        [Fact]
        public void NonMatchingValues_Without_Fields_Should_Use_Fallback_UserMessage()
        {
            var ctx = TestErrorContextFactory.Create(fieldName: null);

            var error = ValidationError.NonMatchingValues(ctx);

            Assert.Contains("do not match", error.UserMessage, StringComparison.OrdinalIgnoreCase);
        }
    }
}
