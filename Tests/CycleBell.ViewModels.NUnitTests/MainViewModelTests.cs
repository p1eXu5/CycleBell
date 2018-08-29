using System;
using CycleBell.Base;
using CycleBellLibrary;
using NUnit.Framework;

namespace CycleBell.ViewModels.NUnitTests
{
    [TestFixture]
    public class MainViewModelTests
    {
        [SetUp]
        public void TestInitializer()
        {}

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
            public IPresetsManager PresetsManager { get; }
            public ITimerManager TimerManager { get; }
            public string FileName { get; }

            public void CreateNewPreset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
