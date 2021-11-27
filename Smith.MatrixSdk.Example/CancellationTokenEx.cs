using System.Threading;
using System.Threading.Tasks;

namespace Smith.MatrixSdk.Example
{
    public static class CancellationTokenEx
    {
        public static Task WhenCancelled(this CancellationToken ct)
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            ct.Register(() => tcs.SetResult());
            return tcs.Task;
        }
    }
}
