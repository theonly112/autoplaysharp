using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using autoplaysharp.Contracts;
using autoplaysharp.Core.Game;

using Microsoft.Extensions.Logging;

using NSubstitute;

using NUnit.Framework;

namespace autoplaysharp.Tests
{
    [TestFixture]
    internal class TaskExecutionerTests
    {
        private TaskExecutioner _executioner;

        [SetUp]
        public void Setup()
        {
            var logger = Substitute.For<ILogger<TaskExecutioner>>();
            //logger.Log<object>(Arg.Any<LogLevel>(), 
            //           Arg.Any<EventId>(), 
            //           Arg.Any<object>(),
            //           Arg.Any<Exception>(),
            //           Arg.Any<Func<object, Exception, string>>()).Returns()
            _executioner = new TaskExecutioner(logger);
        }

        [Test]
        public void QueueTask()
        {
            var task = new TestTask(token => Task.Delay(500));
            _executioner.QueueTask(task);
            Assert.That(() => _executioner.ActiveItem, Is.EqualTo(task).After(100));
            Assert.That(() => _executioner.ActiveItem, Is.EqualTo(null).After(1000));
        }


        [Test]
        public async Task CancelTask()
        {
            var task = new TestTask(token => Task.Delay(1000));
            _executioner.QueueTask(task);
            Assert.That(() => _executioner.ActiveItem, Is.EqualTo(task).After(100));
            await _executioner.Cancel(task);
            Assert.That(() => _executioner.ActiveItem, Is.EqualTo(null).After(1000));
            Assert.That(() => _executioner.Items.Count(), Is.EqualTo(0).After(1000));
        }

        private sealed class TestTask : IGameTask
        {
            private readonly Func<CancellationToken, Task> _del;

            public TestTask(Func<CancellationToken, Task> del)
            {
                _del = del;
            }

            public Task Run(CancellationToken token)
            {
                return _del(token);
            }
        }
    }
}
