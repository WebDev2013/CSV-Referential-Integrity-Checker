using Moq;
using RIChecker.Interfaces;

namespace TestRiChecker.MocksAndBuilders
{
    public class Builder_MockRiReporter
    {
        public static Mock<IRiReporter> Build()
        {
            return new Mock<IRiReporter>();
        }
    }
}
