using System;
using CycleBell.Base;

namespace CycleBell.ViewModels 
{
    public abstract class TimePointViewModelBase : ObservableObject
    {
        #region Fields

        private int _id;
        private byte _loopNumber;

        #endregion
        
        #region Constructors

        protected TimePointViewModelBase (int id, byte loopNumber)
        {
            _id = id;
            _loopNumber = loopNumber;
        }

        #endregion

        #region Properties

        public virtual int Id
        {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged();
            }
        }

        public virtual byte LoopNumber
        {
            get => _loopNumber;
            set {
                _loopNumber = value;
                OnPropertyChanged();
            }
        }
        
        #endregion
    }
}