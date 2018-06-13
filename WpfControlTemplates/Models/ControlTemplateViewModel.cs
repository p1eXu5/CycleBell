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
using System.Windows.Input;
using WpfControlTemplates.Annotations;
using System.Xml;
using System.Windows.Markup;

namespace WpfControlTemplates.Models
{
    public class ControlTemplateViewModel : INotifyPropertyChanged
    {
        #region private
        private readonly IReadOnlyCollection<ControlTypeViewModel> _controlTypeCollection;
        private string _controlTemplateXaml;
        private readonly Grid _grid;
        #endregion

        #region Constructor
        /// <summary>
        ///     Initializes a new instance of the WpfControlTemplates.Models.ControlTemplateViewModel class.
        /// </summary>        
        public ControlTemplateViewModel(Grid grid)
        {
            if (grid == null) throw new ArgumentNullException();

            _grid = grid;

            Control control = new Control();
            Type controlType = control.GetType();

            Assembly assembly = Assembly.GetAssembly (controlType);

            List<ControlTypeViewModel> controlColl = new List<ControlTypeViewModel>();

            foreach (Type type in assembly.GetTypes()) {

                if (type.IsSubclassOf (controlType) && !type.IsAbstract && type.IsPublic)
                    controlColl.Add (new ControlTypeViewModel(type));
            }

            controlColl.Sort(ControlTypeViewModel.TypeComparer.GetComparer);

            _controlTypeCollection = new ReadOnlyCollection<ControlTypeViewModel>(controlColl);
        }
        #endregion

        #region Properties
        /// <summary>
        ///     Collection of Control Types
        /// </summary>
        public IEnumerable<ControlTypeViewModel> ControlsListCollection => _controlTypeCollection;

        /// <summary>
        ///     Text for FlowDocument that shows Control Template XAML code
        /// </summary>
        public string ControlTemplateXaml
        {
            get => _controlTemplateXaml;
            set {
                _controlTemplateXaml = value;
                OnPropertyChanged (nameof(ControlTemplateXaml));
            }
        }
        #endregion

        #region Commands

        /// <summary>
        ///     Load Control Template XAML code for ControlTemplateXaml Property
        /// </summary>
        public ICommand SetSelectedItemCommand
        {
            get => new ActionCommand (LoadControlTemplateXaml);
        }

        #endregion

        #region private Methods

        /// <summary>
        ///     Load Control Template to flow document. Method using in command.
        /// </summary>
        /// <param name="controlTypeViewModel"></param>
        private void LoadControlTemplateXaml(object controlTypeViewModel)
        {
            if (!(controlTypeViewModel is ControlTypeViewModel)) return;

            var controlType = (controlTypeViewModel as ControlTypeViewModel).Type;
            Control control = Activator.CreateInstance(controlType) as Control;
            control.Visibility = System.Windows.Visibility.Collapsed;
            _grid.Children.Add(control);

            ControlTemplate controlTemplate = control.Template;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            StringBuilder sb = new StringBuilder();
            XmlWriter xmlWriter = XmlWriter.Create(sb, settings);
            XamlWriter.Save(controlTemplate, xmlWriter);

            ControlTemplateXaml = sb.ToString();

            _grid.Children.Remove(control);
        }

        #endregion

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
        #endregion
    }

    public class ControlTypeViewModel
    {
        private readonly Type _type;

        public ControlTypeViewModel (Type type)
        {
            _type = type;
        }

        public string TypeName => _type.Name;
        public Type Type => _type;

        public class TypeComparer : IComparer<ControlTypeViewModel>
        {
            private static TypeComparer _instance;

            private TypeComparer() { }

            public static TypeComparer GetComparer
            {
                get {

                    if (_instance == null) {
                        _instance = new TypeComparer();
                    }

                    return _instance;
                }
            }

            public int Compare(ControlTypeViewModel x, ControlTypeViewModel y)
            {
                return x.TypeName.CompareTo(y.TypeName);
            }
        }
    }
}
