namespace Moment
{
    public interface IScheduledEventExecutor
	{
        /// <summary>
        /// Suggestion: use a name that is unlikely to conflict with other mods, this is usually the name of your mod.
        /// </summary>
		string ScheduledEventExecutorId { get; }
        /// <summary>
        /// This will get called when it's time to execute the scheduled event.
        /// </summary>
		void Execute (TLDDateTime time, string eventType, string? eventId, string? eventData);
	}
}
