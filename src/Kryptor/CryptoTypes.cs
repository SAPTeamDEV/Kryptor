﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Specifies the crypto provider type.
    /// </summary>
    public enum CryptoTypes
    {
        /// <summary>
        /// Invalid crypto provider.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Provides Standalone Key (SK) Crypto mechanism.
        /// In this way, each 31 bytes of data is encrypted with a different key Until all the keys are used, then it continues from the first key and this process continues until the end of encryption.
        /// </summary>
        SK = 1
    }
}