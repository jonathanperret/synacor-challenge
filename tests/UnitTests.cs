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
        public void Halt()
        {
            var vm = new VM();
            vm.Memory[0] = 0;
            vm.Run();
            Assert.AreEqual(1, vm.Cycles);
        }

        [Test]
        public void Out()
        {
            var vm = new VM();
            vm.Memory[0] = 19;     // out
            vm.Memory[1] = 65;     // literal

            vm.Memory[2] = 19;     // out
            vm.Memory[3] = 32768;  // reg 0

            vm.Memory[32768] = 66; // literal

            vm.Run();
            Assert.AreEqual("AB", vm.Output.ToString());
        }

        [Test]
        public void SetRegisterFromLiteral()
        {
            var vm = new VM();
            vm.Memory[0] = 1;     // set
            vm.Memory[1] = 32768; // reg 0
            vm.Memory[2] = 123;   // literal
            vm.Run();
            Assert.AreEqual(123, vm.Memory[32768]);
        }

        [Test]
        public void SetRegisterFromRegister()
        {
            var vm = new VM();
            vm.Memory[0] = 1;     // set
            vm.Memory[1] = 32768; // reg 0
            vm.Memory[2] = 32769; // reg 1

            vm.Memory[32769] = 123;

            vm.Run();
            Assert.AreEqual(123, vm.Memory[32768]);
        }

        [Test]
        public void Push()
        {
            var vm = new VM();
            vm.Memory[0] = 2;     // push
            vm.Memory[1] = 123;   // literal

            vm.Memory[2] = 2;     // push
            vm.Memory[3] = 32769; // reg 1

            vm.Memory[32769] = 456;

            vm.Run();
            CollectionAssert.AreEqual(new[] { 456, 123 }, vm.Stack.ToArray());
        }

        [Test]
        public void Jmp()
        {
            var vm = new VM();
            vm.Memory[0] = 6;     // jmp
            vm.Memory[1] = 5;     // literal

            vm.Memory[5] = 6;     // jmp
            vm.Memory[6] = 32768; // reg 0

            vm.Memory[10] = 19;   // out
            vm.Memory[11] = 65;   // literal

            vm.Memory[32768] = 10;

            vm.Run();
            Assert.AreEqual("A", vm.Output.ToString());
        }

        [Test]
        public void Jt()
        {
            var vm = new VM();
            vm.Memory[0] = 7;     // jt
            vm.Memory[1] = 32768; // reg 0
            vm.Memory[2] = 10;     // literal

            vm.Memory[10] = 19;   // out
            vm.Memory[11] = 65;   // literal

            vm.Memory[32768] = 1;

            vm.Run();
            Assert.AreEqual("A", vm.Output.ToString());
        }

        [Test]
        public void Jf()
        {
            var vm = new VM();
            vm.Memory[0] = 8;     // jt
            vm.Memory[1] = 32768; // reg 0
            vm.Memory[2] = 10;     // literal

            vm.Memory[10] = 19;   // out
            vm.Memory[11] = 65;   // literal

            vm.Memory[32768] = 0;

            vm.Run();
            Assert.AreEqual("A", vm.Output.ToString());
        }

    }
}
