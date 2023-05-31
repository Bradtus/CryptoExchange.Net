﻿using CryptoExchange.Net.Authentication;
using System;

namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Exchange options
    /// </summary>
    public class ExchangeOptions
    {
        /// <summary>
        /// Proxy settings
        /// </summary>
        public ApiProxy? Proxy { get; set; }

        /// <summary>
        /// The api credentials used for signing requests to this API.
        /// </summary>        
        public ApiCredentials? ApiCredentials { get; set; }

        /// <summary>
        /// If true, the CallResult and DataEvent objects will also include the originally received json data in the OriginalData property
        /// </summary>
        public bool OutputOriginalData { get; set; } = false;

        /// <summary>
        /// The max time a request is allowed to take
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(20);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"RequestTimeout: {RequestTimeout}, Proxy: {(Proxy == null ? "-" : "set")}, ApiCredentials: {(ApiCredentials == null ? "-" : "set")}";
        }
    }
}
