
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace TestProject1
{
    
    [TestClass]
    public class UnitTest2
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            Debug.Print($"Running ClassInit ${nameof(UnitTest2)}");
        }

        [TestMethod]
        public void TestMethod1() 
        { 
            Debug.Print("Running TestMethod1....."); 
            Assert.AreEqual(1,1); 
        }

        [TestMethod]
        public void TestMethod2() 
        { 
            Debug.Print("Running TestMethod2....."); 
            Assert.AreEqual(2, 2); 
        }

        [TestCleanup()]
        public void Cleanup()
        {
            Debug.Print("Running Test Cleanup.....");
        }

        [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
        public static void ClassCleanup()
        {
            Debug.Print("Running Class Cleanup.....");
        }
    }
}
