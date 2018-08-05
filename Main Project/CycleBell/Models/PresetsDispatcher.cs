using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleBell.Models
{
    public sealed class PresetsDispatcher
    {
        private PresetsDispatcher _presetsDispatcher;

        private PresetsDispatcher() { }

        public PresetsDispatcher GetPresetsDispatcher
        {
            get {
                if (_presetsDispatcher == null) {
                    _presetsDispatcher = new PresetsDispatcher();
                }

                return _presetsDispatcher;
            }
        }

        public static IEnumerable<Preset> GetPresets()
        {
            return null;
        }
    }
}
