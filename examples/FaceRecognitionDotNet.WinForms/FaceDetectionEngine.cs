using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FaceRecognitionDotNet;

namespace FaceRecognitionDotNet.WinForms
{
    internal sealed class FaceDetectionEngine
    {
        public List<string> AnalyzeDirectory(string folder, string modelsDirectory, Model model, int cpus)
        {
            using (var faceRecognition = FaceRecognition.Create(modelsDirectory))
            {
                var imageFiles = this.ImageFilesInFolder(folder).ToArray();
                var results = new string[imageFiles.Length];

                if (cpus == -1)
                    cpus = Environment.ProcessorCount;

                if (cpus <= 1)
                {
                    for (var i = 0; i < imageFiles.Length; i++)
                        results[i] = this.TestImage(faceRecognition, imageFiles[i], model);
                }
                else
                {
                    var option = new ParallelOptions { MaxDegreeOfParallelism = cpus };
                    Parallel.ForEach(Enumerable.Range(0, imageFiles.Length), option, i =>
                    {
                        results[i] = this.TestImage(faceRecognition, imageFiles[i], model);
                    });
                }

                return results.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            }
        }

        public List<string> AnalyzeImage(string imagePath, string modelsDirectory, Model model)
        {
            using (var faceRecognition = FaceRecognition.Create(modelsDirectory))
            {
                var result = this.TestImage(faceRecognition, imagePath, model);
                if (string.IsNullOrWhiteSpace(result))
                    return new List<string>();

                return result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        private IEnumerable<string> ImageFilesInFolder(string folder)
        {
            return Directory.GetFiles(folder)
                            .Where(s => Regex.IsMatch(Path.GetExtension(s), "\\.(jpg|jpeg|png)$", RegexOptions.IgnoreCase | RegexOptions.Compiled));
        }

        private string TestImage(FaceRecognition faceRecognition, string imageToCheck, Model model)
        {
            using (var unknownImage = FaceRecognition.LoadImageFile(imageToCheck))
            {
                var faceLocations = faceRecognition.FaceLocations(unknownImage, 0, model).ToArray();
                if (faceLocations.Length == 0)
                    return string.Empty;

                var builder = new StringBuilder();
                foreach (var faceLocation in faceLocations)
                    builder.AppendLine($"{imageToCheck},{faceLocation.Top},{faceLocation.Right},{faceLocation.Bottom},{faceLocation.Left}");

                return builder.ToString().TrimEnd();
            }
        }
    }
}