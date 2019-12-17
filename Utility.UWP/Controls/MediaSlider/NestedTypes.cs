using System.Collections.Generic;

namespace JLR.Utility.UWP.Controls
{
	public sealed partial class MediaSlider
	{
		#region Types (Public)
		public class MediaSliderMarker
		{
			public string Name { get; }
			public decimal Time { get; }

			public MediaSliderMarker(string name, decimal time)
			{
				Name = name;
				Time = time;
			}
		}

		public class MediaSliderClip
		{
			public string Name { get; }
			public decimal StartTime { get; }
			public decimal EndTime { get; }

			public MediaSliderClip(string name, decimal startTime, decimal endTime)
			{
				Name      = name;
				StartTime = startTime;
				EndTime   = endTime;
			}
		}
		#endregion

		#region Enumerated Types (Public)
		/// <summary>
		/// Specifies the way in which the visible window
		/// adjusts to keep an item visible
		/// (the position thumb, for example).
		/// </summary>
		public enum FollowMode
		{
			/// <summary>
			/// Visible window is never automatically adjusted.
			/// </summary>
			NoFollow,

			/// <summary>
			/// Advance the visible window by its current duration
			/// in the direction of change.
			/// </summary>
			Advance,

			/// <summary>
			/// Once the item reaches the center of the visible window,
			/// keep it centered by moving the window based on the
			/// amount and direction of change.
			/// </summary>
			Scroll
		}

		public enum TimeIntervals
		{
			Frame,
			Millisecond,
			Second,
			Minute,
			Hour,
			Day
		}
		#endregion

		#region Types (Private)
		private class TickTypeComparer : IComparer<KeyValuePair<TickType, int>>
		{
			public int Compare(KeyValuePair<TickType, int> x, KeyValuePair<TickType, int> y)
			{
				if (x.Value != y.Value)
					return x.Value.CompareTo(y.Value);

				var result = 0;
				switch (x.Key)
				{
					case TickType.Origin:
					{
						if (y.Key == TickType.Major || y.Key == TickType.Minor)
							result = 1;
						break;
					}

					case TickType.Major:
					{
						switch (y.Key)
						{
							case TickType.Origin:
								result = -1;
								break;
							case TickType.Minor:
								result = 1;
								break;
						}

						break;
					}

					case TickType.Minor:
					{
						if (y.Key == TickType.Origin || y.Key == TickType.Major)
							result = -1;
						break;
					}
				}

				return result;
			}
		}
		#endregion

		#region Enumerated Types (Private)
		private enum TickType
		{
			Origin,
			Major,
			Minor
		}
		#endregion
	}
}