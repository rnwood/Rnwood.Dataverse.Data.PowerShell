using System;

namespace Fake4Dataverse
{
    /// <summary>
    /// Pluggable clock abstraction for deterministic time in tests.
    /// </summary>
    public interface IClock
    {
        /// <summary>Returns the current UTC time.</summary>
        DateTime UtcNow { get; }
    }

    /// <summary>
    /// Default clock that returns real system time.
    /// </summary>
    public sealed class SystemClock : IClock
    {
        /// <summary>Singleton instance.</summary>
        public static readonly SystemClock Instance = new SystemClock();

        /// <inheritdoc />
        public DateTime UtcNow => DateTime.UtcNow;
    }

    /// <summary>
    /// A manually-controllable clock for testing date/time-sensitive operations.
    /// </summary>
    public sealed class FakeClock : IClock
    {
        private DateTime _utcNow;

        /// <summary>Creates a fake clock set to the specified UTC time.</summary>
        public FakeClock(DateTime utcNow) { _utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc); }

        /// <summary>Creates a fake clock set to 2026-01-01T00:00:00Z.</summary>
        public FakeClock() : this(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)) { }

        /// <inheritdoc />
        public DateTime UtcNow => _utcNow;

        /// <summary>Advances the clock by the specified duration.</summary>
        public void Advance(TimeSpan duration) { _utcNow = _utcNow.Add(duration); }

        /// <summary>Sets the clock to a specific UTC time.</summary>
        public void Set(DateTime utcNow) { _utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc); }
    }
}
