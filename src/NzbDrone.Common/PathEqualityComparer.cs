using System;
using System.Collections.Generic;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common
{
    public class PathEqualityComparer : IEqualityComparer<string>
    {
        public static readonly PathEqualityComparer Instance = new PathEqualityComparer();

        private PathEqualityComparer()
        {
        }

        public bool Equals(string x, string y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return NormalizeForComparison(x).Equals(NormalizeForComparison(y), DiskProviderBase.PathStringComparison);
        }

        public int GetHashCode(string obj)
        {
            var normalized = NormalizeForComparison(obj);

            if (OsInfo.IsWindows)
            {
                normalized = normalized.ToLower();
            }

            return normalized.GetHashCode();
        }

        private static string NormalizeForComparison(string path)
        {
            if (path.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            path = path.Normalize();

            string normalized;

            try
            {
                normalized = path.CleanFilePath();
            }
            catch (ArgumentException)
            {
                // CleanFilePath rejects paths with whitespace-padded components for Windows
                // portability, but such paths are legal on *nix and may be stored in the
                // database. Fall back to a non-validating cleanup so one bad row cannot
                // abort an entire scan.
                normalized = path.TrimEnd(' ').CleanFilePathBasic();
            }

            return normalized.Normalize();
        }
    }
}
