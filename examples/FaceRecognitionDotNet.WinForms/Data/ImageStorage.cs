using System;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace FaceRecognitionDotNet.WinForms.Data
{
    internal static class ImageStorage
    {
        public static string StoreLocalCopy(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Source path is required.", nameof(sourcePath));

            var fullSourcePath = Path.GetFullPath(sourcePath);
            if (!File.Exists(fullSourcePath))
                throw new FileNotFoundException("Image file not found.", fullSourcePath);

            var imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img");
            Directory.CreateDirectory(imageFolder);

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

            var imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img");
            Directory.CreateDirectory(imageFolder);

            var fileName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}_{1:yyyyMMdd_HHmmssfff}_{2}.png",
                string.IsNullOrWhiteSpace(prefix) ? "camera" : prefix.Trim(),
                DateTime.UtcNow,
                Guid.NewGuid().ToString("N"));

            var destinationPath = Path.Combine(imageFolder, fileName);
            bitmap.Save(destinationPath, System.Drawing.Imaging.ImageFormat.Png);
            return Path.GetFullPath(destinationPath);
        }

        public static string StoreFaceImage(string sourcePath, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Source path is required.", nameof(sourcePath));

            var fullSourcePath = Path.GetFullPath(sourcePath);
            if (!File.Exists(fullSourcePath))
                throw new FileNotFoundException("Image file not found.", fullSourcePath);

            var faceFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "face");
            Directory.CreateDirectory(faceFolder);

            var destinationPath = Path.Combine(faceFolder, userId.ToString("N") + ".png");

            if (File.Exists(destinationPath))
                return Path.GetFullPath(destinationPath);

            using (var image = System.Drawing.Image.FromFile(fullSourcePath))
            {
                image.Save(destinationPath, System.Drawing.Imaging.ImageFormat.Png);
            }

            return Path.GetFullPath(destinationPath);
        }
    }
}
