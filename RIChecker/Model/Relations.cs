using System;
using System.Collections.Generic;
using System.Linq;

using RIChecker.Interfaces;

namespace RIChecker.Model
{
    public class Relations : List<Relation>
    {
        public Schemas Schemas {
            get
            {
                var schemas = new Schemas();
                schemas.AddRange(this.SelectMany(r => r.ParentSchema.GetSourceSchemas()).ToList());
                schemas.AddRange(this.SelectMany(r => r.ChildSchema.GetSourceSchemas()).ToList());
                return schemas;
            }
        }

        public void Add(string relationDescriptor, ISchema parentSchema, 
            Func<dynamic, string> relationKeySelector,
            string keyDescriptor, ISchema[] childSchemas, Func<dynamic, bool> childFilter = null)
        {
            childSchemas.ToList().ForEach(childSchema => {
                this.Add(new Relation
                {
                    RelationDescriptor = relationDescriptor,
                    ParentSchema = parentSchema,
                    ChildSchema = childSchema,
                    RelationKeySelector = relationKeySelector,
                    KeyDescriptor = keyDescriptor,
                    RelationChildFilter = childFilter
                });
            });
        }
    }
}
