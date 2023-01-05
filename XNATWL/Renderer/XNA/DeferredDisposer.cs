using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer.XNA
{
    public class DeferredDisposer
    {
        private XNARenderer _renderer;
        private List<IDisposable> _queue;
        private int _disposedPerUpdate = 100;

        public DeferredDisposer(XNARenderer renderer)
        {
            this._renderer = renderer;
            this._queue = new List<IDisposable>();
        }

        public void Add(IDisposable disposable)
        {
            this._queue.Add(disposable);
        }

        public void Update()
        {
            int disposableNow = this._queue.Count;
            int disposedNow = Math.Min(this._queue.Count, this._disposedPerUpdate);

            /*if (disposedNow > 0)
            {
                System.Diagnostics.Debug.WriteLine("Disposing " + disposedNow + "/" + disposableNow);
            }*/

            for (int i = 0; i < disposedNow; i++)
            {
                IDisposable disposed = this._queue[0];
                this._queue.RemoveAt(0);
                disposed.Dispose();
            }

            if (disposedNow > 0)
            {
                System.GC.Collect();
            }
        }
    }
}
