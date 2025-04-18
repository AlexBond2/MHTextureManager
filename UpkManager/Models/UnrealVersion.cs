﻿using System;

namespace UpkManager.Models
{
    public sealed class UnrealVersion : IComparable<UnrealVersion>
    {

        #region Constructors

        public UnrealVersion() { }

        public UnrealVersion(string version)
        {
            string[] parts = version.Split('.');

            int value;

            if (parts.Length > 0 && Int32.TryParse(parts[0], out value)) Major = value;
            if (parts.Length > 1 && Int32.TryParse(parts[1], out value)) Minor = value;
            if (parts.Length > 2 && Int32.TryParse(parts[2], out value)) Release = value;
            if (parts.Length > 3 && Int32.TryParse(parts[3], out value)) Build = value;
        }

        #endregion Constructors

        #region Properties

        public int Major { get; }
        public int Minor { get; }
        public int Release { get; }
        public int Build { get; }

        #endregion Properties

        #region Unreal Properties

        public string Version => $"{Major}.{Minor}.{Release}.{Build}";

        #endregion Unreal Properties

        #region IComparable Implementation

        public int CompareTo(UnrealVersion other)
        {
            if ((Object)other == null) return 1;

            return this < other ? -1 : this > other ? 1 : 0;
        }

        #endregion IComparable Implementation

        #region Overrides

        public override bool Equals(Object obj)
        {
            UnrealVersion dv = obj as UnrealVersion;

            if ((Object)dv == null) return false;

            return this == dv;
        }

        public override int GetHashCode()
        {
            return Version.GetHashCode();
        }

        public override string ToString()
        {
            return Version;
        }

        #endregion Overrides

        #region Operators

        public static bool operator ==(UnrealVersion x, UnrealVersion y)
        {
            if (ReferenceEquals(x, y)) return true;

            if ((Object)x == null || (Object)y == null) return false;

            return x.Major == y.Major && x.Minor == y.Minor && x.Release == y.Release && x.Build == y.Build;
        }

        public static bool operator !=(UnrealVersion x, UnrealVersion y)
        {
            if (ReferenceEquals(x, y)) return false;

            if ((Object)x == null || (Object)y == null) return true;

            return x.Major != y.Major || x.Minor != y.Minor || x.Release != y.Release || x.Build != y.Build;
        }

        public static bool operator <(UnrealVersion x, UnrealVersion y)
        {
            if ((Object)x == null || (Object)y == null) return false;

            return (x.Major < y.Major)
                || (x.Major == y.Major && x.Minor < y.Minor)
                || (x.Major == y.Major && x.Minor == y.Minor && x.Release < y.Release)
                || (x.Major == y.Major && x.Minor == y.Minor && x.Release == y.Release && x.Build < y.Build);
        }

        public static bool operator >(UnrealVersion x, UnrealVersion y)
        {
            if ((Object)x == null || (Object)y == null) return false;

            return (x.Major > y.Major)
                || (x.Major == y.Major && x.Minor > y.Minor)
                || (x.Major == y.Major && x.Minor == y.Minor && x.Release > y.Release)
                || (x.Major == y.Major && x.Minor == y.Minor && x.Release == y.Release && x.Build > y.Build);
        }

        #endregion Operators

    }

}
