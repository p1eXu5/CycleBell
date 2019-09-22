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

namespace CycleBell.ViewModels
{
    public struct TimeSpanDigits
    {
        private char? _sign;
        private byte? _majorH;
        private byte _minorH;
        private byte _majorM;
        private byte _minorM;
        private byte _majorS;
        private byte _minorS;
        private int _milliseconds;

        public char? Sign => _sign;
        public byte? MajorH => _majorH;
        public byte MinorH => _minorH;
        public byte MajorM => _majorM;
        public byte MinorM => _minorM;
        public byte MajorS => _majorS;
        public byte MinorS => _minorS;
        public int Milliseconds => _milliseconds;

        public static TimeSpanDigits Parse (TimeSpan timeSpan)
        {
            TimeSpanDigits res = default (TimeSpanDigits);

            if (timeSpan < TimeSpan.Zero) {
                res._sign = '-';
                timeSpan = timeSpan.Negate();
            }

            if (timeSpan.Hours > 9) {
                Decompose(timeSpan.Hours, out res._majorH, out res._minorH);
            }
            else {
                res._minorH = (byte)timeSpan.Hours;
            }

            Decompose(timeSpan.Minutes, out res._majorM, out res._minorM);
            Decompose(timeSpan.Seconds, out res._majorS, out res._minorS);

            res._milliseconds = timeSpan.Milliseconds;

            return res;
        }

        private static void Decompose (int timeComponent, out byte majorDigit, out byte minorDigit)
        {
            majorDigit = (byte) (timeComponent / 10);
            minorDigit = (byte) (timeComponent - (majorDigit * 10));
        }

        private static void Decompose (int timeComponent, out byte? majorDigit, out byte minorDigit)
        {
            majorDigit = (byte) (timeComponent / 10);
            minorDigit = (byte) (timeComponent - (majorDigit * 10));
        }
    }
}
