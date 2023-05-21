using irsdkSharp.Enums;

namespace iRacingTV
{
	internal class Message
	{
		public BroadcastMessageTypes msg;

		public int var1;
		public int var2;
		public int var3;

		public Message( BroadcastMessageTypes msg, int var1, int var2, int var3 )
		{
			this.msg = msg;

			this.var1 = var1;
			this.var2 = var2;
			this.var3 = var3;
		}
	}
}
