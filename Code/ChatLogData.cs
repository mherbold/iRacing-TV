
namespace iRacingTV
{
	internal class ChatLogData
	{
		public double startSessionTime;
		public double endSessionTime;

		public string text;

		public ChatLogData( double startSessionTime, double endSessionTime, string text )
		{
			this.startSessionTime = startSessionTime;
			this.endSessionTime = endSessionTime;

			this.text = text;
		}
	}
}
