// <copyright file="AsyncHelper.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions to allow an async method to run synchronously.
    /// </summary>
    /// <remarks>
    /// Based on http://stackoverflow.com/a/5097066
    /// </remarks>
    internal static class AsyncHelper
    {
        /// <summary>
        /// Execute's an async Task<T> method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task<T> method to execute</param>
        public static void RunSynchronously(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(
                async _ =>
                {
                    try
                    {
                        await task();
                    }
                    catch (Exception e)
                    {
                        synch.InnerException = e;
                        throw;
                    }
                    finally
                    {
                        synch.EndMessageLoop();
                    }
                },
                null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Execute's an async Task<T> method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task<T> method to execute</param>
        /// <returns></returns>
        public static T RunSynchronously<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            var ret = default(T);
            synch.Post(
                async _ =>
                {
                    try
                    {
                        ret = await task();
                    }
                    catch (Exception e)
                    {
                        synch.InnerException = e;
                        throw;
                    }
                    finally
                    {
                        synch.EndMessageLoop();
                    }
                },
                null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private sealed class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;

            private readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);

            private readonly Queue<Tuple<SendOrPostCallback, object>> items = new Queue<Tuple<SendOrPostCallback, object>>();

            public Exception InnerException { get; set; }

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (this.items)
                {
                    this.items.Enqueue(Tuple.Create(d, state));
                }

                this.workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => this.done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!this.done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (this.items)
                    {
                        if (this.items.Count > 0)
                        {
                            task = this.items.Dequeue();
                        }
                    }

                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (this.InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.RunSync method threw an exception.", this.InnerException);
                        }
                    }
                    else
                    {
                        this.workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}
