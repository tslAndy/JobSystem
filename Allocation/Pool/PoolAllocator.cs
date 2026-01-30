using System.Numerics;
using System.Runtime.InteropServices;

namespace Allocation.Pool;

public unsafe class PoolAllocator
{
    private BlockPool* pools;
    private int fill = 8;
    private int length = 32;

    public PoolAllocator()
    {
        pools = (BlockPool*)NativeMemory.AlignedAlloc((nuint)(32 * sizeof(BlockPool*)), 4);
        for (int i = 0; i < 8; i++)
            InitPool(i, 16U << i);
    }

    private void InitPool(int index, nuint size)
    {
        void* pool = NativeMemory.AlignedAlloc(size * 64, 4);
        Block* block = (Block*)pool;

        pools[index] = new BlockPool
        {
            blockSize = size,
            ptr = pool,
            block = block,
        };

        IntPtr poolIntPtr = new IntPtr(pool);
        for (nuint j = 1; j < 64; j++)
        {
            Block* current = (Block*)(poolIntPtr + (nint)(size * j));
            block->next = current;
            block = current;
        }
    }

    public T* Allocate<T>(int len)
        where T : unmanaged
    {
        nuint size = (nuint)(len * sizeof(T));
        size = BitOperations.RoundUpToPowerOf2(size);
        BlockPool* pool = null;
        for (int i = 0; i < fill; i++)
        {
            BlockPool* temp = pools + i;
            if (temp->blockSize != size || temp->block == null)
                continue;

            pool = temp;
            break;
        }

        if (pool == null)
        {
            if (fill == length)
            {
                length *= 2;
                pools = (BlockPool*)NativeMemory.AlignedRealloc(pools, (nuint)len, 4);
            }
            InitPool(fill, size);
            pool = pools + fill;
            fill++;
        }

        Block* block = pool->block;
        pool->block = pool->block->next;
        return (T*)block;
    }

    public void Deallocate(void* ptr)
    {
        BlockPool* pool = null;
        IntPtr targetPtr = new IntPtr(ptr);
        for (int i = 0; i < fill; i++)
        {
            IntPtr start = new IntPtr(pool->ptr);
            IntPtr end = start + 64 * (nint)pool->blockSize;

            if (start <= targetPtr && targetPtr < end)
                break;
        }
        Block* block = (Block*)targetPtr;
        block->next = pool->block;
        pool->block = block;
    }

    public T* Realloc<T>(T* source, int length)
        where T : unmanaged
    {
        T* dest = Allocate<T>(length);

        BlockPool* pool = null;
        IntPtr targetPtr = new IntPtr(source);
        for (int i = 0; i < fill; i++)
        {
            IntPtr start = new IntPtr(pool->ptr);
            IntPtr end = start + 64 * (nint)pool->blockSize;
            if (start <= targetPtr && targetPtr < end)
                break;
        }

        NativeMemory.Copy(source, dest, (nuint)pool->blockSize);
        Block* block = (Block*)targetPtr;
        block->next = pool->block;
        pool->block = block;

        return dest;
    }

    public void Copy<T>(T* source, T* dest, int length)
        where T : unmanaged => NativeMemory.Copy(source, dest, (nuint)(length * sizeof(T)));

    ~PoolAllocator()
    {
        for (int i = 0; i < fill; i++)
            NativeMemory.AlignedFree(pools[i].ptr);
        NativeMemory.AlignedFree(pools);
    }
}
