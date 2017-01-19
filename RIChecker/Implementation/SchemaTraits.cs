using System;
using System.Collections.Generic;
using System.Linq;

using RIChecker.Interfaces;
using RIChecker.Model;

namespace RIChecker.Implementation
{
    public static class SchemaTraits
    {
        // As per DCI paradigm
        // an ISchema gets this method 
        // when fulfilling the role of IKeyHashsetProvider

        public static HashSet<string> GetKeyHashSet(this IKeyHashsetProviderRole self, IKeyFactory keyFactory, Func<object, string> keySelector)
        {
            var self_ = self as ISchema;
            if (self_ == null)
                return null;

            return (self_ as Schema).GetKeyHashSet(keyFactory, keySelector)
                ?? (self_ as SchemaMultiFile).GetKeyHashSet(keyFactory, keySelector);
        }

        public static HashSet<string> GetKeyHashSet(this Schema self, IKeyFactory keyFactory, Func<object, string> keySelector)
        {
            if (self == null)
                return null;

            var pks = keyFactory.GetKeys(self.Name)
                .Select(keySelector);

            return new HashSet<string>(pks, null);
        }

        public static HashSet<string> GetKeyHashSet(this SchemaMultiFile self, IKeyFactory keyFactory, Func<object, string> keySelector)
        {
            if (self == null)
                return null;

            var pks = self.SourceSchemas
                .SelectMany(s => keyFactory.GetKeys(s.Name)
                    .Select(keySelector))
                .Distinct();

            return new HashSet<string>(pks, null);
        }
    }
}
