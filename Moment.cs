using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using ModData;

namespace Moment
{
    public class Moment : MelonMod
	{
		public static Moment Instance { get; private set;}
        public override void OnEarlyInitializeMelon()
        {
			Instance = this;
        }
        public override void OnInitializeMelon()
        {
			ModSave = new ModDataManager(nameof(Moment));
			uConsole.RegisterCommand("moments", new Action(() =>
			{
				MelonLogger.Msg(MelonLoader.TinyJSON.Encoder.Encode(scheduledEvents));
			}));
        }
		
		internal ModDataManager ModSave { get; private set; }
		internal bool UpdateNow ()
		{
			var now = new TLDDateTime(GameManager.m_TimeOfDay.GetDayNumber(), GameManager.m_TimeOfDay.GetHour(), GameManager.m_TimeOfDay.GetMinutes());
			bool changed = false;

			if (nowInternal.Day != now.Day)
			{
				OnDayChanged?.Invoke(now);
				changed = true;
			}
			if (nowInternal.Hour != now.Hour)
			{
				if (!GameManager.m_TimeOfDay.IsTimeLapseActive())
					OnHourChangedNonAcclerated?.Invoke(now);
				OnHourChanged?.Invoke(now);
				changed = true;
			}
			if (nowInternal.Minute != now.Minute)
			{
				if (!GameManager.m_TimeOfDay.IsTimeLapseActive())
					OnMinuteChangedNonAcclerated?.Invoke(now);
				OnMinuteChanged?.Invoke(now);
				changed = true;
			}
			nowInternal = now;
			return changed;
		}

		internal bool Executing { get; private set; }
		public override void  OnUpdate ()
		{
			if (GameManager.m_TimeOfDay == null || GameManager.m_IsPaused || !Check.InGame || scheduledEvents == null) return;

			if (!UpdateNow()) return;

			int consumed = 0;
			Executing = true;
			for (int i = 0; i < scheduledEvents.Count; i++)
			{
				ScheduledEvent ev = scheduledEvents[i];
				if (ev == null)
				{
					consumed++;
					continue;
				}
				try
				{
					if (ev.Time > Now) break;
					consumed++;

					if (ev.Cancelled)
					{
						eventCache.Remove(ev.EventUID);
						continue;
					}
					if (executors.ContainsKey(ev.ExecutorId))
					{
						Moment.Instance.LoggerInstance?.Msg($"Executing: {ev.EventUID} at {Now}");
						eventCache.Remove(ev.EventUID);
						SaveGlobalData.changedSinceLastSave = true;
						executors[ev.ExecutorId].Execute(Now, ev.EventType, ev.EventId, ev.EventData);
						Moment.Instance.LoggerInstance?.Msg($"Executed : {ev.EventUID} at {Now}");
					}
				}
				catch (Exception ex)
				{
					MelonLogger.Error($"An error happened when executing event: {ev.EventUID} ({ex})");
				}
			}
			if (consumed > 0) scheduledEvents.RemoveRange(0, consumed);
			Executing = false;
			while (pendingToSchedule.TryDequeue(out var pending))
				ScheduleInternal(pending.Item1, pending.Item2);
		}
		/// <summary>
		/// Current ingame time.
		/// </summary>
		public static TLDDateTime Now => Instance.NowInternal;
        private TLDDateTime nowInternal = (-1, -1, -1);
        public TLDDateTime NowInternal
		{
			get
			{
				if (nowInternal == (-1, -1, -1))
				{
					if (!Check.InGame) MelonLogger.Error("Some mod is accessing TOD time out of gameplay, invalid time will be returned.");
					UpdateNow();
				}
				return nowInternal;
			}
			private set => nowInternal = value;
		}
        public static TLDDateTime OneDay => new TLDDateTime(1, 0, 0);
		public static TLDDateTime OneHour => new TLDDateTime(0, 1, 0);
		public static TLDDateTime OneMinute => new TLDDateTime(0, 0, 1);
		/// <summary>
		/// Get called when TLD minute changed. Subscribe to the NonAcclerated one when you are sure waht you gonna do can be skipped when time is acclerated (sleeping, passing time, breaking down... etc.).
		/// </summary>
		public static event Action<TLDDateTime>? OnMinuteChanged;
		/// <summary>
		/// Get called when TLD minute changed. Subscribe to this when you are sure waht you gonna do can be skipped when time is acclerated (sleeping, passing time, breaking down... etc.).
		/// </summary>
		public static event Action<TLDDateTime>? OnMinuteChangedNonAcclerated;
		/// <summary>
		/// Get called when TLD hour changed. Subscribe to the NonAcclerated one when you are sure waht you gonna do can be skipped when time is acclerated (sleeping, passing time, breaking down... etc.).
		/// </summary>
		public static event Action<TLDDateTime>? OnHourChanged;
		/// <summary>
		/// Get called when TLD hour changed. Subscribe to this when you are sure waht you gonna do can be skipped when time is acclerated (sleeping, passing time, breaking down... etc.).
		/// </summary>
		public static event Action<TLDDateTime>? OnHourChangedNonAcclerated;
		/// <summary>
		/// Get called when TLD day changed.
		/// </summary>
		public static event Action<TLDDateTime>? OnDayChanged;
		internal List<ScheduledEvent> scheduledEvents;
		internal Queue<(IScheduledEventExecutor, EventRequest)> pendingToSchedule = new Queue<(IScheduledEventExecutor, EventRequest)>();
		internal Dictionary<string, IScheduledEventExecutor> executors = new Dictionary<string, IScheduledEventExecutor>();
		internal Dictionary<string, ScheduledEvent> eventCache = new Dictionary<string, ScheduledEvent>();


		/// <summary>
		/// <para>Register an executor, which will be notified when it's time to execute an scheduled event.</para>
		/// <para>Always call this on initialization if you are using the schedulers.</para>
		/// </summary>
        public static void RegisterExecutor (IScheduledEventExecutor executor) => Instance.RegisterExecutorInternal(executor);
		/// <summary>
		/// <para>Register an executor, which will be notified when it's time to execute an scheduled event.</para>
		/// <para>Always call this on initialization if you are using the schedulers.</para>
		/// </summary>
		public void RegisterExecutorInternal (IScheduledEventExecutor executor)
		{
			executors.TryAdd(executor.ScheduledEventExecutorId, executor);
		}

		/// <summary>
		/// Schedule the event. If the time specified is D1H1M0, it will be executed at Day 1 Hour 1.
		/// If an event of "ExecutorId-EventType-EventId" already exists, it will be replaced.
		/// </summary>
		public static void Schedule (IScheduledEventExecutor executor, EventRequest evq) => Instance.ScheduleInternal(executor, evq);
		/// <summary>
		/// Schedule the event. If the time specified is D1H1M0, it will be executed at Day 1 Hour 1.
		/// If an event of "ExecutorId-EventType-EventId" already exists, it will be replaced.
		/// </summary>
		public void ScheduleInternal (IScheduledEventExecutor executor, EventRequest evq)
		{
			if (executor?.ScheduledEventExecutorId == null)
			{
				Moment.Instance.LoggerInstance?.Error($"Executor {executor} is not identified, can't schedule event.");
				return;
			}
			if (Executing)
			{
				pendingToSchedule.Enqueue((executor, evq));
				return;
			}
			SaveGlobalData.changedSinceLastSave = true;
			CancelInternal(executor.ScheduledEventExecutorId, evq.EventType, evq.EventId);
			executors.TryAdd(executor.ScheduledEventExecutorId, executor);
			var ev = new ScheduledEvent(evq.Time, executor.ScheduledEventExecutorId, evq.EventType, evq.EventId, evq.EventData);
			ev.Priority = evq.Priority;
			int cursor = 0;
			if (scheduledEvents.Count == 0)
			{
				scheduledEvents.Add(ev);
				eventCache[ev.EventUID] = ev;
				// Moment.Instance.LoggerInstance?.Msg($"Scheduled: {ev.EventUID} at {ev.Time} ({(ev.Time - NowInternal).ToStringRelative()})");
			}
			else
			{
				while (cursor < scheduledEvents.Count)
				{
					if (ev.Time < scheduledEvents[cursor].Time  // earlier than [cursor]
					 || ev.Time == scheduledEvents[cursor].Time && ev.Priority > scheduledEvents[cursor].Priority)  // same minute and has higher priority
					{
						scheduledEvents.Insert(cursor, ev);
						eventCache[ev.EventUID] = ev;
						// Moment.Instance.LoggerInstance?.Msg($"Scheduled: {ev.EventUID} at {ev.Time} ({(ev.Time - NowInternal).ToStringRelative()})");
						break;
					}

					if (cursor == scheduledEvents.Count - 1) // tail, still not found
					{
						scheduledEvents.Add(ev);
						eventCache[ev.EventUID] = ev;
						// Moment.Instance.LoggerInstance?.Msg($"Scheduled: {ev.EventUID} at {ev.Time} ({(ev.Time - NowInternal).ToStringRelative()})");
						break;
					}
					cursor++;
				}
			}
		}

		/// <summary>
		/// Relatively schedule the event. If the time specified is D1H1M0, instead of executing it at Day 1 Hour 1, execute it at 1 day and 1 hour later.
		/// </summary>
		public static void ScheduleRelative (IScheduledEventExecutor executor, EventRequest evq) => Instance.ScheduleRelativeInternal(executor, evq);
		/// <summary>
		/// Relatively schedule the event. If the time specified is D1H1M0, instead of executing it at Day 1 Hour 1, execute it at 1 day and 1 hour later.
		/// </summary>
		public void ScheduleRelativeInternal (IScheduledEventExecutor executor, EventRequest evq)
		{
			evq.Time += NowInternal;
			Schedule(executor, evq);
		}

		/// <summary>
		/// Cancel a scheduled event. Can be safely called without checking IsScheduled().
		/// </summary>
		public static void Cancel (string executorId, string eventType, string? eventId = null) => Instance.CancelInternal(executorId, eventType, eventId);
		/// <summary>
		/// Cancel a scheduled event. Can be safely called without checking IsScheduled().
		/// </summary>
		public void CancelInternal (string executorId, string eventType, string? eventId = null)
		{
            string key = $"{executorId}-{eventType}-{eventId}";
            if (!eventCache.TryGetValue(key, out var ev)) return;
			SaveGlobalData.changedSinceLastSave = true;
			ev.Cancelled = true;
			eventCache.Remove(key);
			Moment.Instance.LoggerInstance?.Msg($"Cancelled: {ev.EventUID} at {ev.Time}");
			if (Executing) return;
			scheduledEvents.Remove(ev);
		}

		/// <summary>
		/// Check if an event is scheduled.
		/// </summary>
		public static bool IsScheduled (string executorId, string eventType, string? eventId = null) => Instance.IsScheduledInternal(executorId, eventType, eventId);
		/// <summary>
		/// Check if an event is scheduled.
		/// </summary>
		public bool IsScheduledInternal (string executorId, string eventType, string? eventId = null)
		{
            string key = $"{executorId}-{eventType}-{eventId}";
            return eventCache.ContainsKey(key);
		}
		
		/// <summary>
		/// Get when is a event is scheeduled to happen. Can return null if there's no such event scheduled.
		/// </summary>
		public static TLDDateTime? GetScheduledTime (string executorId, string eventType, string? eventId = null) => Instance.GetScheduledTimeInternal(executorId, eventType, eventId);
		/// <summary>
		/// Get when is a event is scheeduled to happen. Can return null if there's no such event scheduled.
		/// </summary>
		public TLDDateTime? GetScheduledTimeInternal (string executorId, string eventType, string? eventId = null)
		{
            string key = $"{executorId}-{eventType}-{eventId}";
			TLDDateTime? time = null;
            if (eventCache.TryGetValue(key, out var ev)) time = ev.Time;
			return time;
		}

	}

	// Restore triggers:
	// Entering scene, loaded saved game
	// Save triggers:
	// Leaving scene, sleeping, anyking of saving, entered new game

	// In order to skip restoration/saving to reduce unnecessary performance cost
	// - Skip save when there's no change to the scheduled events
	// - Load only when a saved game is loaded, switching regions can be skipped

	// Things to note:
	// - Restoration will not happens after entering a new save, instead, it triggers a saving
	// - Restoration should still happens when quiting and loading the same game, because it's possible to quit without saving


    [HarmonyPatch(nameof(SaveGameSystem), nameof(SaveGameSystem.RestoreGlobalData))]
	internal static class RestoreGlobalData
	{
    	[HarmonyPriority(Priority.VeryHigh)]
		internal static void Postfix (string name)
		{
            bool inGame = Check.InGame;
			// Moment.Instance.LoggerInstance?.Msg($"---RestoreGlobalData { SaveGameSystem.m_CurrentGameId }---(InGame: {inGame})");
            if (!inGame) return;
			if (Moment.Instance.scheduledEvents != null) return;
			Moment.Instance.scheduledEvents = new List<ScheduledEvent>();
			Moment.Instance.eventCache.Clear();
			var savedEvents = Moment.Instance.ModSave.Load("events");
			// Moment.Instance.LoggerInstance?.Msg($"Decoding: {savedEvents} for game { name }");
			if (savedEvents == null) return;
			var decodedArr = MelonLoader.TinyJSON.Decoder.Decode(savedEvents) as MelonLoader.TinyJSON.ProxyArray;
			if (decodedArr == null)
			{
				Moment.Instance.LoggerInstance?.Error("Failed to decode into array.");
				return;
			}
			foreach (var evDecoded in decodedArr)
			{
				if (evDecoded == null) continue;
				ScheduledEvent ev = new ();
				evDecoded.Populate<ScheduledEvent>(ev);
				if (string.IsNullOrWhiteSpace(ev.ExecutorId) || string.IsNullOrWhiteSpace(ev.EventType)) continue;
				if (Moment.Instance.IsScheduledInternal(ev.ExecutorId, ev.EventType, ev.EventId)) continue;
				Moment.Instance.scheduledEvents.Add(ev);
				Moment.Instance.eventCache[ev.EventUID] = ev;
				// Moment.Instance.LoggerInstance?.Msg($"Re-scheduled: {ev.EventUID} at {ev.Time} ({(ev.Time - Moment.Instance.NowInternal).ToStringRelative()})");
			}
		}
	}

	[HarmonyPatch(typeof(SaveGameSlots), nameof(SaveGameSlots.CreateSlot), new Type[] { typeof(string), typeof(SaveSlotType), typeof(uint), typeof(Episode) })]
	static class ModData_SaveGameSlots_CreateSaveSlotInfo
	{
		[HarmonyPriority(Priority.Last)]
		private static void Prefix(string slotname, SaveSlotType gameMode, uint gameId, Episode episode)
		{
			Moment.Instance.scheduledEvents = new List<ScheduledEvent>();
			SaveGlobalData.changedSinceLastSave = true;
		}
	}

    [HarmonyPatch(nameof(SaveGameSystem), nameof(SaveGameSystem.SaveGlobalData))]
	internal static class SaveGlobalData
	{
		internal static bool changedSinceLastSave;
    	[HarmonyPriority(Priority.VeryHigh)]
		internal static void Postfix (SlotData slot)
		{
            bool inGame = Check.InGame;
			// Moment.Instance.LoggerInstance?.Msg($"---SaveGlobalData { slot.m_GameId }---(InGame: {inGame})");
            if (!inGame)
			{
				return;
			}
			if (!changedSinceLastSave) return;

            string data = MelonLoader.TinyJSON.Encoder.Encode(Moment.Instance.scheduledEvents);
			Moment.Instance.LoggerInstance?.Msg($"Encoded: {data}");
            Moment.Instance.ModSave.Save(data, "events");
            Moment.Instance.ModSave.Save(Moment.Now.TotalMinutes.ToString(), "lastSaved"); // workaround for ModData issue
			changedSinceLastSave = false;
		}
	}

    [HarmonyPatch(nameof(GameManager), nameof(GameManager.OnGameQuit))]
	internal static class OnGameQuit
	{
		internal static void Postfix ()
		{
			// Moment.Instance.LoggerInstance?.Msg($"---OnGameQuit---");
			Moment.Instance.scheduledEvents = null;
			Moment.Instance.eventCache.Clear();
		}
	}
    [HarmonyPatch(nameof(GameManager), nameof(GameManager.HandlePlayerDeath))]
	internal static class HandlePlayerDeath
	{
		internal static void Postfix ()
		{
			// Moment.Instance.LoggerInstance?.Msg($"---HandlePlayerDeath---");
            Moment.Instance.ModSave.Save("", "events"); // workaround for moddata not getting deleted
			Moment.Instance.scheduledEvents = null;
			Moment.Instance.eventCache.Clear();
		}
	}
	// [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.SaveCompletedInternal), new Type[] { typeof(bool) })]
	// static class ModData_SaveGameSystem_SaveCompletedInternal
	// {
	// 	private static void Postfix()
	// 	{
	// 		Moment.Instance.LoggerInstance?.Msg($"---SaveCompletedInternal---");
	// 	}
	// }
}
