using System;
namespace UnityEngine.Networking
{
	/// <summary>
	///   <para>This attribute is used to configure the network settings of scripts that are derived from the NetworkBehaviour base class.</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class NetworkSettingsAttribute : Attribute
	{
		/// <summary>
		///   <para>The QoS channel to use for updates for this script.</para>
		/// </summary>
		public int channel;
		/// <summary>
		///   <para>The sendInterval control how frequently updates are sent for this script.</para>
		/// </summary>
		public float sendInterval = 0.1f;
	}
}
