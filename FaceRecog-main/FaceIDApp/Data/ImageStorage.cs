using System;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace FaceIDApp.Data
{
    internal static class ImageStorage
    {
        /// <summary>
        /// Get a safe image folder. If the base directory contains non-ASCII chars
        /// (e.g. Vietnamese), use a temp folder instead so dlib native can read files.
        /// </summary>
        private static string GetSafeImageFolder(string subFolder)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            if (baseDir.Any(c => c > 127))
            {
                // Use temp path for non-ASCII paths (dlib native limitation)
                var safePath = Path.Combine(Path.GetTempPath(), "FaceIDApp_data", subFolder);
                Directory.CreateDirectory(safePath);
                return safePath;
            }

            var folder = Path.Combine(baseDir, subFolder);
            Directory.CreateDirectory(folder);
            return folder;
        }

        public static string StoreLocalCopy(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Source path is required.", nameof(sourcePath));

            var fullSourcePath = Path.GetFullPath(sourcePath);
            if (!File.Exists(fullSourcePath))
                throw new FileNotFoundException("Image file not found.", fullSourcePath);

            var imageFolder = GetSafeImageFolder("img");

            var sourceFolder = Path.GetFullPath(imageFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (fullSourcePath.StartsWith(sourceFolder, StringComparison.OrdinalIgnoreCase))
                return fullSourcePath;

            var extension = Path.GetExtension(fullSourcePath);
            var fileName = string.Format(
                CultureInfo.InvariantCulture,
                "{0:yyyyMMdd_HHmmssfff}_{1}{2}",
                DateTime.UtcNow,
                Guid.NewGuid().ToString("N"),
                extension);

            var destinationPath = Path.Combine(imageFolder, fileName);
            File.Copy(fullSourcePath, destinationPath, false);
            return Path.GetFullPath(destinationPath);
        }

        public static string StoreBitmap(Bitmap bitmap, string prefix = "camera")
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            var imageFolder = GetSafeImageFolder("img");

            var fileName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}_{1:yyyyMMdd_HHmmssfff}_{2}.png",
                string.IsNullOrWhiteSpace(prefix) ? "camera" : prefix.Trim(),
                DateTime.UtcNow,
                Guid.NewGuid().ToString("N"));

            var destinationPath = Path.Combine(imageFolder, fileName);
            bitmap.Save(destinationPath, ImageFormat.Png);
            return Path.GetFullPath(destinationPath);
        }

        public static string StoreFaceImage(string sourcePath, int employeeId, int imageIndex)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Source path is required.", nameof(sourcePath));

            var fullSourcePath = Path.GetFullPath(sourcePath);
            if (!File.Exists(fullSourcePath))
                throw new FileNotFoundException("Image file not found.", fullSourcePath);

            var faceFolder = GetSafeImageFolder("face");

            var destinationPath = Path.Combine(faceFolder, $"emp_{employeeId}_slot_{imageIndex}.png");

            using (var image = System.Drawing.Image.FromFile(fullSourcePath))
            {
                image.Save(destinationPath, ImageFormat.Png);
            }

            return Path.GetFullPath(destinationPath);
        }
    }
}
