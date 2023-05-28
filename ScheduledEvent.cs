using MelonLoader.TinyJSON;

namespace Moment
{
    public class ScheduledEvent
	{
        public ScheduledEvent() {}

        public ScheduledEvent(TLDDateTime time, string executorId, string eventType, string? eventId = null, string? eventData = null)
        {
            Time = time;
            ExecutorId = executorId;
            EventId = eventId;
            EventData = eventData;
            EventType = eventType;
        }

        [MelonLoader.TinyJSON.Include]
        public TLDDateTime Time { get; set; }
        [MelonLoader.TinyJSON.Include]
		public string ExecutorId { get; set; }
        [MelonLoader.TinyJSON.Include]
		public string EventType { get; set; }
        /// <summary>
        /// The event id. If you have multiple instances of a type of event, give them same EventType and different EventId.
        /// </summary>
        [MelonLoader.TinyJSON.Include]
		public string? EventId { get; set; }
        [MelonLoader.TinyJSON.Include]
		public string? EventData { get; set; }
        public string EventUID => $"{ExecutorId}-{EventType}-{EventId}";
        [MelonLoader.TinyJSON.Include]
        public bool Cancelled { get; set;}
        /// <summary>
        /// Event with higher priority is executed before ones with lower priority. For compatibility. Set this only when you have discussed with the author of the mod when the order of your events matters.
        /// </summary>
        [MelonLoader.TinyJSON.Include]
        public int Priority { get; set; }
	}

    public record EventRequest
    {
        public EventRequest(TLDDateTime time, string eventType, string? eventId = null, string? eventData = null)
        {
            EventType = eventType;
            EventId = eventId;
            EventData = eventData;
            Time = time;
        }

        public TLDDateTime Time { get; internal set; }
        /// <summary>
        /// The event type. If you have multiple instances of a type of event, give them same EventType and different EventId.
        /// </summary>
        public string EventType { get; init; }
        /// <summary>
        /// Set this when you have to schedule multiple event of same types. Scheduled event will be replaced when the (ScheduledEventExecutorId, EventType, EventId) combination duplicates existing events.
        /// </summary>
		public string? EventId { get; init; }
        /// <summary>
        /// Use this to pass data for the execution.
        /// </summary>
		public string? EventData { get; init; }
        /// <summary>
        /// Event with higher priority is executed before ones with lower priority. For compatibility. Set this only when you have discussed with the author of the mod when the order of your events matters.
        /// </summary>
        public int Priority { get; set; }
    }
}
