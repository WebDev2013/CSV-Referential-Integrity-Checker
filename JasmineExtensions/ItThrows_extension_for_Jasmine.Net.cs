using System;
using JasmineNET;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasmineExtensions
{
    public class JasmineExpectedExceptionNotThrownException : Exception
    {
        public JasmineExpectedExceptionNotThrownException()
        {
        }

        public JasmineExpectedExceptionNotThrownException(string message)
            : base(message)
        {
        }

        public JasmineExpectedExceptionNotThrownException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class JasmineExtended : Jasmine
    {
        /// <summary>
        /// Implement a version of It() method which expects an exception in the test
        /// Currently (Dec 2016) Expect.ToThrow() is not implemented in Jasmine.NET
        /// </summary>
        /// <param name="description"></param>
        /// <param name="expectedExceptionType"></param>
        /// <param name="action"></param>
        public void ItThrows(string description, Type expectedExceptionType, Action action)
        {
            try
            {
                action.Invoke();
                // Fail test when no exception is thrown
                throw new JasmineExpectedExceptionNotThrownException(
                    $"TEST: {description} - Expected exception of type: '{expectedExceptionType.Name}', but did not throw an exception");
            }
            catch (JasmineExpectedExceptionNotThrownException) { throw; }
            catch (Exception e)
            {
                if (e.GetType() != expectedExceptionType)
                    // Fail test when exception type is not as expected
                    throw new JException(
                        $"TEST: {description} - Expected exception of type: '{expectedExceptionType.Name}', got exception of type '{e.GetType().Name}'");
            }
        }

        /// <summary>
        /// An overload for It method which takes (and ignores) an exception type parameter
        /// Use this to test a test (red/green/refactor), by changing an ItThrows call to an It call.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="expectedExceptionType"></param>
        /// <param name="action"></param>
        public void It(string description, Type expectedExceptionType, Action action)
        {
            It(description, action);
        }
    }

    [TestClass]
    public class JasminExtensions_TestItThrows : JasmineExtended
    {
        private void Sut_That_Throws_Exception()
        {
            throw new Exception();
        }
        private void Sut_That_Does_Not_Throw_Exception()
        {
        }

        /// <summary>
        /// Expect no exception, absorbed within ItThrows() method. 
        /// Uncomment the ExpectedException attribute to test the test (should fail)
        /// </summary>
        [TestMethod]
        [TestCategory("Jasmine Extensions")]
        //[ExpectedException(typeof(Exception))]  
        public void JasminExtensions_TestItThrows_Test_Passes()
        {
            Describe("Test_ItThrows - variant of It() that expects SUT to throw and exception", () =>
            {
                ItThrows("When the expected exception occurs in SUT, test passes", typeof(Exception), () =>
                {
                    // Act
                    Sut_That_Throws_Exception();
                });
            });
        }

        /// <summary>
        /// Excpect a JException to be thrown. 
        /// Comment out the ExpectedException attribute to test the test.
        /// </summary>
        [TestMethod]
        [TestCategory("Jasmine Extensions")]
        [ExpectedException(typeof(JasmineExpectedExceptionNotThrownException))]
        public void JasminExtensions_TestItThrows_WhenExceptionIsNotThrown_TestFails()
        {
            Describe("Test_ItThrows - variant of It() that expects SUT to throw and exception", () =>
            {
                ItThrows("When an expection does not occur, test fails", typeof(ArgumentException), () =>
                {
                    // Act
                    Sut_That_Does_Not_Throw_Exception();
                });
            });
        }

        /// <summary>
        /// Excpect a JException to be thrown. 
        /// Comment out the ExpectedException attribute to test the test.
        /// </summary>
        [TestMethod]
        [TestCategory("Jasmine Extensions")]
        [ExpectedException(typeof(JException))]
        public void JasminExtensions_TestItThrows_WhenWrongExceptionIsThrown_TestFails()
        {
            Describe("Test_ItThrows - variant of It() that expects SUT to throw and exception", () =>
            {
                ItThrows("When the wrong exception occurs in SUT, test fails", typeof(ArgumentNullException), () =>
                {
                    // Act
                    Sut_That_Throws_Exception();
                });
            });
        }
    }
}
