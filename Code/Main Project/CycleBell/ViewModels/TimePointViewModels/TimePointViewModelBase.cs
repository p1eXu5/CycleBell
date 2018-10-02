using System;
using CycleBell.Annotations;
using CycleBell.Base;
using CycleBellLibrary.Models;

namespace CycleBell.ViewModels.TimePointViewModels
{
    public abstract class TimePointViewModelBase : ObservableObject
    {
        #region Fields

        private int _id;
        private byte _loopNumber;

        protected IPresetViewModel _PresetViewModel;

        private bool _isEnabled = true;
        private bool _isActive = true;

        #endregion

        #region Constructors

        protected TimePointViewModelBase (int id, byte loopNumber, IPresetViewModel presetViewModel)
        {
            _PresetViewModel = presetViewModel;

            _id = id;
            _loopNumber = loopNumber;
        }

        #endregion

        #region Properties

        public abstract TimePoint TimePoint { get; }

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

        public virtual bool IsEnabled
        {
            get => _isEnabled;
            set {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Operators

        public static bool operator == (TimePointViewModelBase baseTpvm, TimePoint timePoint)
        {
            if (ReferenceEquals(baseTpvm, null) || ReferenceEquals(timePoint, null)) {

                return false;
            }

            if (baseTpvm is TimePointViewModel tpvm) {

                // TODO: bag is the equality null == null is true
                return tpvm.TimePoint == timePoint;
            }

            return false;
        }

        public static bool operator != (TimePointViewModelBase baseTpvm, TimePoint timePoint)
        {
            return !(baseTpvm == timePoint);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Object.ReferenceEquals(this, obj);
        }

        #endregion
    }
}