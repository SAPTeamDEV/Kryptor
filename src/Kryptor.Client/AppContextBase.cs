using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.CommonTK;
using SAPTeam.Kryptor.Generators;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Provides an application context with multi task support.
    /// </summary>
    public class AppContextBase : Context
    {
        Dictionary<string, (Task Task, CancellationTokenSource Token)> tasks = new Dictionary<string, (Task Task, CancellationTokenSource Token)>();

        string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        CryptoRandom random = new CryptoRandom();

        /// <inheritdoc/>
        protected override void CreateContext()
        {
            // Just for initializing the EntroX generator.
            var _d = new byte[8];
            random.NextBytes(_d);
            EntroX.AddEntropy(_d);
        }

        /// <inheritdoc/>
        protected override void DisposeContext()
        {
            foreach (var item in tasks.Values)
            {
                if (!item.Token.IsCancellationRequested)
                {
                    item.Token.Cancel();
                }
            }

            Task.WaitAll(tasks.Values.Select(x => x.Task).Where(x => x.Status == TaskStatus.Running).ToArray());
        }

        /// <summary>
        /// Generates a new pair of task id with a cancellation token to start a new task.
        /// </summary>
        /// <returns></returns>
        public (string, CancellationToken) NewTask()
        {
            string id;
            do
            {
                id = "";
                for (int i = 0; i < 5; i++)
                {
                    id += letters[random.Next(letters.Length - 1)];
                }
            }
            while (!tasks.ContainsKey(id));

            var source = new CancellationTokenSource();

            tasks[id] = (Task.CompletedTask, source);
            return (id, source.Token);
        }

        /// <summary>
        /// Registers the task with an id. Monitored tasks will be notified when the app is exiting and have time to gracefully cancel the operation.
        /// </summary>
        /// <param name="id">
        /// The generated id by NewTask().
        /// </param>
        /// <param name="task">
        /// The task to monitor.
        /// </param>
        /// <exception cref="InvalidOperationException"></exception>
        public void MonitorTask(string id, Task task)
        {
            var tData = tasks[id];

            if (tData.Task.IsCompleted && !tData.Token.IsCancellationRequested)
            {
                tasks[id] = (task, tData.Token);
            }
            else
            {
                throw new InvalidOperationException("This id already is being monitored: " + id);
            }
        }
    }
}
