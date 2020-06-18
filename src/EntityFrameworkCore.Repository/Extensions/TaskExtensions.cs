using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Repository.Extensions
{
    public static class TaskExtensions
    {
        public static Task<TResult> Then<TSource, TResult>(this Task<TSource> sourceTask, Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<TResult>();

            sourceTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    taskCompletionSource.TrySetException(task.Exception.InnerExceptions);
                }
                else if (task.IsCanceled)
                {
                    taskCompletionSource.TrySetCanceled();
                }
                else
                {
                    try
                    {
                        var result = selector(task.Result);

                        taskCompletionSource.TrySetResult(result);
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.TrySetException(ex);
                    }
                }

            }, cancellationToken);

            return taskCompletionSource.Task;
        }
    }
}
