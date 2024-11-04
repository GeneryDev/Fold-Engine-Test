using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FoldEngine.Util {
    [DebuggerTypeProxy(typeof(OrderedList<,>.OrderedListDebugView))]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class OrderedList<TK, TV> : List<TV>, ICollection<TV> where TV : struct where TK : IComparable<TK> {
        public delegate TK SortingKeyFunction(TV t);

        private readonly SortingKeyFunction _sortingKeyFunction;

        public OrderedList(SortingKeyFunction sortingKeyFunction) {
            _sortingKeyFunction = sortingKeyFunction;
        }

        public new void Add(TV item) {
            Insert(FindIndexForKey(_sortingKeyFunction(item)), item);
        }

        public TV? GetByKey(TK key) {
            int index = FindIndexForKey(key);
            if(index < 0 || index >= Count) return null;
            TV element = this[index];
            if(_sortingKeyFunction(element).CompareTo(key) != 0) return null;
            return element;
        }

        public TV? GetClosestByKey(TK key) {
            int index = FindIndexForKey(key);
            if(index < 0 || index >= Count) return null;
            return this[index];
        }

        public int FindIndexForKey(TK key) {
            if(Count == 0) return 0;

            int minIndex = 0; // inclusive
            int maxIndex = Count; // exclusive

            if(key.CompareTo(_sortingKeyFunction(this[minIndex])) < 0) {
                return minIndex;
            }

            if(key.CompareTo(_sortingKeyFunction(this[maxIndex - 1])) > 0) {
                return maxIndex;
            }

            while(minIndex < maxIndex) {
                int pivotIndex = (minIndex + maxIndex) / 2;

                TK pivotKey = _sortingKeyFunction(this[pivotIndex]);
                if(pivotKey.CompareTo(key) == 0) {
                    return pivotIndex;
                }

                if(key.CompareTo(pivotKey) > 0) {
                    minIndex = pivotIndex + 1;
                } else {
                    maxIndex = pivotIndex;
                }
            }

            return minIndex;
        }

        private sealed class OrderedListDebugView {
            private readonly ICollection<TV> _collection;

            public OrderedListDebugView(ICollection<TV> collection) {
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public TV[] Items {
                get {
                    var array = new TV[_collection.Count];
                    _collection.CopyTo(array, 0);
                    return array;
                }
            }
        }
    }
}