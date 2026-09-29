using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.Http
{
    [TestFixture]
    public class UserAgentBuilderFixture : TestBase<UserAgentBuilder>
    {
        [Test]
        public void should_not_include_os_details_in_user_agent()
        {
            Mocker.GetMock<IOsInfo>().SetupGet(c => c.Version).Returns("12.34");
            Mocker.GetMock<IOsInfo>().SetupGet(c => c.Name).Returns("TestOS");

            Subject.GetUserAgent(false).Should().NotContain("TestOS").And.NotContain("12.34");
        }

        [Test]
        public void should_return_simplified_user_agent()
        {
            Mocker.GetMock<IOsInfo>().SetupGet(c => c.Version).Returns("12.34");
            Mocker.GetMock<IOsInfo>().SetupGet(c => c.Name).Returns("TestOS");

            Subject.GetUserAgent(true).Should().NotContain("TestOS").And.NotContain("12.34");
        }
    }
}
