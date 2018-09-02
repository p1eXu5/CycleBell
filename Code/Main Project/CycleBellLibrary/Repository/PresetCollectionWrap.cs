using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;
using CycleBellLibrary.Repository;

namespace CycleBellLibrary.Context
{
    public sealed class PresetCollectionWrap : IEnumerable<Preset>, IInnerPresetCollectionWrap
    {
        #region Fields

        private readonly PresetObservableCollection _presets;

        #endregion

        #region Constructor

        public PresetCollectionWrap()
        {
            _presets = new PresetObservableCollection();
            Presets = new ReadOnlyObservableCollection<Preset>(_presets);
        }

        #endregion

        #region Properties

        public ReadOnlyObservableCollection<Preset> Presets { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Clear preset collection
        /// </summary>
        public void Clear()
        {
            if (_presets.Count > 0) {
                _presets.Clear();
            }
        }

        /// <summary>
        /// Deserializes or creates new empty preset
        /// </summary>
        public void LoadFromFile(string fileName)
        {
            CheckFileName (fileName);
            DeserializePresets(fileName);
        }

        /// <summary>
        /// Adds preset to preset collection. If name of a new preset consists in collection then added "_copy" to the it
        /// </summary>
        /// <param name="preset"></param>
        public void Add (Preset preset)
        {
            if (preset == null)
                throw new ArgumentNullException (nameof(preset), "preset can't be null");

            if (preset.PresetName == null)
                throw new ArgumentNullException (nameof(preset.PresetName), "PresetName can't be null");

            if (_presets.Any (p => p.PresetName == preset.PresetName)) 
                preset.PresetName += "_copy";

            _presets.Add (preset);
        }

        public void Remove (Preset preset)
        {
            if (preset == null)
                throw new ArgumentNullException (nameof(preset), "preset can't be null");

            var res = _presets.Remove (preset);

            if (!res)
                throw new ArgumentException("preset doesn't exists", nameof(preset));
        }

        /// <summary>
        /// Serializes presets, for a while
        /// </summary>
        public void SavePresets(string fileName)
        {
            CheckFileName(fileName);
            SerializePresets(fileName);
        }

        /// <summary>
        /// Checks input file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <exception cref="ArgumentException"></exception>
        private void CheckFileName (string fileName)
        {
            if (String.IsNullOrEmpty (fileName) || !File.Exists (fileName))
                throw new ArgumentException("Invalid file name or file doesn't exist", nameof(fileName));
        }


        /// <summary>
        /// Serializes presets
        /// </summary>
        private void SerializePresets(string fileName)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Create)) {

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(PresetObservableCollection));
                xmlSerializer.Serialize(fs, _presets);
            }
        }

        /// <summary>
        /// Deserialies presets
        /// </summary>
        private void DeserializePresets(string fileName)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PresetObservableCollection));
            using (FileStream fStream = File.OpenRead(fileName)) {

                var presets = (PresetObservableCollection)xmlSerializer.Deserialize(fStream);

                if (presets.Count > 0) {
                    _presets.Clear();
                    foreach (var preset in presets) {
                        _presets.Add (preset);
                    }
                }
            }
        }


        #endregion

        public IEnumerator<Preset> GetEnumerator() => _presets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}
