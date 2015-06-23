using System.Collections.Generic;

namespace Trimble.Ems.Cross.Types
{
    public class LRUCacheHelper<K, V>
    {
        readonly Dictionary<K, V> _dict;
        readonly LinkedList<K> _queue = new LinkedList<K>();
        readonly object _syncRoot = new object();
        private readonly int _max;
        public LRUCacheHelper(int capacity)
        {
            _dict = new Dictionary<K, V>(capacity);
            _max = capacity;
        }

        public void Add(K key, V value)
        {
            lock (_syncRoot)
            {
                CheckCapacity();
                _queue.AddLast(key);                            //O(1)
                _dict[key] = value;                             //O(1)
            }
        }

        private void CheckCapacity()
        {
            lock (_syncRoot)
            {
                int count = _dict.Count;                        //O(1)
                if (count == _max)
                {
                    // cache full, so re-use the oldest node
                    var node = _queue.First;
                    _dict.Remove(node.Value);                   //O(1)
                    _queue.RemoveFirst();                       //O(1)
                }
            }
        }

        public void Delete(K key)
        {
            lock (_syncRoot)
            {
                _dict.Remove(key);                              //O(1)
                _queue.Remove(key);                             //O(n)
            }
        }

        public V Get(K key)
        {
            lock (_syncRoot)
            {
                V ret;
                if (_dict.TryGetValue(key, out ret))            //O(1)
                {
                    _queue.Remove(key);                         //O(n)
                    _queue.AddLast(key);                        //O(1)
                }
                return ret;
            }
        }
    }
}
