using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

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
                if (value != null)
                    _values[key] = value;
            }
        }

        public int Count => _values.Count;
        public ICollection<object> Keys => _values.Keys;
        public ICollection<object> Values => _values.Values;
        public void Add(object key, object value) =>  _values[key] = value;
        public void Clear() => _values.Clear();
        public bool Contains(KeyValuePair<object, object> item) => _values.Contains(item);
        public bool ContainsKey(object key) => _values.ContainsKey(key);
        public IEnumerator<KeyValuePair<object, object>> GetEnumerator() => _values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
        public void Remove(object key) => _values.Remove(key);
        public bool TryGetValue(object key, out object value) => _values.TryGetValue(key, out value);

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "For consistency with HybridDictionary, need a void implementation of Remove to be the main visible method")]
        bool IDictionary<object, object>.Remove(object key) => _values.Remove(key);

        #region ICollection<KeyValuePair<TKey,TValue>>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool ICollection<KeyValuePair<object, object>>.IsReadOnly => ((ICollection<KeyValuePair<object, object>>)_values).IsReadOnly;
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        void ICollection<KeyValuePair<object, object>>.Add(KeyValuePair<object, object> item) => Add(item.Key, item.Value);
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        void ICollection<KeyValuePair<object, object>>.CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) => ((ICollection<KeyValuePair<object, object>>)_values).CopyTo(array, arrayIndex);
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool ICollection<KeyValuePair<object, object>>.Remove(KeyValuePair<object, object> item) => _values.Remove(item.Key);

        #endregion


        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool IDictionary.IsFixedSize => ((IDictionary)_values).IsFixedSize;
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool IDictionary.IsReadOnly => ((IDictionary)_values).IsReadOnly;
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        ICollection IDictionary.Keys => _values.Keys;
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        ICollection IDictionary.Values => _values.Values;
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool ICollection.IsSynchronized => ((IDictionary)_values).IsSynchronized;
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        object ICollection.SyncRoot => ((IDictionary)_values).SyncRoot;




        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        bool IDictionary.Contains(object key) => _values.ContainsKey(key);

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        IDictionaryEnumerator IDictionary.GetEnumerator() => _values.GetEnumerator();



        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hiding for consistency with IDictionary<TKey,TValue>")]
        void ICollection.CopyTo(Array array, int index) => ((IDictionary)_values).CopyTo(array, index);

        private class KeyComparer : IEqualityComparer<object>
        {
            private IEqualityComparer _comparer = StringComparer.OrdinalIgnoreCase;

            bool IEqualityComparer<object>.Equals(object x, object y) => _comparer.Equals(x, y);
            int IEqualityComparer<object>.GetHashCode(object obj) => _comparer.GetHashCode(obj);
        }

    }
}
