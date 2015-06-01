using System;
namespace UnityEngine.Networking.NetworkSystem
{
	/// <summary>
	///   <para>Information about another participant in the same network game.</para>
	/// </summary>
	public class PeerInfoMessage : MessageBase
	{
		/// <summary>
		///   <para>The id of the NetworkConnection associated with the peer.</para>
		/// </summary>
		public int connectionId;
		/// <summary>
		///   <para>The IP address of the peer.</para>
		/// </summary>
		public string address;
		/// <summary>
		///   <para>The network port being used by the peer.</para>
		/// </summary>
		public int port;
		/// <summary>
		///   <para>True if this peer is the host of the network game.</para>
		/// </summary>
		public bool isHost;
		/// <summary>
		///   <para>True if the peer if the same as the current client.</para>
		/// </summary>
		public bool isYou;
		public override void Deserialize(NetworkReader reader)
		{
			this.connectionId = (int)reader.ReadPackedUInt32();
			this.address = reader.ReadString();
			this.port = (int)reader.ReadPackedUInt32();
			this.isHost = reader.ReadBoolean();
			this.isYou = reader.ReadBoolean();
		}
		public override void Serialize(NetworkWriter writer)
		{
			writer.WritePackedUInt32((uint)this.connectionId);
			writer.Write(this.address);
			writer.WritePackedUInt32((uint)this.port);
			writer.Write(this.isHost);
			writer.Write(this.isYou);
		}
	}
}
