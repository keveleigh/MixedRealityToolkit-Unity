using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
    public static class AssetHelper
    {
        // NOTE: local path expression is dependent on the folder name of the cloned repository - if different from the default repo name, regex will fail.
        private static readonly Regex _assetPathExpression = new Regex(@".*/(Assets/.*/)[^/]+", RegexOptions.Compiled);
        private static readonly Regex _packagesPathExpression = new Regex(@".*/(Packages/.*/)[^/]+", RegexOptions.Compiled);
        private static readonly Regex _packageCachePathExpression = new Regex(@".*/PackageCache(/[^/]+)@[^/]+(/.*/)[^/]+", RegexOptions.Compiled);
        private static readonly Regex _localPathExpression = new Regex(@".*/lib.editorutils/(.*/)[^/]+", RegexOptions.Compiled);
        private const string _packageFormat = "Packages{0}{1}";
        private const string _localFormat = "Packages/com.microsoft.mrtk.editortools/{0}";
        private const string _unknownPath = "UNKNOWN";

        public static string GetScriptPath([CallerFilePath] string filename = "")
        {
            return GetAssetPath(filename);
        }

        public static string GetAssetPath(string fullPath)
        {
            var normalized = fullPath.Replace('\\', '/');

            var assetMatch = _assetPathExpression.Match(normalized);
            if (assetMatch.Success)
            {
                if (normalized.StartsWith(Application.dataPath))
                {
                    return assetMatch.Groups[1].Value;
                }
                else
                {
                    var reverseNormalizedAssetPath = Application.dataPath.Replace('/', '\\');
                    string relativePath = Path.GetRelativePath(reverseNormalizedAssetPath, fullPath);
                    relativePath = relativePath.Replace('\\', '/');
                    int idx = relativePath.LastIndexOf('/');
                    relativePath = relativePath.Substring(0, idx + 1);
                    idx = relativePath.IndexOf("Packages");
                    relativePath = relativePath.Substring(idx);
                    relativePath = relativePath.Replace("EditorTools", "com.microsoft.mrtk.editortools");
                    return relativePath;
                }
            }

            var packagesMatch = _packagesPathExpression.Match(normalized);
            if (packagesMatch.Success)
            {
                string relativePath = packagesMatch.Groups[1].Value;
				// TODO: find a better way to update runtime package names without hardcoding
                relativePath = relativePath.Replace("EditorTools.package", "com.microsoft.mrtk.editortools");
                return relativePath;
            }

            var packageCacheMatch = _packageCachePathExpression.Match(normalized);
            if (packageCacheMatch.Success)
                return string.Format(_packageFormat, packageCacheMatch.Groups[1].Value, packageCacheMatch.Groups[2].Value);

            var localMatch = _localPathExpression.Match(normalized);
            if (localMatch.Success)
                return string.Format(_localFormat, localMatch.Groups[1].Value);

            return _unknownPath;
        }
    }
}
