using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FaceIDApp.Data
{
    internal static class ModelsDirectoryResolver
    {
        /// <summary>
        /// Resolve the models directory. If the path contains non-ASCII characters
        /// (e.g. Vietnamese), copy models to a temp directory because dlib native
        /// cannot handle Unicode paths.
        /// </summary>
        public static string Resolve()
        {
            var found = FindModelsPath();
            if (found == null)
            {
                throw new DirectoryNotFoundException(
                    "Không tìm thấy thư mục model.\n" +
                    "Cần tối thiểu 2 file:\n" +
                    "- shape_predictor_5_face_landmarks.dat\n" +
                    "- dlib_face_recognition_resnet_model_v1.dat\n\n" +
                    "Đặt chúng vào thư mục 'models' cạnh file .exe hoặc cấu hình 'ModelsDirectory' trong App.config.");
            }

            // If path contains non-ASCII chars, dlib native can't read it.
            // Copy to a safe temp location.
            if (ContainsNonAscii(found))
            {
                return CopyToSafePath(found);
            }

            return found;
        }

        private static string FindModelsPath()
        {
            var configuredPath = ReadSetting("ModelsDirectory");
            var candidates = new List<string>();

            // Direct candidates
            var fixedCandidates = new List<string>
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models"),
                Path.Combine(Application.StartupPath, "models")
            };

            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrWhiteSpace(assemblyLocation))
                fixedCandidates.Add(Path.Combine(assemblyLocation, "models"));

            foreach (var c in fixedCandidates
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (HasRequiredModels(c))
                    return c;
            }

            // Configured path candidates
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                candidates.Add(configuredPath);
                candidates.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath));
                candidates.Add(Path.Combine(Environment.CurrentDirectory, configuredPath));
            }

            foreach (var candidate in candidates
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (HasRequiredModels(candidate))
                    return candidate;
            }

            // Search ancestor directories
            var searchRoots = new List<string>
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Environment.CurrentDirectory,
                Application.StartupPath
            };

            if (!string.IsNullOrWhiteSpace(assemblyLocation))
                searchRoots.Add(assemblyLocation);

            foreach (var root in EnumerateAncestorDirectories(AppDomain.CurrentDomain.BaseDirectory))
                searchRoots.Add(root);

            foreach (var root in searchRoots
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var match = FindModelsDirectory(root);
                if (match != null)
                    return match;
            }

            return null;
        }

        /// <summary>
        /// Copy model files to a safe ASCII-only temp path that dlib can read.
        /// </summary>
        private static string CopyToSafePath(string sourcePath)
        {
            var safePath = Path.Combine(Path.GetTempPath(), "FaceIDApp_models");

            if (!Directory.Exists(safePath))
                Directory.CreateDirectory(safePath);

            var modelFiles = new[]
            {
                "shape_predictor_5_face_landmarks.dat",
                "dlib_face_recognition_resnet_model_v1.dat"
            };

            foreach (var file in modelFiles)
            {
                var src = Path.Combine(sourcePath, file);
                var dst = Path.Combine(safePath, file);

                if (!File.Exists(dst) || new FileInfo(src).Length != new FileInfo(dst).Length)
                {
                    File.Copy(src, dst, true);
                }
            }

            return safePath;
        }

        private static bool ContainsNonAscii(string path)
        {
            return path.Any(c => c > 127);
        }

        private static string ReadSetting(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool HasRequiredModels(string directory)
        {
            if (!Directory.Exists(directory))
                return false;

            return File.Exists(Path.Combine(directory, "shape_predictor_5_face_landmarks.dat"))
                && File.Exists(Path.Combine(directory, "dlib_face_recognition_resnet_model_v1.dat"));
        }

        private static string FindModelsDirectory(string root)
        {
            if (!Directory.Exists(root))
                return null;

            try
            {
                var modelFiles = Directory.GetFiles(root, "shape_predictor_5_face_landmarks.dat", SearchOption.AllDirectories);
                foreach (var modelFile in modelFiles)
                {
                    var directory = Path.GetDirectoryName(modelFile);
                    if (HasRequiredModels(directory))
                        return directory;
                }
            }
            catch { }

            return null;
        }

        private static IEnumerable<string> EnumerateAncestorDirectories(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                yield break;

            var current = new DirectoryInfo(Path.GetFullPath(path));
            while (current != null)
            {
                yield return current.FullName;
                current = current.Parent;
            }
        }
    }
}
