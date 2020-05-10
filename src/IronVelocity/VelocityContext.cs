using System;
using System.Collections;
using System.Collections.Generic;

namespace IronVelocity
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification="Implementing IDictionary for compatibility with NVelocity")]
    public class VelocityContext : IDictionary<string, object>, IReadOnlyDictionary<string, object>
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

        public int Count => _variables.Count;
        public bool IsReadOnly => _variables.IsReadOnly;
        public ICollection<string> Keys => _variables.Keys;
        public ICollection<object> Values => _variables.Values;

        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Keys;
        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Values;

        public void Add(string key, object value) => _variables.Add(key, value);
        public void Add(KeyValuePair<string, object> item) => _variables.Add(item);
        public void Clear() => _variables.Clear();
        public bool Contains(KeyValuePair<string, object> item) => _variables.Contains(item);
        public bool ContainsKey(string key) => _variables.ContainsKey(key);
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => _variables.CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _variables.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _variables.GetEnumerator();
        public bool Remove(string key) => _variables.Remove(key);
        public bool Remove(KeyValuePair<string, object> item) => _variables.Remove(item);
        public bool TryGetValue(string key, out object value) => _variables.TryGetValue(key, out value);
    }

}
