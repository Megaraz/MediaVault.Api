using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests
{
    public class IsValidId
    {

        #region IsValidId Tests
        [Fact]
        public void IsValidId_Should_Return_False_For_Null_Integer()
        {
            // Arrange
            int? id = null;
            // Act
            var result = Validator.IsValidId(id);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidId_Should_Return_False_For_Negative_Integer()
        {
            // Arrange
            int id = -1;

            // Act
            var result = Validator.IsValidId(id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidId_Should_Return_False_For_Default_Integer()
        {
            // Arrange
            int id = default;
            // Act
            var result = Validator.IsValidId(id);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidId_Should_Return_True_For_Valid_Integer()
        {
            // Arrange
            int id = 123;

            // Act
            var result = Validator.IsValidId(id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidId_Should_Return_False_For_Null_String()
        {
            // Arrange
            string id = null!;

            // Act
            var result = Validator.IsValidId(id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidId_Should_Return_False_For_Empty_String()
        {
            // Arrange
            string id = string.Empty;
            // Act
            var result = Validator.IsValidId(id);

            // Assert
            Assert.False(result);
        }


        [Fact]
        public void IsValidId_Should_Return_False_For_Whitespace_String()
        {
            // Arrange
            string id = "   ";
            // Act
            var result = Validator.IsValidId(id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidId_Should_Return_False_For_Empty_Guid()
        {
            // Arrange
            Guid id = Guid.Empty;
            // Act
            var result = Validator.IsValidId(id);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidId_Should_Return_False_For_Null_Guid()
        {
            // Arrange
            Guid? id = null;
            // Act
            var result = Validator.IsValidId(id);
            // Assert
            Assert.False(result);
        }


        [Fact]
        public void IsValidId_Should_Return_True_For_Valid_Guid()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            // Act
            var result = Validator.IsValidId(id);
            // Assert
            Assert.True(result);
        }


        [Fact]
        public void IsValidId_Should_Return_True_For_Valid_String()
        {
            // Arrange
            string id = "valid-id";
            // Act
            var result = Validator.IsValidId(id);
            // Assert
            Assert.True(result);
        }
        #endregion

    }
}
