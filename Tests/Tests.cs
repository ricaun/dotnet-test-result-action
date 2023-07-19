using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Test2(int number)
        {
            System.Console.WriteLine(number);
        }
    }
}