using MelonLoader;

namespace Moment
{
    public static class Tests
	{
		public static void Run ()
		{
			Func<bool>[] tests = new Func<bool>[] { Test1, Test2, Test3, Test4, Test5, Test6, Test7, Test8 };
			for (int i = 0; i < tests.Length; i++)
			{
				if (tests[i]())
					MelonLogger.Msg($"Test#{i+1} passed");
				else 
					MelonLogger.Error($"Test#{i+1} failed");
			}
		}

		public static bool Test1()
		{
			TLDDateTime t = new (1, 0, 0);
			MelonLogger.Msg($"{t} + { new TLDDateTime(0, 25, 25) } -> {t + (0, 25, 25)}");
			t += (0, 25, 25);
			return (t.Day == 2 && t.Hour == 1 && t.Minute == 25);
		}

		public static bool Test2()
		{
			TLDDateTime t = new (2, 1, 25);
			MelonLogger.Msg($"{t} + { new TLDDateTime(-1, -1, -25) } -> {t - (1, 1, 25)}");
			t -= (1, 1, 25);
			return (t.Day == 1 && t.Hour == 0 && t.Minute == 0);
		}

		public static bool Test3()
		{
			TLDDateTime t = new (2, 1, 25);
			MelonLogger.Msg($"{t} + { new TLDDateTime(0, 0, -26) } -> {t - (0, 0, 26)}");
			t -= (0, 0, 26);
			return (t.Day == 2 && t.Hour == 0 && t.Minute == 59);
		}

		public static bool Test4()
		{
			TLDDateTime t = new (2, 1, 25);
			MelonLogger.Msg($"{t} + { new TLDDateTime(0, -24, 0) } -> {t - (0, 24, 0)}");
			t -= (0, 24, 0);
			return (t.Day == 1 && t.Hour == 1 && t.Minute == 25);
		}

		public static bool Test5()
		{
			TLDDateTime t = new (2, 1, 25);
			MelonLogger.Msg($"{t} + { new TLDDateTime(0, 0, -1440) } -> {t - (0, 0, 1440)}");
			t -= (0, 0, 1440);
			return (t.Day == 1 && t.Hour == 1 && t.Minute == 25);
		}

		public static bool Test6()
		{
			TLDDateTime t = new (2, 1, 25);
			MelonLogger.Msg($"{t} + { new TLDDateTime(0, -48, 0) } -> {t - (0, 48, 0)}");
			t -= (0, 48, 0);
			return (t.Day == 0 && t.Hour == 1 && t.Minute == 25);
		}

		public static bool Test7()
		{
			TLDDateTime t = new (8, 23, 59);
			MelonLogger.Msg($"{t} + { new TLDDateTime(0, 0, -4380) } -> {t - (0, 0, 4380)}");
			t -= (0, 0, 4380);
			return (t.Day == 5 && t.Hour == 22 && t.Minute == 59);
		}

		public static bool Test8()
		{
			TLDDateTime t = new (8, 23, 59);
			MelonLogger.Msg($"{t} + { new TLDDateTime(0, 25, 720) } -> {t + (0, 25, 720)}");
			t += (0, 25, 720);
			return (t.Day == 10 && t.Hour == 12 && t.Minute == 59);
		}

		public static bool Test9()
		{
			TLDDateTime t = new (0, 74, 488);
			MelonLogger.Msg($"{t} + { new TLDDateTime(0, 25, 720) } -> {t + (0, 25, 720)}");
			t += (0, 25, 720);
			return (t.Day == 10 && t.Hour == 12 && t.Minute == 59);
		}
	}
}
