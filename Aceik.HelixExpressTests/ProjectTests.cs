using System;
using Aceik.HelixExpress.Main;
using Xunit;

namespace Aceik.HelixExpressTests
{
    public class ProjectTests
    {
        [Fact]
        public void Load()
        {
            new SolutionCreator().LoadTemplateSlnFile();
            Assert.True(true);
        }
    }
}
