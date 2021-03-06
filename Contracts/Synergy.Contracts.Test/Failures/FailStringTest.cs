using JetBrains.Annotations;
using NUnit.Framework;

namespace Synergy.Contracts.Test.Failures
{
    [TestFixture]
    public class FailStringTest
    {
        #region Fail.IfArgumentEmpty

        [Test]
        [TestCase(null)]
        public void IfArgumentEmptyWithNull(string argumentValue)
        {
            // ACT
            var exception = Assert.Throws<DesignByContractViolationException>(
                () => Fail.IfArgumentEmpty(argumentValue, nameof(argumentValue))
            );
            
            // ASSERT
            Assert.That(exception.Message, Is.EqualTo("Argument 'argumentValue' was null."));
        }

        [Test]
        [TestCase("")]
        public void IfArgumentEmptyWithEmptyString(string argumentValue)
        {
            // ACT
            var exception = Assert.Throws<DesignByContractViolationException>(
                () => Fail.IfArgumentEmpty(argumentValue, nameof(argumentValue))
            );

            // ASSERT
            Assert.That(exception.Message, Is.EqualTo("Argument 'argumentValue' was empty."));
        }

        [Test]
        [TestCase("not empty")]
        [TestCase(" ")]
        public void IfArgumentEmptySuccess([NotNull] string argumentValue)
        {
            // ACT
            Fail.IfArgumentEmpty(argumentValue, nameof(argumentValue));
        }

        #endregion

        #region Fail.IfEmpty

        [Test]
        public void IfEmpty()
        {
            Assert.Throws<DesignByContractViolationException>(
                () => Fail.IfEmpty("", "message")
                );

            Assert.Throws<DesignByContractViolationException>(
                () => Fail.IfEmpty(null, "message")
                );

            Fail.IfEmpty("   ", "message");
            Fail.IfEmpty("aa", "message");
        }

        #endregion

        [Test]
        public void IfArgumentWhiteSpace()
        {
            Assert.Throws<DesignByContractViolationException>(
                () => Fail.IfArgumentWhiteSpace(null, "null")
            );

            Assert.Throws<DesignByContractViolationException>(
                () => Fail.IfArgumentWhiteSpace("", "empty")
            );

            Assert.Throws<DesignByContractViolationException>(
                () => Fail.IfArgumentWhiteSpace("   ", "bia�e-znaki")
            );

            Fail.IfArgumentWhiteSpace("nie pusty", "nie-pusty");
        }

        [Test]
        public void IfWhitespace()
        {
            Assert.Throws<DesignByContractViolationException>(
                () => Fail.IfWhitespace("", "message")
                );

            Assert.Throws<DesignByContractViolationException>(
                () => Fail.IfWhitespace(null, "message")
                );

            Assert.Throws<DesignByContractViolationException>(
                () => Fail.IfWhitespace("   ", "message")
                );

            Fail.IfWhitespace("aa", "message");
        }

        [Test]
        public void IfTooLong()
        {
            var veryLong = "very long";
            Assert.Throws<DesignByContractViolationException>(
                () => Fail.IfToLong(veryLong, 3, nameof(veryLong))
            );

            Fail.IfToLong(null, 300, "null");
            Fail.IfToLong("aa", 300, "aa");
            Fail.IfToLong("aa", 2, "aa");
        }

        [Test]
        public void FailIfTooLong()
        {
            var veryLong = "very long";
            Assert.Throws<DesignByContractViolationException>(
                () => veryLong.FailIfToLong(3, nameof(veryLong))
            );

            ((string)null).FailIfToLong(300, "null");
            "aa".FailIfToLong(300, "aa");
            "aa".FailIfToLong(2, "aa");
        }
    }
}