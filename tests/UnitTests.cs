using NUnit.Framework;
using static Program;
using System.IO;

namespace tests
{
    public class UnitTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestHalt()
        {
            var vm = new VM();
            vm.Memory[0] = 0;
            vm.Run();
            Assert.AreEqual(1, vm.Cycles);
        }

        [Test]
        public void TestOut()
        {
            var vm = new VM();
            vm.Memory[0] = 19;
            vm.Memory[1] = 65;
            vm.Memory[2] = 0;
            vm.Run();
            Assert.AreEqual("A", vm.Output.ToString());
        }
    }
}
