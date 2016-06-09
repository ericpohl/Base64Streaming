using System.IO;
using System.Text;

namespace Base64Streaming.Test
{
    public static class Extensions
    {
        public static string AsString(this Stream s, Encoding encoding)
        {
            var reader = new StreamReader(s, encoding);
            return reader.ReadToEnd();
        }

    }
}
