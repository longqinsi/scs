using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class ScsExtensions
    {
        /// <summary>Indicates whether an object of a given type can be assigned a null value to</summary>
        /// <param name="this">The type to check.</param>
        /// <returns>
        /// true:Objects of this type can be assigned a null value to.
        /// false:Objects of this type cannot be assigned a null value to.
        /// </returns>
        public static bool IsNullable(this Type @this)
        {
            if (@this == null)
            {
                return false;
            }
            Type type = @this;
            if (type.HasElementType)
            {
                type = type.GetElementType();
            }
            if (type.IsClass || type.IsInterface)
            {
                return true;
            }
            if (!type.IsGenericType || type.IsGenericTypeDefinition)
            {
                return false;
            }
            else
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
