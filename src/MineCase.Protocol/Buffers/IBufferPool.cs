using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Buffers
{
    public interface IBufferPool<T>
    {
        IBufferPoolScope<T> CreateScope();
    }

    public interface IBufferPoolScope<T> : IDisposable
    {
        ArraySegment<T> Rent(int length);
    }

    [Orleans.GenerateSerializer]
    public class BufferPool<T> : IBufferPool<T>
    {
        [Orleans.Id(0)]
        private readonly ArrayPool<T> _arrayPool;
        [Orleans.Id(1)]
        private readonly ConcurrentBag<BufferPoolScope> _scopes = new ConcurrentBag<BufferPoolScope>();

        public BufferPool(ArrayPool<T> arrayPool)
        {
            _arrayPool = arrayPool;
        }

        public IBufferPoolScope<T> CreateScope()
        {
            if (!_scopes.TryTake(out var scope))
                scope = new BufferPoolScope(this, _arrayPool);
            return scope;
        }

        private void Return(BufferPoolScope scope)
        {
            _scopes.Add(scope);
        }

        [Orleans.GenerateSerializer]
        public class BufferPoolScope : IBufferPoolScope<T>
        {
            [Orleans.Id(0)]
            public readonly BufferPool<T> _bufferPool;
            [Orleans.Id(1)]
            public readonly ArrayPool<T> _arrayPool;
            [Orleans.Id(2)]
            public readonly ConcurrentBag<T[]> _rents = new ConcurrentBag<T[]>();

            public BufferPoolScope(BufferPool<T> bufferPool, ArrayPool<T> arrayPool)
            {
                _bufferPool = bufferPool;
                _arrayPool = arrayPool;
            }

            public void Dispose()
            {
                while (_rents.TryTake(out var rent))
                    _arrayPool.Return(rent);
                _bufferPool.Return(this);
            }

            public ArraySegment<T> Rent(int length)
            {
                var rent = _arrayPool.Rent(length);
                _rents.Add(rent);
                return new ArraySegment<T>(rent, 0, length);
            }
        }
    }
}
