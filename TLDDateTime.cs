using System.Diagnostics.CodeAnalysis;

namespace Moment
{
    /// <summary>
    /// TLD ingame days, hours, minutes.
    /// Can be used as both absolute time and time span: new TLDDateTime(0, 0, 1440) is equal to new TLDDateTime(1, 0, 0);
    /// </summary>
    public struct TLDDateTime : IEquatable<TLDDateTime>, IComparable<TLDDateTime>
	{
        private int minute;
        private int hour;

        public TLDDateTime(int day, int hour, int minute)
        {
            Day = day;
            Hour = hour;
            Minute = minute;
        }

        [MelonLoader.TinyJSON.Include]
        public int Day { get; set; }
        [MelonLoader.TinyJSON.Include]
        public int Hour
        {
            get => hour;
            set
            {
                if (value > 23)
                {
                    var days = value / 24;
                    var safeHours = value - days * 24;
                    if (days != 0) Day += days;
                    hour = safeHours;
                }
                else if (value < 0)
                {
                    var days = value / 24 - 1;
                    var safeHours = value - days * 24;
                    if (days != 0) Day += days;
                    hour = safeHours;
                }
                else
                    hour = value;
            }
        }
        [MelonLoader.TinyJSON.Include]
        public int Minute
        {
            get => minute;
            set
            {
                if (value > 59)
                {
                    var hours = value / 60;
                    var safeMins = value - hours * 60;
                    if (hours != 0) Hour += hours;
                    minute = safeMins;
                }
                else if (value < 0)
                {
                    var hours = value / 60 - 1;
                    var safeMins = value - hours * 60;
                    if (hours != 0) Hour += hours;
                    minute = safeMins;
                }
                else
                    minute = value;
            }
        }

        public int TotalHours => Day * 24 + Hour;
        public int TotalMinutes => TotalHours * 60 + Minute;

        public override string ToString()
        {
            return $"Day{Day} {Hour:D2}:{Minute:D2}";
        }
        public string ToStringRelative()
        {
            return $"{Day:+0;-0}d{Hour:+00;-00}h{Minute:+00;-00}m";
        }


        public int CompareTo(TLDDateTime other)
        {
			var dDelta = (this.Day - other.Day) * 24 * 60;
			var hDelta = (this.Hour - other.Hour) * 60;
			var mDelta = (this.Minute - other.Minute);
			return dDelta + hDelta + mDelta;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is TLDDateTime t && this.Equals(t);
        }

        public bool Equals(TLDDateTime other)
        {
            return this.Day == other.Day && this.Hour == other.Hour && this.Minute == other.Minute;
        }

        public override int GetHashCode()
        {
            return this.Day * 37 + this.Hour * 23 + this.Minute * 11;
        }
        public static implicit operator (int, int, int)(TLDDateTime tldDateTime) => new (tldDateTime.Day, tldDateTime.Hour, tldDateTime.Minute);
        public static implicit operator TLDDateTime((int, int, int) vTuple) => new TLDDateTime(vTuple.Item1, vTuple.Item2, vTuple.Item3);
        public static bool operator ==(TLDDateTime left, TLDDateTime right) => left.Equals(right);
        public static bool operator !=(TLDDateTime left, TLDDateTime right) => !left.Equals(right);

        public static bool operator <(TLDDateTime left, TLDDateTime right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(TLDDateTime left, TLDDateTime right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(TLDDateTime left, TLDDateTime right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(TLDDateTime left, TLDDateTime right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static TLDDateTime operator +(TLDDateTime left, TLDDateTime right)
        {
            return new TLDDateTime(left.Day + right.Day, left.Hour + right.Hour, left.Minute + right.Minute);
        }
        public static TLDDateTime operator -(TLDDateTime left, TLDDateTime right)
        {
            return new TLDDateTime(left.Day - right.Day, left.Hour - right.Hour, left.Minute - right.Minute);
        }
    }
}
