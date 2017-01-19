using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;
using RIChecker.Interfaces;

namespace TestRiChecker.MocksAndBuilders
{
    public class Builder_MockKeyFactory
    {
        public static Mock<IKeyFactory> BuildMockKeyFactory_OnePartParentKey()
        {
            var mockKeyFactory = new Mock<IKeyFactory>();
            mockKeyFactory.Setup(x => x.GetKeys("ParentMff", null))
                .Returns(new List<object> {
                    new { ParentKey = "parentKey1" },
                    new { ParentKey = "parentKey2" }
                });
            mockKeyFactory.Setup(x => x.GetKeys("ChildMff1", null))
                .Returns(new List<object> {
                    new { ParentKey = "parentKey1", ChildKey = "childKey1" },
                    new { ParentKey = "parentKey1", ChildKey = "childKey2" }
                });
            mockKeyFactory.Setup(x => x.GetKeys("ChildMff2", null))
                .Returns(new List<object> {
                    new { ParentKey = "parentKey2", ChildKey = "childKey3" },
                    new { ParentKey = "parentKey2", ChildKey = "childKey4" }
                });
            return mockKeyFactory;
        }

        public static Mock<IKeyFactory> BuildMockKeyFactory_TwoPartKey()
        {
            var mockKeyFactory = new Mock<IKeyFactory>();
            mockKeyFactory.Setup(x => x.GetKeys("ChildMff", null))
                .Returns(new List<object> {
                    new { ParentKey = "parentKey1", ChildKey = "childKey1" },
                    new { ParentKey = "parentKey2", ChildKey = "childKey2" }
                });
            mockKeyFactory.Setup(x => x.GetKeys("GrandchildMff1", null))
                .Returns(new List<object> {
                    new { ParentKey = "parentKey1", ChildKey = "childKey1", GrandchildKey = "grandchildKey1" },
                    new { ParentKey = "parentKey1", ChildKey = "childKey1", GrandchildKey = "grandchildKey2" }
                });
            mockKeyFactory.Setup(x => x.GetKeys("ChildMff2", null))
                .Returns(new List<object> {
                    new { ParentKey = "parentKey2", ChildKey = "childKey2", GrandchildKey = "grandchildKey3" },
                    new { ParentKey = "parentKey2", ChildKey = "childKey2", GrandchildKey = "grandchildKey4" }
                });
            return mockKeyFactory;
        }

        public static Mock<IKeyFactory> BuildMockKeyFactory_MultiSourceParentSchema()
        {
            var mockKeyFactory = new Mock<IKeyFactory>();
            mockKeyFactory.Setup(x => x.GetKeys("Parent1", null))
                .Returns(new List<object> {
                    new { ParentKey = "parentKey1" },
                    new { ParentKey = "parentKey2" }
                });
            mockKeyFactory.Setup(x => x.GetKeys("Parent2", null))
                .Returns(new List<object> {
                    new { ParentKey = "parentKey2" },
                    new { ParentKey = "parentKey3" }
                });
            mockKeyFactory.Setup(x => x.GetKeys("ChildMff1", null))
                .Returns(new List<object> {
                    new { ParentKey = "parentKey1", ChildKey = "childKey1" },
                    new { ParentKey = "parentKey1", ChildKey = "childKey2" }
                });
            mockKeyFactory.Setup(x => x.GetKeys("ChildMff2", null))
                .Returns(new List<object> {
                    new { ParentKey = "parentKey2", ChildKey = "childKey3" },
                    new { ParentKey = "parentKey2", ChildKey = "childKey4" }
                });
            return mockKeyFactory;
        }

    }
}
