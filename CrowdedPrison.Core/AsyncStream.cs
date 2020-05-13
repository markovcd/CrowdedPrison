using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrowdedPrison.Core
{
  public class AsyncStream<T> : IAsyncEnumerable<T>, IEnumerable<T>
  {
    private readonly List<T> list = new List<T>();
    private bool isFinished;

    private event EventHandler<T> Added;
    private event EventHandler Finished;

    public void Add(T data)
    {
      list.Add(data);
      Added?.Invoke(this, data);
    }

    public void Finish()
    {
      isFinished = true;
      Finished?.Invoke(this, EventArgs.Empty);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
      return new OutputStreamEnumerator(cancellationToken, this);
    }

    private class OutputStreamEnumerator : IAsyncEnumerator<T>
    {
      private readonly AsyncStream<T> stream;
      private CancellationToken cancellationToken;
      private int position = -1;
      private TaskCompletionSource<object> tcs;

      public T Current => stream.list[position];

      public OutputStreamEnumerator(CancellationToken cancellationToken, AsyncStream<T> stream)
      {
        this.cancellationToken = cancellationToken;
        this.stream = stream;
        stream.Added += Stream_Added;
        stream.Finished += Stream_Finished;
      }

      private void Stream_Finished(object sender, EventArgs e)
      {
        tcs?.TrySetResult(default);
      }

      private void Stream_Added(object sender, T e)
      {
        tcs?.TrySetResult(default);
      }

      async ValueTask<bool> IAsyncEnumerator<T>.MoveNextAsync()
      {
        cancellationToken.ThrowIfCancellationRequested();

        position++;
        if (stream.list.Count > position) return true;
        if (stream.isFinished) return false;
        tcs = new TaskCompletionSource<object>();

        using (cancellationToken.Register(() => tcs?.TrySetCanceled(cancellationToken)))
        {
          await tcs.Task;
        }

        tcs = null;

        return !stream.isFinished;
      }

#pragma warning disable CS1998
      async ValueTask IAsyncDisposable.DisposeAsync()
#pragma warning restore CS1998
      {
        stream.Added -= Stream_Added;
        stream.Finished -= Stream_Finished;
        position = -1;
      }
    }

    public IEnumerator<T> GetEnumerator()
    {
      return list.ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}