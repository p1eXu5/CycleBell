using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfControlTemplates.Annotations;

namespace WpfControlTemplates.Models
{
    public class ControlTemplateViewModel : INotifyPropertyChanged
    {

        private readonly IReadOnlyCollection<ControlTypeViewModel> _controlTypeCollection;
        private string _controlTemplateXaml;

        public ControlTemplateViewModel()
        {
            Control control = new Control();
            Type controlType = control.GetType();

            Assembly assembly = Assembly.GetAssembly (controlType);

            IList<ControlTypeViewModel> controlColl = new List<ControlTypeViewModel>();

            foreach (Type type in assembly.GetTypes()) {

                if (type.IsSubclassOf (controlType) && !type.IsAbstract && type.IsPublic)
                    controlColl.Add (new ControlTypeViewModel(type));
            }

            _controlTypeCollection = new ReadOnlyCollection<ControlTypeViewModel>(controlColl);
        }

        public IEnumerable<ControlTypeViewModel> ControlsListCollection => _controlTypeCollection;

        public ActionCommand SetSelectedItemCommand
        {
            get => new ActionCommand (p => { ControlTemplateXaml = "Done!"; });
        }

        public string ControlTemplateXaml
        {
            get => _controlTemplateXaml;
            set {
                _controlTemplateXaml = value;
                OnPropertyChanged (nameof(ControlTemplateXaml));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }

    public class ControlTypeViewModel
    {
        private readonly Type _type;

        public ControlTypeViewModel (Type type)
        {
            _type = type;
        }

        public string TypeName => _type.Name;
    }
}
