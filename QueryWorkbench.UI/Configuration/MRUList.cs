using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace QueryWorkbenchUI.Configuration {
    /// <summary>
    /// Most Recently Used list helper
    /// </summary>
    [DataContract]
    public class MRUList<T> {
        private readonly int _maxItems;
        private Dictionary<int, T> _items = new Dictionary<int, T>();

        public MRUList() {
        }

        public MRUList(int maxItems = 0) {
            _maxItems = maxItems;
        }

        public void AddItem(T item) {
            if (_items.Count == 0) {
                _items.Add(0, item);
                return;
            }
            if (_items[0].Equals(item)) return;

            var values = _items.Values.Where(v => !v.Equals(item))
                                      .ToList();
            values.Insert(0, item);
            var expectedCountOnExit = _items.ContainsValue(item) ? _items.Count : _items.Count + 1;
            for (int k = 0; k < _items.Count; k++) {
                _items[k] = values[k];
            }
            if (_items.Count < expectedCountOnExit) {
                _items.Add(_items.Count, values.Last());
            }
            if (_maxItems > 0) {
                _items = _items.Take(_maxItems).ToDictionary(i => i.Key, i => i.Value);
            }
        }

        [DataMember]
        public Dictionary<int, T> Items {
            get {
                if (_items == null) {
                    _items = new Dictionary<int, T>();
                }
                return _items;
            }

        }

    }
}
