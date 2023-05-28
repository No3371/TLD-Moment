# Moment

A timing utility framework mod for the game The Long Dark.

## Usage

Easily get called every ingame day/hour/minute:
```csharp
Moment.Moment.OnHourChanged += (t) => {
    MelonLogger.Msg($"Current time: { t }");
};
```

Schedule your event to happens at a specific ingame day and time:
```csharp
// get called at first day 22:00
Moment.Moment.Schedule(Muscle.Instance, new EventRequest((0, 22, 0), "losingMuscle")); 
```

Or make it happen after some time:
```csharp
// get called 22 hours later
Moment.Moment.ScheduleRelative(Muscle.Instance, new EventRequest((0, 22, 0), "losingMuscle")); 
```

Check if an event is scheduled and schedule one if not:
```csharp
if (!Moment.Moment.IsScheduled(Muscle.Instance.ScheduledEventExecutorId, "losingMuscle"))
    Moment.Moment.ScheduleRelative(Muscle.Instance, new EventRequest((0, 22, 0), "losingMuscle"));
```

## Dependencies

- [dommrogers's ModData](https://github.com/dommrogers/ModData/)