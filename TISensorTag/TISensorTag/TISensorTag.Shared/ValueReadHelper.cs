using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage.Streams;

namespace TISensorTag
{
    internal class ValueReadHelper
    {
        public static string ReadString(IBuffer buffer)
        {
            var reader = DataReader.FromBuffer(buffer);

            byte[] bytes = new byte[buffer.Length];

            reader.ReadBytes(bytes);

            return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
    }
}
