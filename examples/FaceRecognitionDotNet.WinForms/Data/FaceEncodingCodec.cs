using System;
using System.Globalization;
using System.Linq;
using FaceRecognitionDotNet;

namespace FaceRecognitionDotNet.WinForms.Data
{
    internal static class FaceEncodingCodec
    {
        public static string Serialize(FaceEncoding encoding)
        {
            if (encoding == null)
                return null;

            var values = encoding.GetRawEncoding();
            return string.Join(";", values.Select(value => value.ToString("R", CultureInfo.InvariantCulture)));
        }

        public static FaceEncoding Deserialize(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return null;

            var values = payload.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(value => double.Parse(value, CultureInfo.InvariantCulture))
                                .ToArray();

            return FaceRecognition.LoadFaceEncoding(values);
        }
    }
}
