/*
 * Copyright © 2018 Vladimir Likhatskiy. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *          http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *  
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CycleBellLibrary.Models;

namespace CycleBellLibrary.Repository
{
    public sealed class PresetCollectionManager :  IInnerPresetCollectionManager
    {
        #region Fields

        private readonly PresetSerializableObservableCollection _presetSerializableCollection;

        #endregion

        #region Constructor

        public PresetCollectionManager()
        {
            Preset.AutoUpdateTimePointBaseTimes = true;

            _presetSerializableCollection = new PresetSerializableObservableCollection();
            Presets = new ReadOnlyObservableCollection<Preset>(_presetSerializableCollection);
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
            if (_presetSerializableCollection.Count > 0) {
                _presetSerializableCollection.Clear();
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

            if (_presetSerializableCollection.Any (p => p.PresetName == preset.PresetName)) 
                preset.PresetName += "_copy";

            _presetSerializableCollection.Add (preset);
        }

        public void Remove (Preset preset)
        {
            if (preset == null)
                throw new ArgumentNullException (nameof(preset), "preset can't be null");

            var res = _presetSerializableCollection.Remove (preset);

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
            catch (FileNotFoundException) {
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
                throw new FileNotFoundException("File doesn't exist", nameof(fileName));
        }

        /// <summary>
        /// Serializes presets
        /// </summary>
        private void SerializePresets(string fileName)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Create)) {

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(PresetSerializableObservableCollection));
                xmlSerializer.Serialize(fs, _presetSerializableCollection);
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

                    foreach (var preset in presets) {
                        _presetSerializableCollection.Add (preset);
                    }
                }
            }
        }

        #endregion

        #region IEnumerable Implementation

        public IEnumerator<Preset> GetEnumerator() => _presetSerializableCollection.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        #endregion

    }
}
