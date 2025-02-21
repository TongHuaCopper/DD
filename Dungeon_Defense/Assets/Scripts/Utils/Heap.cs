using System;
using System.Collections.Generic;

/// <summary>
/// 最小堆实现（优先队列）
/// 功能：快速获取代价最小的节点
/// </summary>
/// <typeparam name="T">需要实现IHeapItem接口的类型</typeparam>
public class Heap<T> where T : IHeapItem<T>
{
    private T[] _items;         // 存储堆元素的数组
    private int _currentCount;  // 当前元素数量

    public Heap(int maxSize)
    {
        _items = new T[maxSize];
    }

    /// <summary>
    /// 添加元素到堆
    /// 步骤：
    /// 1. 将新元素放在数组末尾
    /// 2. 向上调整堆结构
    /// </summary>
    public void Add(T item)
    {
        item.HeapIndex = _currentCount;   // 记录元素在堆中的位置
        _items[_currentCount] = item;     // 放入数组末尾
        SortUp(item);                     // 从下往上调整堆结构
        _currentCount++;                  // 元素数量+1
    }

    /// <summary>
    /// 移除并返回堆顶元素（最小值）
    /// 步骤：
    /// 1. 保存堆顶元素
    /// 2. 将末尾元素移到堆顶
    /// 3. 向下调整堆结构
    /// </summary>
    public T RemoveFirst()
    {
        T firstItem = _items[0];          // 取出堆顶元素
        _currentCount--;                  // 元素数量-1

        // 将最后一个元素移到堆顶
        _items[0] = _items[_currentCount];
        _items[0].HeapIndex = 0;

        SortDown(_items[0]);             // 从上往下调整堆结构
        return firstItem;
    }

    /// <summary>
    /// 当元素值变小时更新位置
    /// （用于A*中当发现更优路径时）
    /// </summary>
    public void UpdateItem(T item)
    {
        SortUp(item);  // 只需向上调整（因为值只会变小）
    }

    // 基本属性
    public int Count => _currentCount;
    public bool Contains(T item) => Equals(_items[item.HeapIndex], item);

    /// <summary>
    /// 向上调整（当子节点比父节点小时）
    /// 循环比较父节点，直到找到合适位置
    /// </summary>
    private void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;  // 计算父节点索引
        
        while (true)
        {
            T parentItem = _items[parentIndex];
            
            // 当前节点比父节点小，需要交换
            if (item.CompareTo(parentItem) < 0)
            {
                Swap(item, parentItem);
                parentIndex = (item.HeapIndex - 1) / 2;  // 更新父节点索引
            }
            else
            {
                break;  // 位置正确，终止循环
            }
        }
    }

    /// <summary>
    /// 向下调整（当父节点比子节点大时）
    /// 循环比较子节点，直到找到合适位置
    /// </summary>
    private void SortDown(T item)
    {
        while (true)
        {
            int leftChildIndex = item.HeapIndex * 2 + 1;  // 左子节点索引
            int rightChildIndex = item.HeapIndex * 2 + 2; // 右子节点索引
            int swapIndex = item.HeapIndex;               // 待交换位置

            // 检查左子节点是否更小
            if (leftChildIndex < _currentCount)
            {
                if (_items[leftChildIndex].CompareTo(_items[swapIndex]) < 0)
                {
                    swapIndex = leftChildIndex;
                }

                // 检查右子节点是否比当前候选更小
                if (rightChildIndex < _currentCount && 
                   _items[rightChildIndex].CompareTo(_items[swapIndex]) < 0)
                {
                    swapIndex = rightChildIndex;
                }
            }

            // 如果没有需要交换的子节点，终止循环
            if (swapIndex == item.HeapIndex) break;

            // 执行交换
            Swap(item, _items[swapIndex]);
        }
    }

    /// <summary>
    /// 交换两个元素的位置
    /// 需要同时更新数组和HeapIndex记录
    /// </summary>
    private void Swap(T a, T b)
    {
        // 交换数组中的位置
        (_items[a.HeapIndex], _items[b.HeapIndex]) = (_items[b.HeapIndex], _items[a.HeapIndex]);
        
        // 更新HeapIndex记录
        (a.HeapIndex, b.HeapIndex) = (b.HeapIndex, a.HeapIndex);
    }
}

/// <summary>
/// 堆元素必须实现的接口
/// </summary>
public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex { get; set; }  // 记录元素在堆中的位置
}