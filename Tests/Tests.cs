using NUnit.Framework;
using System.Threading;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void Test1()
        {
            Thread.Sleep(1234);
            Assert.Pass();
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Test2(int number)
        {
            System.Console.WriteLine(number);
        }

        [Test]
        public void Test3()
        {
            Thread.Sleep(5678);
            Assert.Pass();
        }
    }
}