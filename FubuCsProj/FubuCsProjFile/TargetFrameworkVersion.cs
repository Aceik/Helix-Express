using System;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class TargetFrameworkVersion : IEquatable<TargetFrameworkVersion>, IComparable, IComparable<TargetFrameworkVersion>
	{
		private Version version;

		public TargetFrameworkVersion(string version)
		{
			if (string.IsNullOrWhiteSpace(version))
			{
				throw new ArgumentNullException("version");
			}
			this.version = new Version(version.TrimStart(new char[]
			{
				'v'
			}));
		}

		public override string ToString()
		{
			return string.Format("v{0}", this.version);
		}

		public bool Equals(TargetFrameworkVersion other)
		{
			return !object.ReferenceEquals(null, other) && (object.ReferenceEquals(this, other) || object.Equals(this.version, other.version));
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (!(obj.GetType() != base.GetType()) && this.Equals((TargetFrameworkVersion)obj)));
		}

		public override int GetHashCode()
		{
			if (!(this.version != null))
			{
				return 0;
			}
			return this.version.GetHashCode();
		}

		public int CompareTo(TargetFrameworkVersion other)
		{
			if (other == null)
			{
				return 1;
			}
			return this.version.CompareTo(other.version);
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			return this.CompareTo((TargetFrameworkVersion)obj);
		}

		public static bool operator ==(TargetFrameworkVersion left, TargetFrameworkVersion right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(TargetFrameworkVersion left, TargetFrameworkVersion right)
		{
			return !object.Equals(left, right);
		}

		public static bool operator <(TargetFrameworkVersion x, TargetFrameworkVersion y)
		{
			return (!(x == null) || !(y == null)) && (x == null || x.CompareTo(y) < 0);
		}

		public static bool operator >(TargetFrameworkVersion x, TargetFrameworkVersion y)
		{
			return (!(x == null) || !(y == null)) && !(x == null) && x.CompareTo(y) > 0;
		}

		public static implicit operator string(TargetFrameworkVersion value)
		{
			if (!(value == null))
			{
				return value.ToString();
			}
			return null;
		}

		public static implicit operator TargetFrameworkVersion(string value)
		{
			return new TargetFrameworkVersion(value);
		}
	}
}
