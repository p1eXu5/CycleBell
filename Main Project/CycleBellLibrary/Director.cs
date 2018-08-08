using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;

namespace CycleBellLibrary
{
    public class Director : IDirector
    {
        #region Fields

        private PresetObservableCollection _presets;

        #endregion

        #region Constructor

        public Director()
        {
            _presets = new PresetObservableCollection();
            Presets = new ReadOnlyObservableCollection<Preset>(_presets);
        }

        public Director(string fileName) : this()
        {
            FileName = fileName;
            LoadPresets();
        }

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => _presets.CollectionChanged += value;
            remove => _presets.CollectionChanged -= value;
        }

        #endregion

        #region Properties

        public ReadOnlyObservableCollection<Preset> Presets { get; }

        public string FileName { get; set; }

        #endregion

        #region Methods

        public void LoadPresets()
        {
            //NewPresets();
            //return;

            if (!String.IsNullOrEmpty (FileName) && File.Exists (FileName)) {
                try {
                    DeserializePresets();
                }
                catch (XmlException) {
                    NewPresets();
                }
            }
            else {
               NewPresets();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NewPresets()
        {
            if (_presets.Count > 0) {
                _presets.Clear();
            }
            _presets.Add (new Preset());
        }

        public void AddNewPreset(string name = null)
        {
            var emptyPreset = _presets.FirstOrDefault(p => p.PresetName == "");

            if (emptyPreset == null) {

                _presets.Add(new Preset(name));
                return;
            }

            if (!emptyPreset.TimePoints.Any())
                return;

            // Now emptyPreset is filled and unsaved
        }

        public void AddPreset(Preset preset) => _presets.Add(preset);

        public void SavePresets()
        {
            SerializePresets();
        }

        private void SerializePresets()
        {
            using (FileStream fs = File.Open(FileName, FileMode.Create)) {

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(PresetObservableCollection));
                xmlSerializer.Serialize(fs, _presets);
            }
        }

        private void DeserializePresets()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PresetObservableCollection));

            using (FileStream fStream = File.OpenRead(FileName)) {

                _presets = (PresetObservableCollection)xmlSerializer.Deserialize(fStream);
            }
        }

        #endregion

    }
}
