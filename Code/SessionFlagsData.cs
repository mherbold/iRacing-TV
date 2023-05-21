
namespace iRacingTV
{
	internal class SessionFlagsData
	{
		public int sessionTick;
		public double sessionTime;
		public uint sessionFlags;

		public SessionFlagsData( int sessionTick, double sessionTime, uint sessionFlags )
		{
			this.sessionTick = sessionTick;
			this.sessionTime = sessionTime;
			this.sessionFlags = sessionFlags;
		}
	}
}
