using System;
using System.Collections.Generic;
using System.Linq;

namespace DesktopNotifications
{
    /// <summary>
    /// 提供字典扩展方法和高性能数据结构
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 尝试根据值查找对应的键（优化版本）
        /// </summary>
        /// <typeparam name="K">键类型</typeparam>
        /// <typeparam name="V">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="value">要查找的值</param>
        /// <param name="key">找到的键</param>
        /// <returns>是否找到匹配的键值对</returns>
        public static bool TryGetKey<K, V>(this IDictionary<K, V> dict, V value, out K key)
        {
            if (dict == null)
            {
                key = default!;
                return false;
            }

            // 对于小字典，直接遍历仍然高效
            if (dict.Count <= 10)
            {
                foreach (var entry in dict)
                {
                    if (EqualityComparer<V>.Default.Equals(entry.Value, value))
                    {
                        key = entry.Key;
                        return true;
                    }
                }
            }
            else
            {
                // 对于大字典，使用LINQ优化查找
                var kvp = dict.FirstOrDefault(kvp => EqualityComparer<V>.Default.Equals(kvp.Value, value));
                if (!kvp.Equals(default(KeyValuePair<K, V>)))
                {
                    key = kvp.Key;
                    return true;
                }
            }

            key = default!;
            return false;
        }
    }

    /// <summary>
    /// 高性能双向字典，支持O(1)复杂度的双向查找
    /// </summary>
    /// <typeparam name="TFirst">第一种类型</typeparam>
    /// <typeparam name="TSecond">第二种类型</typeparam>
    public class BidirectionalDictionary<TFirst, TSecond>
    {
        private readonly Dictionary<TFirst, TSecond> _firstToSecond;
        private readonly Dictionary<TSecond, TFirst> _secondToFirst;

        /// <summary>
        /// 初始化双向字典
        /// </summary>
        public BidirectionalDictionary()
        {
            _firstToSecond = new Dictionary<TFirst, TSecond>();
            _secondToFirst = new Dictionary<TSecond, TFirst>();
        }

        /// <summary>
        /// 初始化双向字典，指定初始容量
        /// </summary>
        /// <param name="capacity">初始容量</param>
        public BidirectionalDictionary(int capacity)
        {
            _firstToSecond = new Dictionary<TFirst, TSecond>(capacity);
            _secondToFirst = new Dictionary<TSecond, TFirst>(capacity);
        }

        /// <summary>
        /// 获取字典中的元素数量
        /// </summary>
        public int Count => _firstToSecond.Count;

        /// <summary>
        /// 获取所有第一类型的键
        /// </summary>
        public ICollection<TFirst> FirstKeys => _firstToSecond.Keys;

        /// <summary>
        /// 获取所有第二类型的键
        /// </summary>
        public ICollection<TSecond> SecondKeys => _secondToFirst.Keys;

        /// <summary>
        /// 添加键值对
        /// </summary>
        /// <param name="first">第一类型值</param>
        /// <param name="second">第二类型值</param>
        /// <exception cref="ArgumentException">当键已存在时抛出</exception>
        public void Add(TFirst first, TSecond second)
        {
            if (_firstToSecond.ContainsKey(first))
                throw new ArgumentException($"Key '{first}' already exists in the dictionary.", nameof(first));

            if (_secondToFirst.ContainsKey(second))
                throw new ArgumentException($"Key '{second}' already exists in the dictionary.", nameof(second));

            _firstToSecond.Add(first, second);
            _secondToFirst.Add(second, first);
        }

        /// <summary>
        /// 尝试添加键值对
        /// </summary>
        /// <param name="first">第一类型值</param>
        /// <param name="second">第二类型值</param>
        /// <returns>是否成功添加</returns>
        public bool TryAdd(TFirst first, TSecond second)
        {
            if (_firstToSecond.ContainsKey(first) || _secondToFirst.ContainsKey(second))
                return false;

            _firstToSecond.Add(first, second);
            _secondToFirst.Add(second, first);
            return true;
        }

        /// <summary>
        /// 根据第一类型值获取第二类型值
        /// </summary>
        /// <param name="first">第一类型值</param>
        /// <returns>对应的第二类型值</returns>
        public TSecond GetByFirst(TFirst first)
        {
            return _firstToSecond[first];
        }

        /// <summary>
        /// 根据第二类型值获取第一类型值
        /// </summary>
        /// <param name="second">第二类型值</param>
        /// <returns>对应的第一类型值</returns>
        public TFirst GetBySecond(TSecond second)
        {
            return _secondToFirst[second];
        }

        /// <summary>
        /// 尝试根据第一类型值获取第二类型值
        /// </summary>
        /// <param name="first">第一类型值</param>
        /// <param name="second">输出的第二类型值</param>
        /// <returns>是否找到</returns>
        public bool TryGetByFirst(TFirst first, out TSecond second)
        {
            return _firstToSecond.TryGetValue(first, out second!);
        }

        /// <summary>
        /// 尝试根据第二类型值获取第一类型值
        /// </summary>
        /// <param name="second">第二类型值</param>
        /// <param name="first">输出的第一类型值</param>
        /// <returns>是否找到</returns>
        public bool TryGetBySecond(TSecond second, out TFirst first)
        {
            return _secondToFirst.TryGetValue(second, out first!);
        }

        /// <summary>
        /// 检查是否包含指定的第一类型键
        /// </summary>
        /// <param name="first">第一类型值</param>
        /// <returns>是否包含</returns>
        public bool ContainsFirst(TFirst first)
        {
            return _firstToSecond.ContainsKey(first);
        }

        /// <summary>
        /// 检查是否包含指定的第二类型键
        /// </summary>
        /// <param name="second">第二类型值</param>
        /// <returns>是否包含</returns>
        public bool ContainsSecond(TSecond second)
        {
            return _secondToFirst.ContainsKey(second);
        }

        /// <summary>
        /// 根据第一类型键移除键值对
        /// </summary>
        /// <param name="first">第一类型值</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveByFirst(TFirst first)
        {
            if (_firstToSecond.TryGetValue(first, out var second))
            {
                _firstToSecond.Remove(first);
                _secondToFirst.Remove(second);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据第二类型键移除键值对
        /// </summary>
        /// <param name="second">第二类型值</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveBySecond(TSecond second)
        {
            if (_secondToFirst.TryGetValue(second, out var first))
            {
                _firstToSecond.Remove(first);
                _secondToFirst.Remove(second);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清空字典
        /// </summary>
        public void Clear()
        {
            _firstToSecond.Clear();
            _secondToFirst.Clear();
        }
    }
}