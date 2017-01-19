using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using RIChecker.Utils;
using RIChecker.Model;
using RIChecker.Interfaces;
using RIChecker.Implementation;

namespace RIChecker
{
    public class RiChecker
    {
        private Relations relations; 
        private IRiReporter reporter;
        private IKeyFactory keyFactory;
        private IFileParser parser;
        private Config config;

        public List<RiResult> Results { get; set; }

        public RiChecker(Relations relations, IKeyFactory keyFactory, IFileParser parser, IRiReporter reporter, Config config)
        {
            this.relations = relations;
            this.keyFactory = keyFactory;
            this.parser = parser;
            this.reporter = reporter;
            this.config = config;

            Results = new List<RiResult>();
        }

        public RiChecker(Relations relations, Config config) 
        {
            this.relations = relations;
            var reporter = new RiReporter(config);
            this.keyFactory = new KeyFactory(
                schemas: relations.Schemas,
                //fileParser: new FileParser(lineParser), 
                csvFileHandler: new CsvFileHandler(config) , 
                reporter: reporter );
            this.reporter = reporter;
            this.config = config;

            Results = new List<RiResult>();
        }

        public void Check()
        {
            reporter.OnInit(relations.Count);

            // Group relations to minimize parent key reads
            var parentGroups = relations
                .GroupBy(r => new { r.ParentSchema, r.RelationKeySelector })
                .Select(g => new {
                    Relations = g.ToList(),
                    ParentKeyHash = (g.Key.ParentSchema as IKeyHashsetProviderRole).GetKeyHashSet(keyFactory, g.Key.RelationKeySelector)
                })
            .ToList();

            try
            {
                parentGroups.ForEach(pg => {
                    pg.Relations.ForEach(relation => {
                        relation.ParentKeyCount = pg.ParentKeyHash.Count();
                        RelationChecker(relation, pg.ParentKeyHash);
                    });
                });
            }
            finally
            {
                reporter.OnComplete(Results);
            }
        }

        private void RelationChecker(Relation relation, HashSet<string> parentKeyHash)
        {
            reporter.OnNextItem();
            try
            {
                var orphans = GetOrphans(relation, parentKeyHash);
                Results.Add(GetResults(relation, orphans));
            }
            catch (Exception e)
            {
                Results.Add(GetResults(relation, e));
            }
        }

        private List<object> GetOrphans(Relation relation, HashSet<string> parentKeyHash)
        {
            var keys = keyFactory.GetKeys(relation.ChildSchema.Name, relation.RelationChildFilter);
            return keys
                .IncrementCounter( ()=> { relation.ChildKeyCount++; })       // Replace Count() with pipline method to ensure we don't enumerate the file twice
                .Where(ck => !parentKeyHash.Contains(relation.RelationKeySelector(ck)))
                .ToList();
        }

        private RiResult GetResults(Relation relation, List<object> orphans)
        {
            var orphanSample = config.SampleSize == 0 ? orphans :
                orphans.Take(config.SampleSize).ToList(); 

            return new RiResult
            {
                Description = relation.ToString(),
                OrphansSample = orphanSample,
                ParentCount = relation.ParentKeyCount,
                ChildCount = relation.ChildKeyCount,
                TotalOrphanCount = orphans.Count()
            };
        }

        private RiResult GetResults(Relation relation, Exception e)
        {
            return new RiResult
            {
                Description = relation.ToString(),
                ParentCount = relation.ParentKeyCount,
                ChildCount = relation.ChildKeyCount,
                ErrorMessage = e.Message
            };
        }
    }
    }
