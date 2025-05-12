using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<(T item, float priority)> elements = new List<(T, float)>();

    public void Enqueue(T item, float priority)
    {
        elements.Add((item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;
        for (int i = 1; i < elements.Count; i++)
        {
            if (elements[i].priority < elements[bestIndex].priority)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }

    public bool IsEmpty => elements.Count == 0;
}
