using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FaceRecog;

namespace FaceRecog.WinForms.Data
{
    internal sealed class FaceRecognitionService
    {
        public List<FaceMatchItem> RecognizeFaces(
            string imagePath,
            string modelsDirectory,
            Model model,
            IReadOnlyList<AppUserItem> users,
            double tolerance = 0.6d)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                throw new ArgumentException("Image path is required.", nameof(imagePath));
            if (string.IsNullOrWhiteSpace(modelsDirectory))
                throw new ArgumentException("Models directory is required.", nameof(modelsDirectory));

            var enrolledUsers = (users ?? Array.Empty<AppUserItem>())
                .Where(user => !string.IsNullOrWhiteSpace(user.FaceEncodingData))
                .Select(user => new EnrolledFace
                {
                    User = user,
                    Encoding = FaceEncodingCodec.Deserialize(user.FaceEncodingData)
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
                    var best = FindBestMatch(enrolledUsers, encoding, tolerance);

                    results.Add(new FaceMatchItem
                    {
                        FaceIndex = index + 1,
                        Box = $"{location.Top},{location.Right},{location.Bottom},{location.Left}",
                        UserId = best?.User.Id,
                        Username = best?.User.Username,
                        FullName = best?.User.FullName,
                        Status = best == null ? "Unknown" : "Matched",
                        Distance = best?.Distance ?? double.MaxValue,
                        EncodingData = FaceEncodingCodec.Serialize(encoding)
                    });
                }

                return results;
            }
        }

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

        private static FaceMatchResult FindBestMatch(
            IEnumerable<EnrolledFace> users,
            FaceEncoding encodingToCheck,
            double tolerance)
        {
            FaceMatchResult best = null;

            foreach (var item in users)
            {
                var distance = FaceRecognition.FaceDistance(item.Encoding, encodingToCheck);
                if (distance > tolerance)
                    continue;

                if (best == null || distance < best.Distance)
                {
                    best = new FaceMatchResult
                    {
                        User = item.User,
                        Distance = distance
                    };
                }
            }

            return best;
        }

        private sealed class EnrolledFace
        {
            public AppUserItem User { get; set; }

            public FaceEncoding Encoding { get; set; }
        }

        private sealed class FaceMatchResult
        {
            public AppUserItem User { get; set; }

            public double Distance { get; set; }
        }
    }
}
