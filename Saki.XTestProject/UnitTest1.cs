using Saki.ModelTemplate;

namespace Saki.XTestProject
{
    public class UnitTest1
    {
        [Theory]
        [InlineData(3, 2)]
        public void Test1(int a, int b)
        {
            int c = a + b; 
            Assert.Equal(5, c);
        }

        /// <summary>
        /// ²âÊÔ·½·¨Ê¾Àý
        /// </summary>
        [Theory]
        [MemberData(nameof())]
        public void TestFunc()
        {

        }


        public List<> GetData()
        {
            
        }
    }
}