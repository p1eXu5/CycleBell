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
    public sealed class PresetManager : IPresetManager
    {
        #region Fields

        private PresetObservableCollection _presets;

        #endregion

        #region Constructor

        public PresetManager()
        {
            _presets = new PresetObservableCollection();
            Presets = new ReadOnlyObservableCollection<Preset>(_presets);
        }

        public PresetManager(string fileName) : this()
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

        public ReadOnlyObservableCollection<Preset> Presets { get; private set; }

        public string FileName { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Deserializes or creates new empty preset
        /// </summary>
        public void LoadPresets()
        {
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

        /// <summary>
        /// Clears exists presets if they are and creates new empty preset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NewPresets()
        {
            if (_presets.Count > 0) {
                _presets.Clear();
            }
            _presets.Add (new Preset());
        }

        /// <summary>
        /// Adds empty preset if it isn't in preset collection
        /// </summary>
        /// <param name="name"></param>
        public void AddNewEmptyPreset(string name = null)
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

        /// <summary>
        /// Adds preset to preset collection
        /// </summary>
        /// <param name="preset"></param>
        public void AddPreset(Preset preset) => _presets.Add(preset);

        /// <summary>
        /// Serializes presets, for a while
        /// </summary>
        public void SavePresets()
        {
            SerializePresets();
        }

        /// <summary>
        /// Serializes presets
        /// </summary>
        private void SerializePresets()
        {
            using (FileStream fs = File.Open(FileName, FileMode.Create)) {

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(PresetObservableCollection));
                xmlSerializer.Serialize(fs, _presets);
            }
        }

        /// <summary>
        /// Deserialies presets
        /// </summary>
        private void DeserializePresets()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PresetObservableCollection));

            using (FileStream fStream = File.OpenRead(FileName)) {

                _presets = (PresetObservableCollection)xmlSerializer.Deserialize(fStream);
                Presets = new ReadOnlyObservableCollection<Preset>(_presets);
            }
        }

        #endregion

    }
}
