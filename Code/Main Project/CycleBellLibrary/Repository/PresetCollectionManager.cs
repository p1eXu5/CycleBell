﻿using System;
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

namespace CycleBellLibrary.Repository
{
    public sealed class PresetCollectionManager :  IPresetCollectionManager, IInnerPresetCollectionManager, IEnumerable<Preset>
    {
        #region Fields

        private readonly PresetSerializableObservableCollection _presetsSerializable;

        #endregion

        #region Constructor

        public PresetCollectionManager()
        {
            _presetsSerializable = new PresetSerializableObservableCollection();
            Presets = new ReadOnlyObservableCollection<Preset>(_presetsSerializable);
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
            if (_presetsSerializable.Count > 0) {
                _presetsSerializable.Clear();
            }
        }

        /// <summary>
        /// Deserializes or creates new empty preset
        /// </summary>
        public void OpenPresets(string fileName)
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

            if (_presetsSerializable.Any (p => p.PresetName == preset.PresetName)) 
                preset.PresetName += "_copy";

            _presetsSerializable.Add (preset);
        }

        public void Remove (Preset preset)
        {
            if (preset == null)
                throw new ArgumentNullException (nameof(preset), "preset can't be null");

            var res = _presetsSerializable.Remove (preset);

            if (!res)
                throw new ArgumentException("preset doesn't exists", nameof(preset));
        }

        /// <summary>
        /// Serializes presets, for a while
        /// </summary>
        public void SavePresets(string fileName)
        {
            try {
                CheckFileName (fileName);
            }
            catch (ArgumentException) {
            }

            SerializePresets(fileName);
        }

        /// <summary>
        /// Checks input file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <exception cref="ArgumentException"></exception>
        private void CheckFileName (string fileName)
        {
            if (String.IsNullOrEmpty (fileName))
                throw new ArgumentNullException(nameof(fileName), "File name is null or empty");

            if (!File.Exists (fileName))
                throw new ArgumentException("File doesn't exist", nameof(fileName));
        }

        /// <summary>
        /// Serializes presets
        /// </summary>
        private void SerializePresets(string fileName)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Create)) {

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(PresetSerializableObservableCollection));
                xmlSerializer.Serialize(fs, _presetsSerializable);
            }
        }

        /// <summary>
        /// Deserialies presets
        /// </summary>
        private void DeserializePresets(string fileName)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PresetSerializableObservableCollection));
            using (FileStream fStream = File.OpenRead(fileName)) {

                var presets = (PresetSerializableObservableCollection)xmlSerializer.Deserialize(fStream);

                if (presets.Count > 0) {
                    _presetsSerializable.Clear();
                    foreach (var preset in presets) {
                        _presetsSerializable.Add (preset);
                    }
                }
            }
        }

        #endregion

        #region IEnumerable Implementation

        public IEnumerator<Preset> GetEnumerator() => _presetsSerializable.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        #endregion

    }
}