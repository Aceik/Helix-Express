using Aceik.HelixExpress.Main;
using Xunit;

namespace Aceik.HelixExpress.Tests
{
    public class Tests
    {
        [Fact]
        public void Create_ShouldCreateLanguageModel()
        {
            new SolutionCreator().LoadTemplateSlnFile();
        }
    }
}