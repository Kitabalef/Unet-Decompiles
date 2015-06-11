using System;
namespace UnityEngine.Networking
{
	/// <summary>
	///   <para>The details of a network message received by a client or server on a network connection.</para>
	/// </summary>
	public class NetworkMessage
	{
		/// <summary>
		///   <para>The id of the message type of the message.</para>
		/// </summary>
		public short msgType;
		/// <summary>
		///   <para>The connection the message was recieved on.</para>
		/// </summary>
		public NetworkConnection conn;
		/// <summary>
		///   <para>A NetworkReader object that contains the contents of the message.</para>
		/// </summary>
		public NetworkReader reader;
		/// <summary>
		///   <para>The transport layer channel the message was sent on.</para>
		/// </summary>
		public int channelId;
		/// <summary>
		///   <para>Returns a string with the numeric representation of each byte in the payload.</para>
		/// </summary>
		/// <param name="payload">Network message payload to dump.</param>
		/// <param name="sz">Length of payload in bytes.</param>
		/// <returns>
		///   <para>Dumped info from payload.</para>
		/// </returns>
		public static string Dump(byte[] payload, int sz)
		{
			string str = "[";
			for (int i = 0; i < sz; i++)
			{
				str = str + payload[i].ToString() + " ";
			}
			return str + "]";
		}
		public MSG ReadMessage<MSG>() where MSG : MessageBase, new()
		{
			MSG result = Activator.CreateInstance<MSG>();
			result.Deserialize(this.reader);
			return result;
		}
		public void ReadMessage<MSG>(MSG msg) where MSG : MessageBase
		{
			msg.Deserialize(this.reader);
		}
	}
}
