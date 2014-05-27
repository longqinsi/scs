using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Hik.Utility
{
    /// <summary>
    ///   Converts Type to its text representation and vice versa. Since v.2.12 all types serialize to the AssemblyQualifiedName.
    ///   Use overloaded constructor to shorten type names.
    /// </summary>
    public sealed class TypeNameConverter
    {
        /// <summary>
        /// Default instance
        /// </summary>
        public static readonly TypeNameConverter Default = new TypeNameConverter(false, false, false);

        private readonly ConcurrentDictionary<Type, string> _typeToNameCache = new ConcurrentDictionary<Type, string>();
        private readonly ConcurrentDictionary<string, Type> _nameToTypeCache = new ConcurrentDictionary<string, Type>();

        /// <summary>
        ///   Version=x.x.x.x will be inserted to the type name
        /// </summary>
        public bool IncludeAssemblyVersion
        {
            get;
            private set;
        }

        /// <summary>
        ///   Culture=.... will be inserted to the type name
        /// </summary>
        public bool IncludeCulture
        {
            get;
            private set;
        }

        /// <summary>
        ///   PublicKeyToken=.... will be inserted to the type name
        /// </summary>
        public bool IncludePublicKeyToken
        {
            get;
            private set;
        }

        /// <summary>
        /// Since v.2.12 as default the type name is equal to Type.AssemblyQualifiedName
        /// </summary>
        public TypeNameConverter()
        {
        }

        /// <summary>
        ///   Some values from the Type.AssemblyQualifiedName can be removed
        /// </summary>
        /// <param name="includeAssemblyVersion"></param>
        /// <param name="includeCulture"></param>
        /// <param name="includePublicKeyToken"></param>
        public TypeNameConverter(bool includeAssemblyVersion, bool includeCulture, bool includePublicKeyToken)
        {
            this.IncludeAssemblyVersion = includeAssemblyVersion;
            this.IncludeCulture = includeCulture;
            this.IncludePublicKeyToken = includePublicKeyToken;
        }

        /// <summary>
        ///   Gives back Type from the text.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public Type ConvertToType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }
            Type type;
            if (_nameToTypeCache.TryGetValue(typeName, out type))
            {
                return type;
            }
            else
            {
                type = Type.GetType(typeName, true);
                if(type != null)
                {
                    _nameToTypeCache.TryAdd(typeName, type);
                }
                return type;
            }
        }

        /// <summary>
        ///   Gives type as text
        /// </summary>
        /// <param name="type"></param>
        /// <returns>string.Empty if the type is null</returns>
        public string ConvertToTypeName(Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }
            string assemblyQualifiedName;
            if (this._typeToNameCache.TryGetValue(type, out assemblyQualifiedName))
            {
                return assemblyQualifiedName;
            }
            assemblyQualifiedName = type.AssemblyQualifiedName;
            if (!this.IncludeAssemblyVersion)
            {
                assemblyQualifiedName = removeAssemblyVersion(assemblyQualifiedName);
            }
            if (!this.IncludeCulture)
            {
                assemblyQualifiedName = removeCulture(assemblyQualifiedName);
            }
            if (!this.IncludePublicKeyToken)
            {
                assemblyQualifiedName = removePublicKeyToken(assemblyQualifiedName);
            }
            this._typeToNameCache.TryAdd(type, assemblyQualifiedName);
            this._nameToTypeCache.TryAdd(assemblyQualifiedName, type);
            return assemblyQualifiedName;
        }

        private static string removeAssemblyVersion(string typename)
        {
            return Regex.Replace(typename, ", Version=\\d+.\\d+.\\d+.\\d+", string.Empty);
        }

        private static string removeCulture(string typename)
        {
            return Regex.Replace(typename, ", Culture=\\w+", string.Empty);
        }

        private static string removePublicKeyToken(string typename)
        {
            return Regex.Replace(typename, ", PublicKeyToken=\\w+", string.Empty);
        }
    }

}
