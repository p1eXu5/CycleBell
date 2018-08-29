using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CycleBell.Base;
using CycleBellLibrary;
using Moq;
using NUnit.Framework;

namespace CycleBell.ViewModels.NUnitTests
{
    [TestFixture]
    public class MainViewModelTests
    {
        private Mock<IPresetsManager> mockPresetsManager = new Mock<IPresetsManager>();

        [SetUp]
        public void TestInitializer()
        {
        }

        [Test]
        public void class_TypeCheck_IsObservableObject()
        {
            Type actualType = typeof(MainViewModel).BaseType;
            Type expectedType = typeof(ObservableObject);

            Assert.AreEqual(expectedType, actualType);
        }

        [Test]
        public void CreatePresetCommand_PresetsIsEmpty_AddsEmptyPreset()
        {
            var viewModel = GetStubedMainViewModel();

            viewModel.CreatePresetCommand.Execute(null);

            Assert.IsTrue (viewModel.Presets.Count == 1);
        }

        private MainViewModel GetStubedMainViewModel()
        {
            var stubDialogRegistrator = new FakeDialogRegistrator();
            var stubCycleBellManager = new FakeCycleBellManager();

            return new MainViewModel (stubDialogRegistrator, stubCycleBellManager);
        }

        internal class FakeDialogRegistrator : IDialogRegistrator
        {
            public void Register<TViewModel, TView>() where TViewModel : IDialogCloseRequested where TView : IDialog
            {
                throw new NotImplementedException();
            }

            public bool? ShowDialog<TViewModel> (TViewModel viewModel) where TViewModel : IDialogCloseRequested
            {
                throw new NotImplementedException();
            }
        }

        internal class FakeCycleBellManager : ICycleBellManager
        {
            public IPresetsManager PresetsManager { get; } = new FakePresetsManager();
            public ITimerManager TimerManager { get; }
            public string FileName { get; }

            public void CreateNewPreset()
            {
                PresetsManager.Add(null);
            }
        }

        internal class FakePresetsManager : IPresetsManager
        {
            private readonly ObservableCollection<Preset> _presets;

            public FakePresetsManager()
            {
                _presets = new ObservableCollection<Preset>();
                Presets = new ReadOnlyObservableCollection<Preset>(_presets);
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public string FileName { get; set; }
            public ReadOnlyObservableCollection<Preset> Presets { get; }
            public void Clear()
            {
                throw new NotImplementedException();
            }

            public void LoadFromFile (string fileName)
            {
                throw new NotImplementedException();
            }

            public void Add (Preset preset)
            {
                _presets.Add (Preset.EmptyPreset);
            }

            public void SavePresets()
            {
                throw new NotImplementedException();
            }
        }
    }
}
