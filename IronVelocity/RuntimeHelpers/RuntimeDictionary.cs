using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.RuntimeHelpers
{
    public class RuntimeDictionary : IDictionary<string, object>, IDictionary
    {
        private readonly Dictionary<string, object> _values;
        public RuntimeDictionary()
            : this(0)
        {
        }

        public RuntimeDictionary(int capacity)
        {
            _values = new Dictionary<string, object>(capacity, StringComparer.OrdinalIgnoreCase);
        }

        public object this[string key]
        {
            get
            {
                object result;
                if (_values.TryGetValue(key, out result))
                    return result;

                return null;
            }
            set
            {
                _values[key] = value;
            }
        }

        public int Count { get { return _values.Count; } }
        public ICollection<string> Keys { get { return _values.Keys; } }
        public ICollection<object> Values { get { return _values.Values; } }


        public void Add(string key, object value)
        {
            _values.Add(key, value);
        }


        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _values.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _values.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _values.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _values.Remove(item.Key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _values.TryGetValue(key, out value);
        }



        bool ICollection<KeyValuePair<string, object>>.IsReadOnly { get { return ((ICollection<KeyValuePair<string, object>>)_values).IsReadOnly; } }
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            ((ICollection<KeyValuePair<string, object>>)_values).Add(item);
        }
        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object>>)_values).CopyTo(array, arrayIndex);
        }



        bool IDictionary.IsFixedSize { get { return ((IDictionary)_values).IsFixedSize; } }
        bool IDictionary.IsReadOnly { get { return ((IDictionary)_values).IsReadOnly; } }
        ICollection IDictionary.Keys { get { return ((IDictionary)_values).Keys; } }
        ICollection IDictionary.Values { get { return ((IDictionary)_values).Values; } }
        bool ICollection.IsSynchronized { get { return ((IDictionary)_values).IsSynchronized; } }
        object ICollection.SyncRoot { get { return ((IDictionary)_values).SyncRoot; } }


        void IDictionary.Add(object key, object value)
        {
            ((IDictionary)_values).Add(key, value);
        }

        bool IDictionary.Contains(object key)
        {
            return ((IDictionary)_values).Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)_values).GetEnumerator();
        }


        void IDictionary.Remove(object key)
        {
            ((IDictionary)_values).Remove(key);
        }


        object IDictionary.this[object key]
        {
            get { return ((IDictionary)_values)[key]; }
            set { ((IDictionary)_values)[key] = value; }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((IDictionary)_values).CopyTo(array, index);
        }

    }

}
