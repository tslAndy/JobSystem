using System.Runtime.InteropServices;

namespace Allocation.Arena;

public unsafe class ArenaAllocator
{
    private Bag<Block> blocks;

    private const nuint DEFAULT_BLOCK_SIZE = 4 * 1024 * 1024;

    public ArenaAllocator()
    {
        blocks = new Bag<Block>(8);
        blocks.Add(new Block(DEFAULT_BLOCK_SIZE));
    }

    public T* Allocate<T>(int length) where T : unmanaged
    {
        void* ptr;
        nuint len = (nuint)(length * sizeof(T));

        for (nuint i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].TryAllocate(len, out ptr))
                return (T*)ptr;
        }

        blocks.Add(new Block(Math.Max(len, DEFAULT_BLOCK_SIZE)));
        blocks[blocks.Count - 1].TryAllocate(len, out ptr);
        return (T*)ptr;
    }

    public void Reset()
    {
        for (nuint i = 0; i < blocks.Count; i++)
            blocks[i].Reset();
    }

    public void Copy<T>(T* source, T* dest, int length) where T : unmanaged => NativeMemory.Copy(source, dest, (nuint)(length * sizeof(T)));

    ~ArenaAllocator()
    {
        for (nuint i = 0; i < blocks.Count; i++)
            blocks[i].Dispose();
        blocks.Dispose();
    }
}
