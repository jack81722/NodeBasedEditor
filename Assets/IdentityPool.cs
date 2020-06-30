using System.Collections;
using System.Collections.Generic;

public class IdentityPool
{
    private int serialId;
    private Queue<int> idPool;
    private HashSet<int> used;

    public IdentityPool()
    {
        serialId = 0;
        idPool = new Queue<int>();
        used = new HashSet<int>();
    }

    public int NewID()
    {
        int id;
        lock (idPool)
        {
            if (idPool.Count > 0)
            {
                id = idPool.Dequeue();
                used.Add(id);
                return id;
            }
        }

        id = serialId++;
        while (used.Contains(id))
        {
            id = serialId++;
        }
        used.Add(id);
        return id;
    }

    public void RecycleID(int id)
    {
        lock (idPool)
        {
            if (!idPool.Contains(id))
            {
                idPool.Enqueue(id);
            }
            used.Remove(id);
        }
    }

    public void SetUsedId(int id)
    {
        used.Add(id);
    }

    public void Reset()
    {
        serialId = 0;
        idPool.Clear();
        used.Clear();
    }
}
