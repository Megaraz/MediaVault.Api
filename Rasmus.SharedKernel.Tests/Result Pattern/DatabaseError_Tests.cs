using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class DatabaseError_Tests
    {
        // ── Base ErrorType invariant ──────────────────────────────────────────
        // All factory methods must set ErrorType.Database on the base class.

        [Theory]
        [MemberData(nameof(AllFactoryInstances))]
        public void All_Factories_Should_Set_ErrorType_To_Database(DatabaseError error)
        {
            Assert.Equal(ErrorType.Database, error.Type);
        }

        public static IEnumerable<object[]> AllFactoryInstances()
        {
            var ctx = TestErrorContextFactory.Create();
            var ex = new Exception("db error");

            yield return [DatabaseError.SaveChangesFailure(ctx, ex)];
            yield return [DatabaseError.QueryFailure(ctx, ex)];
            yield return [DatabaseError.ConcurrencyFailure(ctx, ex)];
            yield return [DatabaseError.UnexpectedFailure(ctx, ex)];
        }

        // ── DatabaseErrorType is set correctly ────────────────────────────────

        [Fact]
        public void SaveChangesFailure_Should_Set_Correct_DatabaseErrorType()
        {
            var ctx = TestErrorContextFactory.Create();
            var error = DatabaseError.SaveChangesFailure(ctx, new Exception());

            Assert.Equal(DatabaseErrorType.SaveChangesFailure, error.DatabaseErrorType);
        }

        [Fact]
        public void QueryFailure_Should_Set_Correct_DatabaseErrorType()
        {
            var ctx = TestErrorContextFactory.Create();
            var error = DatabaseError.QueryFailure(ctx, new Exception());

            Assert.Equal(DatabaseErrorType.QueryFailure, error.DatabaseErrorType);
        }

        [Fact]
        public void ConcurrencyFailure_Should_Set_Correct_DatabaseErrorType()
        {
            var ctx = TestErrorContextFactory.Create();
            var error = DatabaseError.ConcurrencyFailure(ctx, new Exception());

            Assert.Equal(DatabaseErrorType.ConcurrencyFailure, error.DatabaseErrorType);
        }

        [Fact]
        public void UnexpectedFailure_Should_Set_Correct_DatabaseErrorType()
        {
            var ctx = TestErrorContextFactory.Create();
            var error = DatabaseError.UnexpectedFailure(ctx, new Exception());

            Assert.Equal(DatabaseErrorType.UnexpectedFailure, error.DatabaseErrorType);
        }

        // ── Exception is always attached ──────────────────────────────────────
        // All database errors require an Exception so callers can inspect the cause.

        [Fact]
        public void SaveChangesFailure_Should_Attach_Exception()
        {
            var ctx = TestErrorContextFactory.Create();
            var exception = new InvalidOperationException("constraint violation");

            var error = DatabaseError.SaveChangesFailure(ctx, exception);

            Assert.Equal(exception, error.Exception);
        }

        [Fact]
        public void QueryFailure_Should_Attach_Exception()
        {
            var ctx = TestErrorContextFactory.Create();
            var exception = new TimeoutException("query timed out");

            var error = DatabaseError.QueryFailure(ctx, exception);

            Assert.Equal(exception, error.Exception);
        }

        [Fact]
        public void ConcurrencyFailure_Should_Attach_Exception()
        {
            var ctx = TestErrorContextFactory.Create();
            var exception = new Exception("concurrency conflict");

            var error = DatabaseError.ConcurrencyFailure(ctx, exception);

            Assert.Equal(exception, error.Exception);
        }

        [Fact]
        public void UnexpectedFailure_Should_Attach_Exception()
        {
            var ctx = TestErrorContextFactory.Create();
            var exception = new Exception("unexpected");

            var error = DatabaseError.UnexpectedFailure(ctx, exception);

            Assert.Equal(exception, error.Exception);
        }

        // ── Descriptions mention the entity name ──────────────────────────────
        // Entity name is the primary diagnostic identifier in the message.

        [Fact]
        public void SaveChangesFailure_Description_Should_Contain_EntityName()
        {
            var ctx = TestErrorContextFactory.Create(); // EntityName = "User"

            var error = DatabaseError.SaveChangesFailure(ctx, new Exception());

            Assert.Contains("User", error.UserMessage);
        }

        [Fact]
        public void QueryFailure_Description_Should_Contain_EntityName()
        {
            var ctx = TestErrorContextFactory.Create();

            var error = DatabaseError.QueryFailure(ctx, new Exception());

            Assert.Contains("User", error.UserMessage);
        }
    }
}
