using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Heap<T> where T : IHeapItem<T>  
{
    T[] items;
    int currentItemCount;

    public Heap(int maxCountSize)
    {
        items = new T[maxCountSize];
    }
    public void Add(T item)
    {
        item.heapIndex = currentItemCount;
        items[currentItemCount++] = item;
        SortUp(item);
    }
    public T RemoveFirst()
    {
        T firstItem = items[0];
        items[0] = items[--currentItemCount];
        items[0].heapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }
    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }

    public bool Contains(T item)
    {
        return Equals(items[item.heapIndex],item);
    }
    public void SortDown(T item)
    {
        while (true) { 
            int leftChildIndex = item.heapIndex * 2 + 1;
            int rightChildIndex = item.heapIndex * 2 + 2;
            int swapIndex = 0;

            if (leftChildIndex > currentItemCount) break;

            //자식 중 더 작은 자식노드 찾는 방식
             swapIndex = leftChildIndex;
            if (items[swapIndex].CompareTo(items[rightChildIndex]) > 0)
            { 
                swapIndex = rightChildIndex;
            }

            if (item.CompareTo(items[swapIndex])>0)
            {
                Swap(item, items[swapIndex]);
            }
            else
            {
                return;
            }
            
        }
    }

    public void SortUp(T item)
    {
        //heap에서 부모 노드의 인덱스는 인덱스-1 /2 이다.
        int parentIndex = (item.heapIndex - 1) / 2;

        while(parentIndex > 0)
        {
            if (item.CompareTo(items[parentIndex]) < 0)
            {
                Swap(item, items[parentIndex]);
                parentIndex = (item.heapIndex - 1) / 2;
            }
        }
    }
    public void Swap(T itemA, T itemB)
    {
        items[itemA.heapIndex] = itemB;
        items[itemB.heapIndex]= itemA;
        int tmpIndex = itemA.heapIndex;
        itemA.heapIndex = itemB.heapIndex;
        itemA.heapIndex = tmpIndex;

    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int heapIndex
    {
        get;
        set;
    }
}
