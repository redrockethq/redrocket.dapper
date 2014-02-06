using FlitBit.Wireup;

namespace RedRocket.Dapper.Core.Tests.Infrastructure
{
    public abstract class AbstractTests
    {
        protected AbstractTests()
        {
            WireupCoordinator.SelfConfigure();
        }
    }
}
