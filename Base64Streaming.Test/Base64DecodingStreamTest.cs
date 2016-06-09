using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Base64Streaming.Test
{
    [TestFixture]
    public class Base64DecodingStreamTest
    {
        [Test]
        [TestCase("The quick brown fox jumped over the lazy dog.")]
        [TestCase("The quick brown fox jumped over the lazy dog")]
        [TestCase("Man is distinguished, not only by his reason, but by this singular passion from other animals, which is a lust of the mind, that by a perseverance of delight in the continued and indefatigable generation of knowledge, exceeds the short vehemence of any carnal pleasure.")]
        public void Test1(string expected)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(expected);
            string base64 = Convert.ToBase64String(bytes);
            
            Console.WriteLine("{0} => {1}", expected, base64);
            var textReader = new StringReader(base64);
            using (var stream = new Base64DecodingStream(textReader))
            {
                Assert.AreEqual(expected, stream.AsString(Encoding.UTF8));
            }
        }

        [Test]
        public void LongBase64String_ShortBufferSize()
        {
            string filename = @"C:\Users\epohl\Dropbox\projects\Base64Streaming\Base64Streaming.Test\base64.txt";
            using (var inputStream = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                var inputReader = new StreamReader(inputStream);
                using (var decodedStream = new Base64DecodingStream(inputReader))
                {
                    Console.WriteLine(decodedStream.AsString(Encoding.UTF8));
                }
            }
        }

        // Tests from RFC 4648 - see http://tools.ietf.org/html/rfc4648#page-12
        [Test]
        [TestCase("", "")]
        [TestCase("Zg==", "f")]
        [TestCase("Zm8=", "fo")]
        [TestCase("Zm9v", "foo")]
        [TestCase("Zm9vYg==", "foob")]
        [TestCase("Zm9vYmE=", "fooba")]
        [TestCase("Zm9vYmFy", "foobar")]
        public void Rfc4868(string input, string expectedOutput)
        {
            using (var inputReader = new StringReader(input))
            {
                using (var decodedStream = new Base64DecodingStream(inputReader))
                {
                    Assert.AreEqual(expectedOutput, decodedStream.AsString(Encoding.UTF8));
                }
            }
        }



        // TODO: Handle whitespace
    }
}
