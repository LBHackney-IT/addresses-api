using FluentAssertions;
using NUnit.Framework;
using Reindex;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReindexTests.MessageQueue
{
    public class MessageQueueTests
    {
        [TestCase(0)]
        [TestCase(100)]
        [TestCase(598)]
        public void MessageTimingIsValid_InvalidTimings_ReturnsFalse(int timeDelaySeconds)
        {
            var message = new SqsMessage()
            {
                taskId = "test",
                newIndex = "test",
                alias = "test",
                timeCreated = DateTime.Now.AddSeconds(-timeDelaySeconds)
            };

            var handler = new HandlerTestDouble();

            handler.MessageTimingIsValid(message).Should().Be(false);
        }

        [TestCase(600)]
        [TestCase(601)]
        [TestCase(700)]
        [TestCase(99999)]
        public void MessageTimingIsValid_ValidTimings_ReturnsTrue(int timeDelaySeconds)
        {
            var message = new SqsMessage()
            {
                taskId = "test",
                newIndex = "test",
                alias = "test",
                timeCreated = DateTime.Now.AddSeconds(-timeDelaySeconds)
            };

            var handler = new HandlerTestDouble();

            handler.MessageTimingIsValid(message).Should().Be(true);
        }

        [TestCase(null, 600)]
        [TestCase(1, 600)]
        [TestCase(599, 600)]
        [TestCase(600, 600)]
        [TestCase(601, 601)]
        [TestCase(899, 899)]
        [TestCase(900, 900)]
        [TestCase(901, 900)]
        [TestCase(999999, 900)]
        public void GetSqsMessageDelaySeconds_VariousEnvironmentSettings_ReturnsCorrectDelay(int? value, int expectedReturn)
        {
            Environment.SetEnvironmentVariable("SQS_MESSAGE_DELAY", value?.ToString());
            var handler = new HandlerTestDouble();

            handler.GetSqsMessageDelaySeconds().Should().Be(expectedReturn);
        }

        [Test]
        public void GetSqsMessageDelaySeconds_InvalidEnvironmentSetting_ReturnsMinimumDelayANdLogsMessage()
        {
            Environment.SetEnvironmentVariable("SQS_MESSAGE_DELAY", "invalid-will-throw-exception");
            var handler = new HandlerTestDouble();

            handler.GetSqsMessageDelaySeconds().Should().Be(600);
            handler.LastError.Should().Be("SQS_MESSAGE_DELAY either not found or not an integer, using default of 600s");
        }
    }
}
