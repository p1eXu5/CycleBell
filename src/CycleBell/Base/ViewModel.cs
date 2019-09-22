using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleBell.Base
{
    public abstract class ViewModel : ObservableObject, IDataErrorInfo
    {
        public string this[string columnName] => OnValidate(columnName);

        public string Error => throw new NotImplementedException();

        protected virtual string OnValidate(string columnName)
        {
            var validationContext = new ValidationContext(this) { MemberName = columnName };

            var validationResult = new Collection<ValidationResult>();

            var isValidate = Validator.TryValidateObject(this, validationContext, validationResult, true);

            return !isValidate ? validationResult[0].ErrorMessage : null;
        }
    }
}
