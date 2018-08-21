using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunWithObservableCollection1
{
    class Foo
    {
        private ObservableCollection<string> _coll;

        public Foo(string str) : this()
        {
            if (String.IsNullOrEmpty(str))
                _coll.Add(str);
        }
        
        public Foo(IEnumerable<string> coll) : this()
        {
            if (coll != null && coll.Any()) {
                foreach (var element in coll) {
                    
                    _coll.Add(element);
                }
            }
        }

        public Foo()
        {
            _coll = new ObservableCollection<string>();
            Coll = new ReadOnlyObservableCollection<string>(_coll);
        }

        public void ResetCollection()
        {
            _coll = new ObservableCollection<string>();
            Coll = new ReadOnlyObservableCollection<string>(_coll);
        }

        public ReadOnlyObservableCollection<string> Coll { get; private set; }

        public string this[int idx]
        {
            get => _coll[idx];
            set => _coll[idx] = value;
        }

        public void Add(string str)
        {
            _coll.Add(str);
        }

        public void Clear()
        {
            _coll.Clear();
        }


        //public event NotifyCollectionChangedEventHandler CollectionChanged
        //{
        //    add => _coll.CollectionChanged += value;
        //    remove => _coll.CollectionChanged -= value;
        //}
    }
}
