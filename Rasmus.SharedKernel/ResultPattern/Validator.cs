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


            return !(id is string str && string.IsNullOrWhiteSpace(str) ||
                id is Guid guid && guid == Guid.Empty ||
                id.Equals(default(TKey)));
        }


    }
}
