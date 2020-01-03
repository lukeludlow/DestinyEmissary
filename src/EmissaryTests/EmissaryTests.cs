using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EmissaryTests
{
    [TestClass]
    public class EmissaryTests
    {

        // i split the tests for each method into their own test file because this file was 
        // like 1000 lines long and unreadable. so the "EmissaryTests" class file is now empty,
        // but i'm keeping it around because someday i want to turn this into integration tests.

        // random side note. if i run the tests in iterm and then can't see what i'm typing, 
        // then enter the command `stty sane`

        [TestMethod]
        public void Startup_WithConfig_ShouldReturnNewEmissaryInstance()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            Emissary actual = Emissary.Startup(config);
        }

    }
}
