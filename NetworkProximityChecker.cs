using System;
using System.Collections.Generic;
namespace UnityEngine.Networking
{
	/// <summary>
	///   <para>Component that controls visibility of networked objects for players.</para>
	/// </summary>
	[AddComponentMenu("Network/NetworkProximityChecker"), RequireComponent(typeof(NetworkIdentity))]
	public class NetworkProximityChecker : NetworkBehaviour
	{
		/// <summary>
		///   <para>Enumeration of methods to use to check proximity.</para>
		/// </summary>
		public enum CheckMethod
		{
			/// <summary>
			///   <para>Use 3D physics to determine proximity.</para>
			/// </summary>
			Physics3D,
			/// <summary>
			///   <para>Use 2D physics to determine proximity.</para>
			/// </summary>
			Physics2D
		}
		/// <summary>
		///   <para>The maximim range that objects will be visible at.</para>
		/// </summary>
		public int visRange = 10;
		/// <summary>
		///   <para>How often (in seconds) that this object should update the set of players that can see it.</para>
		/// </summary>
		public float visUpdateInterval = 1f;
		/// <summary>
		///   <para>Which method to use for checking proximity of players.</para>
		/// </summary>
		public NetworkProximityChecker.CheckMethod checkMethod;
		/// <summary>
		///   <para>Flag to force this object to be hidden for players.</para>
		/// </summary>
		public bool forceHidden;
		private float visUpdateTime;
		private void Update()
		{
			if (!NetworkServer.active)
			{
				return;
			}
			if (Time.get_time() - this.visUpdateTime > this.visUpdateInterval)
			{
				base.GetComponent<NetworkIdentity>().RebuildObservers(false);
				this.visUpdateTime = Time.get_time();
			}
		}
		public override bool OnCheckObserver(NetworkConnection newObserver)
		{
			if (this.forceHidden)
			{
				return false;
			}
			PlayerController playerController = newObserver.playerControllers[0];
			Vector3 position = playerController.unetView.get_gameObject().get_transform().get_position();
			return (position - base.get_transform().get_position()).get_magnitude() < (float)this.visRange;
		}
		public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
		{
			if (this.forceHidden)
			{
				NetworkIdentity component = base.GetComponent<NetworkIdentity>();
				if (component.connectionToClient != null)
				{
					observers.Add(component.connectionToClient);
				}
				return true;
			}
			NetworkProximityChecker.CheckMethod checkMethod = this.checkMethod;
			if (checkMethod == NetworkProximityChecker.CheckMethod.Physics3D)
			{
				Collider[] array = Physics.OverlapSphere(base.get_transform().get_position(), (float)this.visRange);
				Collider[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					Collider collider = array2[i];
					NetworkIdentity component2 = collider.GetComponent<NetworkIdentity>();
					if (component2 != null && component2.connectionToClient != null)
					{
						observers.Add(component2.connectionToClient);
					}
				}
				return true;
			}
			if (checkMethod != NetworkProximityChecker.CheckMethod.Physics2D)
			{
				return false;
			}
			Collider2D[] array3 = Physics2D.OverlapCircleAll(base.get_transform().get_position(), (float)this.visRange);
			Collider2D[] array4 = array3;
			for (int j = 0; j < array4.Length; j++)
			{
				Collider2D collider2D = array4[j];
				NetworkIdentity component3 = collider2D.GetComponent<NetworkIdentity>();
				if (component3 != null && component3.connectionToClient != null)
				{
					observers.Add(component3.connectionToClient);
				}
			}
			return true;
		}
		public override void OnSetLocalVisibility(bool vis)
		{
			NetworkProximityChecker.SetVis(base.get_gameObject(), vis);
		}
		private static void SetVis(GameObject go, bool vis)
		{
			Renderer[] components = go.GetComponents<Renderer>();
			for (int i = 0; i < components.Length; i++)
			{
				Renderer renderer = components[i];
				renderer.set_enabled(vis);
			}
			for (int j = 0; j < go.get_transform().get_childCount(); j++)
			{
				Transform child = go.get_transform().GetChild(j);
				NetworkProximityChecker.SetVis(child.get_gameObject(), vis);
			}
		}
	}
}
