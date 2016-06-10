using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Base64Streaming.Test
{
    [TestFixture]
    public class Base64DecodingStreamTest
    {
        [Test]
        [TestCase("The quick brown fox jumped over the lazy dog.")]
        [TestCase("The quick brown fox jumped over the lazy dog")]
        [TestCase(
            "Man is distinguished, not only by his reason, but by this singular passion from other animals, which is a lust of the mind, that by a perseverance of delight in the continued and indefatigable generation of knowledge, exceeds the short vehemence of any carnal pleasure."
            )]
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

        [TestCase("Zg", "f")]
        [TestCase("Zm8", "fo")]
        [TestCase("Zm9vYg", "foob")]
        [TestCase("Zm9vYmE", "fooba")]
        [Test]
        public void MissingPadChars(string input, string expectedOutput)
        {
            using (var inputReader = new StringReader(input))
            {
                using (var decodedStream = new Base64DecodingStream(inputReader))
                {
                    Assert.AreEqual(expectedOutput, decodedStream.AsString(Encoding.UTF8));
                }
            }
        }

        [Test]
        public void OneExtraCharacter_ThrowsFormatException()
        {
            using (var inputReader = new StringReader("Zm9vY"))
            {
                using (var decodedStream = new Base64DecodingStream(inputReader))
                {
                    Assert.Throws<FormatException>(() => Console.WriteLine(decodedStream.AsString(Encoding.UTF8)));
                }
            }
        }

        [TestCase('A', 0)]
        [TestCase('Z', 25)]
        [TestCase('a', 26)]
        [TestCase('z', 51)]
        [TestCase('0', 52)]
        [TestCase('9', 61)]
        [TestCase('+', 62)]
        [TestCase('/', 63)]
        [TestCase('=', 64)]
        [Test]
        public void ValueFromChar(char c, int expected)
        {
            Assert.AreEqual(expected, Base64DecodingStream.ValueFromChar(c));
        }

        [TestCase('A', 0u)]
        [TestCase('Z', 25u)]
        [TestCase('a', 26u)]
        [TestCase('z', 51u)]
        [TestCase('0', 52u)]
        [TestCase('9', 61u)]
        [TestCase('+', 62u)]
        [TestCase('/', 63u)]
        [TestCase('=', 64u)]
        [Test]
        public void ValueFromChar2(char c, uint expected)
        {
            Assert.AreEqual(expected, Base64DecodingStream.ValueFromChar2((uint) c));
        }

        [Test]
        public void ReadCharacterQuartet_Empty()
        {
            char[] buffer = new char[4];
            using (var inputReader = new StringReader(""))
            {
                Assert.AreEqual(0, Base64DecodingStream.ReadCharacterQuartet(inputReader, buffer));
                for (int i = 0; i < 4; i++)
                {
                    Assert.AreEqual(0, buffer[i]);
                }
            }
        }

        [Test]
        public void ReadCharacterQuartet_Full()
        {
            char[] buffer = new char[4];
            using (var inputReader = new StringReader("fred"))
            {
                Assert.AreEqual(4, Base64DecodingStream.ReadCharacterQuartet(inputReader, buffer));
                CollectionAssert.AreEqual("fred", buffer);
            }
        }

        [Test]
        public void ReadCharacterQuartet_WhitespaceAtBeginning()
        {
            char[] buffer = new char[4];
            using (var inputReader = new StringReader(" fred"))
            {
                Assert.AreEqual(4, Base64DecodingStream.ReadCharacterQuartet(inputReader, buffer));
                CollectionAssert.AreEqual("fred", buffer);
            }
        }

        [Test]
        public void ReadCharacterQuartet_TwoWhitespaceAtBeginning()
        {
            char[] buffer = new char[4];
            using (var inputReader = new StringReader("  fred"))
            {
                Assert.AreEqual(4, Base64DecodingStream.ReadCharacterQuartet(inputReader, buffer));
                CollectionAssert.AreEqual("fred", buffer);
            }
        }

        [Test]
        public void ReadCharacterQuartet_FourWhitespaceAtBeginning()
        {
            char[] buffer = new char[4];
            using (var inputReader = new StringReader("    fred"))
            {
                Assert.AreEqual(4, Base64DecodingStream.ReadCharacterQuartet(inputReader, buffer));
                CollectionAssert.AreEqual("fred", buffer);
            }
        }

        [Test]
        public void ReadCharacterQuartet_TwoWhitespaceInMiddle()
        {
            char[] buffer = new char[4];
            using (var inputReader = new StringReader("fr  ed"))
            {
                Assert.AreEqual(4, Base64DecodingStream.ReadCharacterQuartet(inputReader, buffer));
                CollectionAssert.AreEqual("fred", buffer);
            }
        }

        [Test]
        public void ReadCharacterQuartet_InterspersedWhitespace()
        {
            char[] buffer = new char[4];
            using (var inputReader = new StringReader(" f r e d "))
            {
                Assert.AreEqual(4, Base64DecodingStream.ReadCharacterQuartet(inputReader, buffer));
                CollectionAssert.AreEqual("fred", buffer);
            }
        }

        [Test]
        public void ReadCharacterQuartet_IncompleteQuartet()
        {
            char[] buffer = new char[4];
            using (var inputReader = new StringReader("fr"))
            {
                Assert.AreEqual(2, Base64DecodingStream.ReadCharacterQuartet(inputReader, buffer));
                Assert.AreEqual('f', buffer[0]);
                Assert.AreEqual('r', buffer[1]);
            }
        }

        [Test]
        public void ReadCharacterQuartet_IncompleteQuartetAndWhitespaceAtEnd()
        {
            char[] buffer = new char[4];
            using (var inputReader = new StringReader("fr  "))
            {
                Assert.AreEqual(2, Base64DecodingStream.ReadCharacterQuartet(inputReader, buffer));
                Assert.AreEqual('f', buffer[0]);
                Assert.AreEqual('r', buffer[1]);
            }
        }
    }
}
