using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FaceRecog;

namespace FaceIDApp.Data
{
    internal sealed class FaceRecognitionService
    {
        /// <summary>
        /// Nhận diện khuôn mặt trong ảnh và so khớp với danh sách face_data từ DB.
        /// </summary>
        public List<FaceMatchItem> RecognizeFaces(
            string imagePath,
            string modelsDirectory,
            Model model,
            IReadOnlyList<FaceDataDto> faceDataList,
            double tolerance = 0.6d)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                throw new ArgumentException("Image path is required.", nameof(imagePath));

            var enrolledFaces = (faceDataList ?? new List<FaceDataDto>())
                .Where(fd => !string.IsNullOrWhiteSpace(fd.Encoding) && fd.IsActive)
                .Select(fd => new EnrolledFace
                {
                    FaceData = fd,
                    Encoding = FaceEncodingCodec.Deserialize(fd.Encoding)
                })
                .Where(item => item.Encoding != null)
                .ToArray();

            var resolvedModelsDirectory = ResolveModelsDirectory(modelsDirectory);

            using (var faceRecognition = FaceRecognition.Create(resolvedModelsDirectory))
            using (var image = FaceRecognition.LoadImageFile(imagePath))
            {
                var faceLocations = LocateFaces(faceRecognition, image, model);
                var faceEncodings = faceRecognition.FaceEncodings(image, faceLocations, 1, PredictorModel.Small, model).ToArray();
                var results = new List<FaceMatchItem>();

                for (var index = 0; index < faceEncodings.Length; index++)
                {
                    var encoding = faceEncodings[index];
                    var location = faceLocations[index];
                    var best = FindBestMatch(enrolledFaces, encoding, tolerance);

                    results.Add(new FaceMatchItem
                    {
                        FaceIndex = index + 1,
                        Box = $"{location.Top},{location.Right},{location.Bottom},{location.Left}",
                        EmployeeId = best?.FaceData.EmployeeId,
                        EmployeeName = best?.FaceData.EmployeeName,
                        EmployeeCode = best?.FaceData.EmployeeCode,
                        MatchedFaceDataId = best?.FaceData.Id,
                        Status = best == null ? "Unknown" : "Matched",
                        Distance = best?.Distance ?? double.MaxValue,
                        Confidence = best != null ? Math.Max(0, 1.0 - best.Distance) : 0,
                        EncodingData = FaceEncodingCodec.Serialize(encoding)
                    });
                }

                return results;
            }
        }

        /// <summary>
        /// Trích xuất face encoding từ 1 ảnh (dùng khi đăng ký khuôn mặt).
        /// Trả về null nếu ảnh không có đúng 1 khuôn mặt.
        /// </summary>
        public string BuildEncodingData(string imagePath, string modelsDirectory, Model model)
        {
            var result = ExtractFirstEncoding(imagePath, ResolveModelsDirectory(modelsDirectory), model);
            return result == null ? null : FaceEncodingCodec.Serialize(result);
        }

        private static FaceEncoding ExtractFirstEncoding(string imagePath, string modelsDirectory, Model model)
        {
            using (var faceRecognition = FaceRecognition.Create(modelsDirectory))
            using (var image = FaceRecognition.LoadImageFile(imagePath))
            {
                var faceLocations = LocateFaces(faceRecognition, image, model);
                if (faceLocations.Length != 1)
                    return null;

                var faceEncodings = faceRecognition.FaceEncodings(image, faceLocations, 1, PredictorModel.Small, model).ToArray();
                return faceEncodings.Length == 1 ? faceEncodings[0] : null;
            }
        }

        private static Location[] LocateFaces(FaceRecognition faceRecognition, Image image, Model model)
        {
            var faceLocations = faceRecognition.FaceLocations(image, 0, model).ToArray();
            if (faceLocations.Length == 0)
                faceLocations = faceRecognition.FaceLocations(image, 1, model).ToArray();

            if (faceLocations.Length == 0)
                faceLocations = faceRecognition.FaceLocations(image, 2, model).ToArray();

            return faceLocations;
        }

        private static string ResolveModelsDirectory(string modelsDirectory)
        {
            if (!string.IsNullOrWhiteSpace(modelsDirectory) && Directory.Exists(modelsDirectory))
                return modelsDirectory;

            return ModelsDirectoryResolver.Resolve();
        }

        private static MatchResult FindBestMatch(
            IEnumerable<EnrolledFace> faces,
            FaceEncoding encodingToCheck,
            double tolerance)
        {
            MatchResult best = null;

            foreach (var item in faces)
            {
                var distance = FaceRecognition.FaceDistance(item.Encoding, encodingToCheck);
                if (distance > tolerance)
                    continue;

                if (best == null || distance < best.Distance)
                {
                    best = new MatchResult
                    {
                        FaceData = item.FaceData,
                        Distance = distance
                    };
                }
            }

            return best;
        }

        private sealed class EnrolledFace
        {
            public FaceDataDto FaceData { get; set; }
            public FaceEncoding Encoding { get; set; }
        }

        private sealed class MatchResult
        {
            public FaceDataDto FaceData { get; set; }
            public double Distance { get; set; }
        }
    }

    internal class FaceMatchItem
    {
        public int FaceIndex { get; set; }
        public string Box { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public int? MatchedFaceDataId { get; set; }
        public string Status { get; set; }
        public double Distance { get; set; }
        public double Confidence { get; set; }
        public string EncodingData { get; set; }
    }
}
