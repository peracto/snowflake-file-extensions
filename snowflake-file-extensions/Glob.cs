using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Snowflake.FileStream
{
    public static class Glob
    {
        public static IEnumerable<string> ExpandFileNames(string location)
        {
            var normalisePath = NormalisePath(location);
            
            if (!IsGlob(location))
                return GetSingleFolder(normalisePath);
            
            var directoryName = Path.GetDirectoryName(normalisePath);

            return GetFiles(
                directoryName != null && IsGlob(directoryName)
                    ? GetDeepFolder(directoryName)
                    : GetSingleFolder(directoryName),
                Path.GetFileName(normalisePath)
            );
        }

        private static readonly string pathSeperator = $"{Path.DirectorySeparatorChar}"; 
        
        private static IEnumerable<string> GetDeepFolder(string directoryName)
        {
            var pathParts = directoryName.Split(Path.DirectorySeparatorChar);
            var filePaths = GetSingleFolder("");

            for (var i = 0; i < pathParts.Length; i++)
            {
                // Build non-global path components first
                var path = pathSeperator;
                for (; i < pathParts.Length && !IsGlob(pathParts[i]); i++)
                    path = Path.Join(path, pathParts[i]);

                filePaths = i < pathParts.Length && IsGlob(pathParts[i])
                    ? ExpandFolderPath__Glob(filePaths, pathParts[i], path)
                    : ExpandFolderPath__NonGlob(filePaths, path);
            }

            return filePaths;
        }

        private static IEnumerable<string> ExpandFolderPath__Glob(IEnumerable<string> filePaths, string part, string path)
        {
            IEnumerable<string> union = null;
            foreach (var filePath in filePaths)
            {
                var p = Path.Join(filePath, path); 
                if (!Directory.Exists(p)) continue;
                union = union.InternalConcat(
                    Directory.EnumerateDirectories(p, part, EnumerationOptions)
                );
            }

            return union ?? GetSingleFolder();
        }

        private static IEnumerable<string> ExpandFolderPath__NonGlob(IEnumerable<string> filePaths, string part)
            => filePaths.Select(filePath => Path.Join(filePath, part)).Where(Directory.Exists);

        private static IEnumerable<string> GetFiles(IEnumerable<string> folders, string fileName)
            => IsGlob(fileName)
                ? GetFiles__Glob(folders, fileName)
                : GetFiles__NonGlob(folders, fileName);

        private static IEnumerable<string> GetFiles__NonGlob(IEnumerable<string> folders, string fileName)
            => folders.Select(p => Path.Join(p, fileName));


        private static IEnumerable<string> GetFiles__Glob(IEnumerable<string> folders, string fileName)
        {
            var ext = Path.GetExtension(fileName);
            var fixup = ext.Length == 4 && fileName.Contains("*");

            foreach (var folder in folders)
            {
                foreach (var file in Directory.EnumerateFiles(folder, fileName, EnumerationOptions))
                {
                    if (!fixup || file.EndsWith(ext))
                        yield return file;
                }
            }
        }

        private static IEnumerable<string> GetSingleFolder(string value = null)
        {
            if (value != null) yield return value;
        }


        static string NormalisePath(string path)
        {
            var p = path.Trim();
            return Path.GetFullPath(
                p.StartsWith('~')
                    ? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), p.Substring(1))
                    : p
            );
        }

        private static bool IsGlob(string path) 
            => path.Contains('*') || path.Contains('?');


        private static EnumerationOptions EnumerationOptions = new EnumerationOptions()
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = false,
            MatchCasing = MatchCasing.PlatformDefault,
            ReturnSpecialDirectories = false,
        };
    }
    
    internal static class LinqHelpers
    {
        public static IEnumerable<T> InternalConcat<T>(this IEnumerable<T> seq1, IEnumerable<T> seq2)
            => seq1 == null ? seq2 : seq2 == null ? seq1 : seq1.Concat(seq2);
    }
}