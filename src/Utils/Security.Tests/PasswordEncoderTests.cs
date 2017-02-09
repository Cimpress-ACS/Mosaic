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


using NUnit.Framework;
using System;

namespace VP.FF.PT.Common.Utils.Security.UnitTests
{
    [TestFixture]
    public class PasswordEncoderTests
    {
        [Test]
        public void EncodePassword_DecodeIt_VerifyTheyMatch()
        {
            string password = "sjdh aifa;lkjhf aifha fljkahfladjhf lad faldkjf";

            Assert.AreEqual(password, password.EncodePassword().DecodePassword());
        }

        [Test]
        public void EncodePasswordBase64_DecodeIt_VerifyTheyMatch()
        {
            string password = "asjdh asjh asd a7sd ajksdb a8sd7a870123874 afh ajkf 98f90";

            Assert.AreEqual(password, password.EncodeBase64().DecodeBase64());
        }

        [Test]
        public void EncryptPassword_DecryptIt_VerifyTheyMatch()
        {
            string password = "4236 283764 287364 28hgsdjahsgd ajhsdg ajhsdg jahgd jahg sdja";

            Assert.AreEqual(password, password.EncryptPassword().DecryptPassword());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EncodeNullPasswordWillGetException()
        {
            ((string)null).EncodePassword();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DecodeNullPasswordWillGetException()
        {
            ((string)null).DecodePassword();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EncryptNullPasswordWillGetException()
        {
            ((string)null).EncryptPassword();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DecryptNullPasswordWillGetException()
        {
            ((string)null).DecryptPassword();
        }

        [Test]
        public void EnsureThatPasswordCanBeGeneratedWithOnlineToolForCurrentMosaic()
        {
            // I am using this site to double encoding the Mosaic password() UTF-8 encoding
            // https://www.base64encode.org/
            // getting:
            string encodedMosaicPassword = "VW1sS05tTTFjMnRBS21GUQ==";
            string decodedMosaicPassword = "RiJ6c5sk@*aP";

            Assert.AreEqual(decodedMosaicPassword, encodedMosaicPassword.DecodePassword());
        }

        [Test]
        public void EnsureThatPasswordCanBeGeneratedWithOnlineToolForCurrentUnicorn()
        {
            // I am using this site to double encoding the unicorn password() UTF-8 encoding
            // https://www.base64encode.org/
            // getting:
            string encodedUnicornPassword = "ZFc1cFkyOXliZz09";
            string decodedUnicornPassword = "unicorn";

            Assert.AreEqual(decodedUnicornPassword, encodedUnicornPassword.DecodePassword());
        }

        [Test]
        public void QuickTestSimpleBase64Decoding()
        {
            string encodedMosaicPassword = "VW1sS05tTTFjMnRBS21GUQ==";

            string expectedOneStepEncoding = "UmlKNmM1c2tAKmFQ";

            string oneStepEncoding = encodedMosaicPassword.DecodeBase64();

            Assert.AreEqual(expectedOneStepEncoding, oneStepEncoding);
        }
    }
}
