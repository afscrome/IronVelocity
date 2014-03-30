using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IronVelocity.Runtime
{
    public class RuntimeDictionary : IDictionary<object, object>, IDictionary
    {
        private static IEqualityComparer<object> _comparer = new KeyComparer();

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly Dictionary<object, object> _values;
        public RuntimeDictionary()
            : this(0)
        {
        }

        public RuntimeDictionary(int capacity)
        {
            _values = new Dictionary<object, object>(capacity, _comparer);
        }

        public object this[object key]
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
        public ICollection<object> Keys { get { return _values.Keys; } }
        public ICollection<object> Values { get { return _values.Values; } }

        public void Add(object key, object value)
        {
            _values.Add(key, value);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(KeyValuePair<object, object> item)
        {
            return _values.Contains(item);
        }

        public bool ContainsKey(object key)
        {
            return _values.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public void Remove(object key)
        {
            _values.Remove(key);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "For consistency with HybridDictionary, need a void implementation of Remove to be the main visible method")]
        bool IDictionary<object, object>.Remove(object key)
        {
            return _values.Remove(key);
        }

        public bool TryGetValue(object key, out object value)
        {
            return _values.TryGetValue(key, out value);
        }


        #region ICollection<KeyValuePair<TKey,TValue>>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool ICollection<KeyValuePair<object, object>>.IsReadOnly { get { return ((ICollection<KeyValuePair<object, object>>)_values).IsReadOnly; } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        void ICollection<KeyValuePair<object, object>>.Add(KeyValuePair<object, object> item)
        {
            ((ICollection<KeyValuePair<object, object>>)_values).Add(item);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        void ICollection<KeyValuePair<object, object>>.CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<object, object>>)_values).CopyTo(array, arrayIndex);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool ICollection<KeyValuePair<object, object>>.Remove(KeyValuePair<object, object> item)
        {
            return _values.Remove(item.Key);
        }

        #endregion


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification="Hiding for consistency with IDictionary<TKey,TValue>")]
        bool IDictionary.IsFixedSize { get { return ((IDictionary)_values).IsFixedSize; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool IDictionary.IsReadOnly { get { return ((IDictionary)_values).IsReadOnly; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        ICollection IDictionary.Keys { get { return _values.Keys; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        ICollection IDictionary.Values { get { return _values.Values; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool ICollection.IsSynchronized { get { return ((IDictionary)_values).IsSynchronized; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        object ICollection.SyncRoot { get { return ((IDictionary)_values).SyncRoot; } }




        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool IDictionary.Contains(object key)
        {
            return _values.ContainsKey(key);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return _values.GetEnumerator();
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        void ICollection.CopyTo(Array array, int index)
        {
            ((IDictionary)_values).CopyTo(array, index);
        }

        private class KeyComparer : IEqualityComparer<object>
        {
            private IEqualityComparer _comparer = StringComparer.OrdinalIgnoreCase;

            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                return _comparer.Equals(x, y);
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                return _comparer.GetHashCode(obj);
            }
        }

    }
}
