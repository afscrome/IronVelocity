using System;
using System.Collections;
using System.Collections.Generic;

namespace IronVelocity
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification="Implementing IDictionary for compatibility with NVelocity")]
    public class VelocityContext : IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _variables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public VelocityContext()
        {
        }

        public VelocityContext(IDictionary<string, object> values)
        {
            if (values != null)
            {
                foreach (var variable in values)
                    _variables.Add(variable);

            }
        }

        public object this[string key]
        {
            get
            {
                object result;
                if (_variables.TryGetValue(key, out result))
                    return result;

                return null;
            }
            set
            {
                if (value != null)
                    _variables[key] = value;
            }
        }

        public int Count { get { return _variables.Count; } }
        public bool IsReadOnly { get { return _variables.IsReadOnly; } }
        public ICollection<string> Keys { get { return _variables.Keys; } }
        public ICollection<object> Values { get { return _variables.Values; } }


        public void Add(string key, object value)
        {
            _variables.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _variables.Add(item);
        }

        public void Clear()
        {
            _variables.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _variables.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _variables.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _variables.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _variables.GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _variables.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _variables.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _variables.Remove(item);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _variables.TryGetValue(key, out value);
        }
    }

}
