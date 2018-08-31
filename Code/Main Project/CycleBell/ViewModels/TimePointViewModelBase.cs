using CycleBell.Base;

namespace CycleBell.ViewModels 
{
    public class TimePointViewModelBase : ObservableObject
    {
        #region Fields

        private int _id;
        private byte _loopNumber;

        #endregion
        
        #region Constructors

        private TimePointViewModelBase() {}

        public TimePointViewModelBase (int id, byte loopNumber)
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