using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace ClientEmulator
{
    internal static class ImageQueue
    {
        private static readonly Queue<(Bitmap Image, float time)> _queue = new Queue<(Bitmap, float)>();
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public static int Count => _queue.Count;

        public static async void AddImage(Bitmap image, float time)
        {
            if (await _semaphore.WaitAsync(0))
            {
                lock (_queue)
                {
                    _queue.Enqueue((image, time));
                }
            }
        }

        public static (Bitmap Image, float time) GetImage()
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    _semaphore.Release();
                    return _queue.Dequeue();
                }
            }

            return (null, 0);
        }
    }
}
