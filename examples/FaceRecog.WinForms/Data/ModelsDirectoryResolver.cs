using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace FaceRecog.WinForms.Data
{
    internal static class ModelsDirectoryResolver
    {
        public static string Resolve()
        {
            var configuredPath = ReadSetting("ModelsDirectory");
            var candidates = new List<string>();
            var searchRoots = new List<string>();

            var fixedCandidates = new List<string>
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models"),
                Path.Combine(Application.StartupPath, "models")
            };

            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrWhiteSpace(assemblyLocation))
                fixedCandidates.Add(Path.Combine(assemblyLocation, "models"));

            try
            {
                var processModuleLocation = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
                if (!string.IsNullOrWhiteSpace(processModuleLocation))
                    fixedCandidates.Add(Path.Combine(processModuleLocation, "models"));
            }
            catch
            {
            }

            foreach (var fixedCandidate in fixedCandidates.Where(path => !string.IsNullOrWhiteSpace(path)).Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (HasRequiredModels(fixedCandidate))
                    return fixedCandidate;
            }

            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                candidates.Add(configuredPath);
                candidates.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath));
                candidates.Add(Path.Combine(Environment.CurrentDirectory, configuredPath));
            }

            searchRoots.Add(AppDomain.CurrentDomain.BaseDirectory);
            searchRoots.Add(Environment.CurrentDirectory);
            searchRoots.Add(Application.StartupPath);

            if (!string.IsNullOrWhiteSpace(assemblyLocation))
                searchRoots.Add(assemblyLocation);

            try
            {
                var processModuleLocation = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
                if (!string.IsNullOrWhiteSpace(processModuleLocation))
                    searchRoots.Add(processModuleLocation);
            }
            catch
            {
            }

            foreach (var root in EnumerateAncestorDirectories(AppDomain.CurrentDomain.BaseDirectory))
                searchRoots.Add(root);

            foreach (var root in EnumerateAncestorDirectories(Environment.CurrentDirectory))
                searchRoots.Add(root);

            foreach (var candidate in candidates.Where(path => !string.IsNullOrWhiteSpace(path)).Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (HasRequiredModels(candidate))
                    return candidate;
            }

            foreach (var root in searchRoots.Where(path => !string.IsNullOrWhiteSpace(path)).Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var recursiveMatch = FindModelsDirectory(root);
                if (recursiveMatch != null)
                    return recursiveMatch;
            }

            var diagnosticLines = new List<string>
            {
                $"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}",
                $"StartupPath: {Application.StartupPath}",
                $"CurrentDirectory: {Environment.CurrentDirectory}",
                $"AssemblyLocation: {assemblyLocation ?? "<null>"}"
            };

            diagnosticLines.Add("Fixed candidates:");
            diagnosticLines.AddRange(fixedCandidates.Select(path => $"- {path} => {DescribeModelDirectory(path)}"));

            diagnosticLines.Add("Configured/derived candidates:");
            diagnosticLines.AddRange(candidates.Select(path => $"- {Path.GetFullPath(path)} => {DescribeModelDirectory(Path.GetFullPath(path))}"));

            diagnosticLines.Add("Search roots:");
            diagnosticLines.AddRange(searchRoots.Select(root => $"- {Path.GetFullPath(root)} => {DescribeModelDirectory(Path.Combine(Path.GetFullPath(root), "models"))}"));

            throw new DirectoryNotFoundException(
                "Không tìm thấy thư mục model. Hãy tải file model từ repo FaceRecog/dlib và đặt vào thư mục models.\n" +
                "Cần tối thiểu 2 file: shape_predictor_5_face_landmarks.dat và dlib_face_recognition_resnet_model_v1.dat.\n" +
                "Các đường dẫn đã thử:\n" + string.Join("\n", diagnosticLines) +
                "\n\nNếu muốn, bạn có thể đặt key appSettings 'ModelsDirectory' trong App.config để trỏ thẳng tới thư mục model.");
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

            var modelFiles = Directory.GetFiles(root, "shape_predictor_5_face_landmarks.dat", SearchOption.AllDirectories);
            foreach (var modelFile in modelFiles)
            {
                var directory = Path.GetDirectoryName(modelFile);
                if (HasRequiredModels(directory))
                    return directory;
            }

            return null;
        }

        private static string DescribeModelDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                return "<empty>";

            if (!Directory.Exists(directory))
                return "missing";

            var landmarkFile = Path.Combine(directory, "shape_predictor_5_face_landmarks.dat");
            var recognitionFile = Path.Combine(directory, "dlib_face_recognition_resnet_model_v1.dat");

            return $"exists, landmarks={File.Exists(landmarkFile)}, recognition={File.Exists(recognitionFile)}";
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