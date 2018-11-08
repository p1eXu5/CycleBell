using System;

namespace CycleBell.Base {
    public interface IDialogRegistrator
    {
        void Register<TViewModel, TView>() 
            
            where      TView : IDialog
            where TViewModel : IDialogCloseRequested;

        
        bool? ShowDialog<TViewModel> (TViewModel viewModel, Predicate<object> predicate = null) 
            
            where TViewModel: IDialogCloseRequested;
    }
}