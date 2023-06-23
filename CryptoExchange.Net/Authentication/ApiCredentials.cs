﻿using System;
using System.IO;
using System.Security;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Api credentials, used to sign requests accessing private endpoints
    /// </summary>
    public class ApiCredentials: IDisposable
    {
        /// <summary>
        /// The api key to authenticate requests
        /// </summary>
        public SecureString? Key { get; }

        /// <summary>
        /// The api secret to authenticate requests
        /// </summary>
        public SecureString? Secret { get; }

        /// <summary>
        /// Type of the credentials
        /// </summary>
        public ApiCredentialsType CredentialType { get; }

        /// <summary>
        /// Create Api credentials providing an api key and secret for authentication
        /// </summary>
        /// <param name="key">The api key used for identification</param>
        /// <param name="secret">The api secret used for signing</param>
        public ApiCredentials(SecureString key, SecureString secret) : this(key, secret, ApiCredentialsType.Hmac)
        {
        }

        /// <summary>
        /// Create Api credentials providing an api key and secret for authentication
        /// </summary>
        /// <param name="key">The api key used for identification</param>
        /// <param name="secret">The api secret used for signing</param>
        /// <param name="credentialsType">The type of credentials</param>
        public ApiCredentials(SecureString key, SecureString secret, ApiCredentialsType credentialsType)
        {
            if (key == null || secret == null)
                throw new ArgumentException("Key and secret can't be null/empty");

            CredentialType = credentialsType;
            Key = key;
            Secret = secret;
        }

        /// <summary>
        /// Create Api credentials providing an api key and secret for authentication
        /// </summary>
        /// <param name="key">The api key used for identification</param>
        /// <param name="secret">The api secret used for signing</param>
        public ApiCredentials(string key, string secret) : this(key, secret, ApiCredentialsType.Hmac)
        {
        }

        /// <summary>
        /// Create Api credentials providing an api key and secret for authentication
        /// </summary>
        /// <param name="key">The api key used for identification</param>
        /// <param name="secret">The api secret used for signing</param>
        /// <param name="credentialsType">The type of credentials</param>
        public ApiCredentials(string key, string secret, ApiCredentialsType credentialsType)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
                throw new ArgumentException("Key and secret can't be null/empty");

            CredentialType = credentialsType;
            Key = key.ToSecureString();
            Secret = secret.ToSecureString();
        }

        /// <summary>
        /// Copy the credentials
        /// </summary>
        /// <returns></returns>
        public virtual ApiCredentials Copy()
        {
            // Use .GetString() to create a copy of the SecureString
            return new ApiCredentials(Key!.GetString(), Secret!.GetString(), CredentialType);
        }

        /// <summary>
        /// Create Api credentials providing a stream containing json data. The json data should include two values: apiKey and apiSecret
        /// </summary>
        /// <param name="inputStream">The stream containing the json data</param>
        /// <param name="identifierKey">A key to identify the credentials for the API. For example, when set to `binanceKey` the json data should contain a value for the property `binanceKey`. Defaults to 'apiKey'.</param>
        /// <param name="identifierSecret">A key to identify the credentials for the API. For example, when set to `binanceSecret` the json data should contain a value for the property `binanceSecret`. Defaults to 'apiSecret'.</param>
        public ApiCredentials(Stream inputStream, string? identifierKey = null, string? identifierSecret = null)
        {
            using var reader = new StreamReader(inputStream, Encoding.UTF8, false, 512, true);
            
            var stringData = reader.ReadToEnd();
            var jsonData = stringData.ToJToken();
            if(jsonData == null)
                throw new ArgumentException("Input stream not valid json data");

            var key = TryGetValue(jsonData, identifierKey ?? "apiKey");
            var secret = TryGetValue(jsonData, identifierSecret ?? "apiSecret");

            if (key == null || secret == null)
                throw new ArgumentException("apiKey or apiSecret value not found in Json credential file");

            Key = key.ToSecureString();
            Secret = secret.ToSecureString();            

            inputStream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Try get the value of a key from a JToken
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string? TryGetValue(JToken data, string key)
        {
            if (data[key] == null)
                return null;
            return (string) data[key]!;
        }       

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Key?.Dispose();
            Secret?.Dispose();
        }
    }
}
