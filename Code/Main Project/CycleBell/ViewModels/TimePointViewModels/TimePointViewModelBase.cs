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
        protected bool _IsEnabled;

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

        public virtual bool IsRunning => _PresetViewModel.IsRunning;

        public virtual bool IsEnabled
        {
            get => IsRunning || _IsEnabled;
            set {
                _IsEnabled = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Operators

        public static bool operator == (TimePointViewModelBase baseTpvm, TimePoint timePoint)
        {
            if (baseTpvm is TimePointViewModel tpvm) {

                return tpvm.TimePoint == timePoint;
            }

            return false;
        }

        public static bool operator != (TimePointViewModelBase baseTpvm, TimePoint timePoint)
        {
            return !(baseTpvm == timePoint);
        }

        #endregion
    }
}