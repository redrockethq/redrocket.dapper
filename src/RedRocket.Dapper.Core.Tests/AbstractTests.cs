using FlitBit.Wireup;

namespace RedRocket.Dapper.Core.Tests
{

    public abstract class AbstractTests
    {
        protected AbstractTests()
        {
            WireupCoordinator.SelfConfigure();
        }
    }
}
