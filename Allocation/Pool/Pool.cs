namespace Allocation.Pool;

unsafe struct BlockPool
{
    public nuint blockSize;
    public void* ptr;
    public Block* block;
}

