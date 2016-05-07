using System.Threading.Tasks;

namespace DataProtection.Azure
{
    internal static class TaskExtensions
    {
        public static void SyncAwait(this Task task)
        {
            task.GetAwaiter().GetResult();
        }

        public static T SyncAwait<T>(this Task<T> task)
        {
            return task.GetAwaiter().GetResult();
        }
    }
}