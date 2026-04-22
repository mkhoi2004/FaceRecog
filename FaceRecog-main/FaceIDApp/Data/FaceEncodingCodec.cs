using System;
using System.Globalization;
using System.Linq;
using FaceRecog;

namespace FaceIDApp.Data
{
    /// <summary>
    /// Serialize/Deserialize FaceEncoding (double[128]) ↔ semicolon-separated TEXT
    /// Format: "0.123;-0.456;0.789;..." (128 values)
    /// </summary>
    internal static class FaceEncodingCodec
    {
        public static string Serialize(FaceEncoding encoding)
        {
            if (encoding == null)
                return null;

            var values = encoding.GetRawEncoding();
            return string.Join(";", values.Select(v => v.ToString("R", CultureInfo.InvariantCulture)));
        }

        public static FaceEncoding Deserialize(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return null;

            var values = payload.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(v => double.Parse(v, CultureInfo.InvariantCulture))
                                .ToArray();

            return FaceRecognition.LoadFaceEncoding(values);
        }
    }
}
