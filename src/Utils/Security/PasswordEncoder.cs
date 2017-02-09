/* Copyright 2017 Cimpress

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License. */


using System;
using System.Security.Cryptography;
using System.Text;

namespace VP.FF.PT.Common.Utils.Security
{
    /// <summary>
    /// Provides basic functionality to encode/decode and encrypt/decrypt a password
    /// using extensions methods on strings
    /// 
    /// encode/decode : double base64 encoding
    /// encrypt/decrypt : internal c# encryption 
    ///     CAVEAT: would be more difficult to manually generate an encrypted/decrypted password
    /// </summary>
    public static class PasswordEncoder
    {
        // Scope of the protection to be applied (has to be the same for encryption/decryption)
        private static readonly DataProtectionScope Scope = DataProtectionScope.LocalMachine;
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Encodes a given password (text) as a simple obfuscation method
        /// </summary>
        /// <param name="plainTextPassword">the original password/plain text</param>
        /// <returns>encoded text</returns>
        public static string EncodePassword(this string plainTextPassword)
        {
            if (plainTextPassword == null)
            {
                throw new ArgumentNullException("plainTextPassword");
            }

            return
                plainTextPassword.EncodeBase64().EncodeBase64();
        }

        /// <summary>
        /// Decodes a given obfuscated/encoded text to a clear text (password)
        /// </summary>
        /// <param name="encodedPassword">encoded text/password</param>
        /// <returns>decoded text</returns>
        public static string DecodePassword(this string encodedPassword)
        {
            if (encodedPassword == null)
            {
                throw new ArgumentNullException("encodedPassword");
            }

            return
                 encodedPassword.DecodeBase64().DecodeBase64();
        }

        /// <summary>
        /// Encrypt a given password/plain text
        /// </summary>
        /// <param name="plainTextPassword">the original password/plain text</param>
        /// <returns>encrypted text/password</returns>
        public static string EncryptPassword(this string plainTextPassword)
        {
            if (plainTextPassword == null)
            {
                throw new ArgumentNullException("plainTextPassword");
            }

            var data = DefaultEncoding.GetBytes(plainTextPassword);
            byte[] encrypted = ProtectedData.Protect(data, null, Scope);

            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypts a given encrypted text/password to clear text
        /// </summary>
        /// <param name="encryptedPassword">encrypted text/password</param>
        /// <returns>decrypted text</returns>
        public static string DecryptPassword(this string encryptedPassword)
        {
            if (encryptedPassword == null)
            {
                throw new ArgumentNullException("encryptedPassword");
            }

            byte[] data = Convert.FromBase64String(encryptedPassword);

            byte[] decrypted = ProtectedData.Unprotect(data, null, Scope);
            return DefaultEncoding.GetString(decrypted);
        }

        /// <summary>
        /// Encode a text base64
        /// </summary>
        /// <param name="input">text</param>
        /// <returns>encoded text</returns>
        public static string EncodeBase64(this string input)
        {
            byte[] data = DefaultEncoding.GetBytes(input);

            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// Decode text base64
        /// </summary>
        /// <param name="input">encoded text</param>
        /// <returns>decoded text</returns>
        public static string DecodeBase64(this string input)
        {
            byte[] data = Convert.FromBase64String(input);

            return DefaultEncoding.GetString(data);
        }
    }
}
