using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class Result_Tests
    {
        [Fact]
        public void Successful_Result_Should_Have_True_IsSuccess()
        {
            var result = Result.Success();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Successful_Result_Should_Have_False_IsFailure()
        {
            var result = Result.Success();

            Assert.False(result.IsFailure);
        }


    }
}
