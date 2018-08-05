using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleBell.Models
{
    public class Preset
    {
        private readonly ObservableCollection<TimePoint> _points;

        public Preset() : this(""){}

        public Preset(string name)
        {
            _points = new ObservableCollection<TimePoint>();
            _points.Add(new TimePoint());

            Name = name;
        } 
        

        public string Name { get; set; }
    }
}
