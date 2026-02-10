namespace Obsydian.Core.Time;

/// <summary>
/// In-game clock with configurable time scale, day/night phases, and calendar.
/// One real second = TimeScale game minutes by default.
/// </summary>
public sealed class GameClock
{
    /// <summary>How many game minutes pass per real second.</summary>
    public float TimeScale { get; set; } = 10f;

    /// <summary>Current hour (0-23).</summary>
    public int Hour { get; private set; } = 6;

    /// <summary>Current minute (0-59).</summary>
    public int Minute { get; private set; }

    /// <summary>Current day number (1-based).</summary>
    public int Day { get; private set; } = 1;

    /// <summary>Current season (0-3).</summary>
    public int Season { get; private set; }

    /// <summary>Current year (1-based).</summary>
    public int Year { get; private set; } = 1;

    /// <summary>Days per season.</summary>
    public int DaysPerSeason { get; set; } = 28;

    /// <summary>Number of seasons per year.</summary>
    public int SeasonsPerYear { get; set; } = 4;

    /// <summary>Total game minutes elapsed since start.</summary>
    public double TotalMinutes { get; private set; }

    public bool IsPaused { get; set; }

    /// <summary>Current phase of day.</summary>
    public DayPhase Phase => Hour switch
    {
        >= 5 and < 8 => DayPhase.Dawn,
        >= 8 and < 12 => DayPhase.Morning,
        >= 12 and < 17 => DayPhase.Afternoon,
        >= 17 and < 20 => DayPhase.Evening,
        _ => DayPhase.Night
    };

    /// <summary>Normalized time of day (0.0 = midnight, 0.5 = noon).</summary>
    public float NormalizedTime => (Hour * 60 + Minute) / 1440f;

    // Events
    public event Action<int, int>? OnMinuteChanged;
    public event Action<int>? OnHourChanged;
    public event Action<DayPhase>? OnPhaseChanged;
    public event Action<int>? OnDayChanged;
    public event Action<int>? OnSeasonChanged;

    private float _accumulator;
    private DayPhase _lastPhase;
    private int _lastHour;

    public GameClock()
    {
        _lastPhase = Phase;
        _lastHour = Hour;
    }

    /// <summary>Call once per frame with real deltaTime in seconds.</summary>
    public void Update(float deltaTime)
    {
        if (IsPaused) return;

        _accumulator += deltaTime * TimeScale;

        while (_accumulator >= 1f)
        {
            _accumulator -= 1f;
            AdvanceMinute();
        }
    }

    private void AdvanceMinute()
    {
        Minute++;
        TotalMinutes++;
        OnMinuteChanged?.Invoke(Hour, Minute);

        if (Minute >= 60)
        {
            Minute = 0;
            Hour++;

            if (Hour >= 24)
            {
                Hour = 0;
                AdvanceDay();
            }

            OnHourChanged?.Invoke(Hour);
        }

        if (_lastHour != Hour)
        {
            _lastHour = Hour;
            var newPhase = Phase;
            if (newPhase != _lastPhase)
            {
                _lastPhase = newPhase;
                OnPhaseChanged?.Invoke(newPhase);
            }
        }
    }

    private void AdvanceDay()
    {
        Day++;
        OnDayChanged?.Invoke(Day);

        if (Day > DaysPerSeason)
        {
            Day = 1;
            Season++;

            if (Season >= SeasonsPerYear)
            {
                Season = 0;
                Year++;
            }

            OnSeasonChanged?.Invoke(Season);
        }
    }

    /// <summary>Set the clock to a specific time.</summary>
    public void SetTime(int hour, int minute = 0)
    {
        Hour = System.Math.Clamp(hour, 0, 23);
        Minute = System.Math.Clamp(minute, 0, 59);
        _lastPhase = Phase;
        _lastHour = Hour;
    }

    /// <summary>Set the date.</summary>
    public void SetDate(int day, int season = 0, int year = 1)
    {
        Day = System.Math.Max(1, day);
        Season = System.Math.Clamp(season, 0, SeasonsPerYear - 1);
        Year = System.Math.Max(1, year);
    }

    public string TimeString => $"{Hour:D2}:{Minute:D2}";
    public string DateString => $"Y{Year} S{Season + 1} D{Day}";
}

public enum DayPhase
{
    Dawn,
    Morning,
    Afternoon,
    Evening,
    Night
}
