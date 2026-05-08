using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.ResultPattern
{
    public static class Validator
    {
        public static bool IsValidId<TKey>(TKey id)
        {
            if (id is null)
                return false;


            return !((id is string str && string.IsNullOrWhiteSpace(str)) ||
                     (id is Guid guid && guid == Guid.Empty) ||
                     (id is int intId && intId <= 0) ||
                     id.Equals(default(TKey)));
        }

        public static void ValidateAndAdjustPaginationParameters(ref int pageNumber, ref int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1; // Default to page 1 if the provided page number is too low
            if (pageSize < 1)
                pageSize = 1; // Default to a minimum page size of 1 if the provided page size is too low
        }


    }
}
