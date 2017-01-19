using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RIChecker.Model;
using RIChecker.Interfaces;
using RIChecker.Implementation;

namespace TestRiChecker.MocksAndBuilders
{
    public static class Builder_Relations
    {
        public static Relations BuildRelations_OneParentWithTwoChildren()
        {
            Func<List<string>, object> parentParser = r => new { ParentKey = r[0], ParentValue1 = r[1], ParentValue2 = r[2] };
            Func<List<string>, object> childParser = r => new { ParentKey = r[0], ChildKey = r[1], ChildValue1 = r[2], ChildValue2 = r[3] };

            Func<dynamic, string> relationKeySelector = x => x.ParentKey;

            var schemas = new Schemas
            {
                new Schema("ParentMff", parentParser),
                new Schema("ChildMff1", childParser),
                new Schema("ChildMff2", childParser)
            };

            var relations = new Relations();
            relations.Add("One parent with two children",
                parentSchema: schemas["ParentMff"], 
                relationKeySelector: relationKeySelector,
                keyDescriptor: "ParentKey",
                childSchemas: new []
                {
                    schemas["ChildMff1"],
                    schemas["ChildMff2"]
                }
            );

            return relations;
        }

        public static Relations BuildRelations_MultiSourceParentSchema()
        {
            Func<List<string>, object> parentParser = r => new { ParentKey = r[0], ParentValue1 = r[1], ParentValue2 = r[2] };
            Func<List<string>, object> childParser = r => new { ParentKey = r[0], ChildKey = r[1], ChildValue1 = r[2], ChildValue2 = r[3] };

            Func<dynamic, string> relationKeySelector = x => x.ParentKey;

            var schemas = new Schemas
            {
                new SchemaMultiFile(
                    name: "ParentMff",
                    sourceSchemas: new List<ISchema> {
                        new Schema("Parent1", parentParser),
                        new Schema("Parent2", parentParser)
                    }
                ),
                new Schema("ChildMff1", childParser),
                new Schema("ChildMff2", childParser)
            };

            var relations = new Relations();
            relations.Add("Multi source parent schema test",
                parentSchema: schemas["ParentMff"],
                relationKeySelector: relationKeySelector,
                keyDescriptor: "ParentKey",
                childSchemas: new []
                {
                    schemas["ChildMff1"],
                    schemas["ChildMff2"]
                }
            );

            return relations;
        }

        public static Relations BuildRelations_TwoParents()
        {
            Func<List<string>, object> parentParser = r => new { ParentKey = r[0], ParentValue1 = r[1], ParentValue2 = r[2] };
            Func<List<string>, object> childParser = r => new { ParentKey = r[0], ChildKey = r[1], ChildValue1 = r[2], ChildValue2 = r[3] };

            Func<dynamic, string> relationKeySelector = x => x.ParentKey;

            var schemas = new Schemas
            {
                new Schema("ParentMff1", parentParser),
                new Schema("ParentMff2", parentParser),
                new Schema("ChildMff1", childParser),
                new Schema("ChildMff2", childParser),
                new Schema("ChildMff3", childParser),
                new Schema("ChildMff4", childParser)
            };

            var relations = new Relations();
            relations.Add("First parent relations",
                parentSchema: schemas["ParentMff1"],
                relationKeySelector: relationKeySelector,
                keyDescriptor: "ParentKey",
                childSchemas: new []
                {
                    schemas["ChildMff1"],
                    schemas["ChildMff2"]
                }
            );
            relations.Add("Second parent relations",
                parentSchema: schemas["ParentMff2"],
                relationKeySelector: relationKeySelector,
                keyDescriptor: "ParentKey",
                childSchemas: new []
                {
                    schemas["ChildMff3"],
                    schemas["ChildMff4"]
                }
            );

            return relations;
        }

        public static Relations BuildRelations_TwoPartKeys()
        {
            Func<List<string>, object> childParser = r => new { ParentKey = r[0], ChildKey = r[1], ChildValue1 = r[2] };
            Func<List<string>, object> grandchildParser = r => new { ParentKey = r[0], ChildKey  = r[1], GrandchildKey = r[2], GrandchildValue1 = r[3] };

            Func<dynamic, string> relationKeySelector = x => $"{x.ParentKey}:{x.ChildKey}";

            var schemas = new Schemas
                {
                    new Schema("ChildMff", childParser),
                    new Schema("GrandchildMff1", childParser),
                    new Schema("GrandchildMff2", childParser)
                };

            var relations = new Relations();
            relations.Add("Two part keys relation",
                parentSchema: schemas["ChildMff"],
                relationKeySelector: relationKeySelector,
                keyDescriptor: "ParentKey:ChildKey",
                childSchemas: new []
                {
                        schemas["GrandchildMff1"],
                        schemas["GrandchildMff2"]
                }
            );

            return relations;
        }

    }
}
