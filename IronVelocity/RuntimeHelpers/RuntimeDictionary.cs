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

        public bool TryGetValue(string key, out object value)
        {
            return _values.TryGetValue(key, out value);
        }


        #region ICollection<KeyValuePair<TKey,TValue>>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly { get { return ((ICollection<KeyValuePair<string, object>>)_values).IsReadOnly; } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            ((ICollection<KeyValuePair<string, object>>)_values).Add(item);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object>>)_values).CopyTo(array, arrayIndex);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return _values.Remove(item.Key);
        }

        #endregion


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification="Hiding for consistency with IDictionary<TKey,TValue>")]
        bool IDictionary.IsFixedSize { get { return ((IDictionary)_values).IsFixedSize; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool IDictionary.IsReadOnly { get { return ((IDictionary)_values).IsReadOnly; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        ICollection IDictionary.Keys { get { return ((IDictionary)_values).Keys; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        ICollection IDictionary.Values { get { return ((IDictionary)_values).Values; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool ICollection.IsSynchronized { get { return ((IDictionary)_values).IsSynchronized; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        object ICollection.SyncRoot { get { return ((IDictionary)_values).SyncRoot; } }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        void IDictionary.Add(object key, object value)
        {
            ((IDictionary)_values).Add(key, value);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool IDictionary.Contains(object key)
        {
            return ((IDictionary)_values).Contains(key);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)_values).GetEnumerator();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        void IDictionary.Remove(object key)
        {
            ((IDictionary)_values).Remove(key);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        object IDictionary.this[object key]
        {
            get { return ((IDictionary)_values)[key]; }
            set { ((IDictionary)_values)[key] = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        void ICollection.CopyTo(Array array, int index)
        {
            ((IDictionary)_values).CopyTo(array, index);
        }

    }

}
