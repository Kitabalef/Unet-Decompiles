using System;
using System.Collections.Generic;
namespace UnityEngine.Networking
{
	internal class NetworkScene
	{
		internal Dictionary<NetworkInstanceId, NetworkIdentity> m_LocalObjects = new Dictionary<NetworkInstanceId, NetworkIdentity>();
		internal static Dictionary<NetworkHash128, GameObject> s_GUIDToPrefab = new Dictionary<NetworkHash128, GameObject>();
		internal static Dictionary<NetworkHash128, SpawnDelegate> s_SpawnHandlers = new Dictionary<NetworkHash128, SpawnDelegate>();
		internal static Dictionary<NetworkHash128, UnSpawnDelegate> s_UnspawnHandlers = new Dictionary<NetworkHash128, UnSpawnDelegate>();
		internal void Shutdown()
		{
			this.ClearLocalObjects();
			NetworkScene.ClearSpawners();
		}
		internal void SetLocalObject(NetworkInstanceId netId, GameObject obj, bool isClient, bool isServer)
		{
			if (LogFilter.logDev)
			{
				Debug.Log(string.Concat(new object[]
				{
					"SetLocalObject ",
					netId,
					" ",
					obj
				}));
			}
			if (obj == null)
			{
				this.m_LocalObjects[netId] = null;
				return;
			}
			NetworkIdentity networkIdentity = null;
			if (this.m_LocalObjects.ContainsKey(netId))
			{
				networkIdentity = this.m_LocalObjects[netId];
			}
			if (networkIdentity == null)
			{
				networkIdentity = obj.GetComponent<NetworkIdentity>();
				this.m_LocalObjects[netId] = networkIdentity;
			}
			networkIdentity.UpdateClientServer(isClient, isServer);
		}
		internal GameObject FindLocalObject(NetworkInstanceId netId)
		{
			if (this.m_LocalObjects.ContainsKey(netId))
			{
				return this.m_LocalObjects[netId].get_gameObject();
			}
			return null;
		}
		internal bool GetNetworkIdentity(NetworkInstanceId netId, out NetworkIdentity uv)
		{
			if (this.m_LocalObjects.ContainsKey(netId) && this.m_LocalObjects[netId] != null)
			{
				uv = this.m_LocalObjects[netId];
				return true;
			}
			uv = null;
			return false;
		}
		internal bool RemoveLocalObject(NetworkInstanceId netId)
		{
			return this.m_LocalObjects.Remove(netId);
		}
		internal bool RemoveLocalObjectAndDestroy(NetworkInstanceId netId)
		{
			if (this.m_LocalObjects.ContainsKey(netId))
			{
				NetworkIdentity networkIdentity = this.m_LocalObjects[netId];
				Object.Destroy(networkIdentity.get_gameObject());
				return this.m_LocalObjects.Remove(netId);
			}
			return false;
		}
		internal void ClearLocalObjects()
		{
			this.m_LocalObjects.Clear();
		}
		internal static void RegisterPrefab(GameObject prefab)
		{
			NetworkIdentity component = prefab.GetComponent<NetworkIdentity>();
			if (component)
			{
				if (LogFilter.logDebug)
				{
					Debug.Log(string.Concat(new object[]
					{
						"Registering prefab '",
						prefab.get_name(),
						"' as asset:",
						component.assetId
					}));
				}
				NetworkScene.s_GUIDToPrefab[component.assetId] = prefab;
			}
			else if (LogFilter.logError)
			{
				Debug.LogError("Could not register '" + prefab.get_name() + "' since it contains no NetworkIdentity component");
			}
		}
		internal static bool GetPrefab(NetworkHash128 assetId, out GameObject prefab)
		{
			if (!assetId.IsValid())
			{
				prefab = null;
				return false;
			}
			if (NetworkScene.s_GUIDToPrefab.ContainsKey(assetId) && NetworkScene.s_GUIDToPrefab[assetId] != null)
			{
				prefab = NetworkScene.s_GUIDToPrefab[assetId];
				return true;
			}
			prefab = null;
			return false;
		}
		internal static void ClearSpawners()
		{
			NetworkScene.s_GUIDToPrefab.Clear();
			NetworkScene.s_SpawnHandlers.Clear();
			NetworkScene.s_UnspawnHandlers.Clear();
		}
		public static void UnregisterSpawnHandler(NetworkHash128 assetId)
		{
			NetworkScene.s_SpawnHandlers.Remove(assetId);
			NetworkScene.s_UnspawnHandlers.Remove(assetId);
		}
		internal static void RegisterSpawnHandler(NetworkHash128 assetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
		{
			if (spawnHandler == null || unspawnHandler == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("RegisterSpawnHandler custom spawn function null for " + assetId);
				}
				return;
			}
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"RegisterSpawnHandler asset '",
					assetId,
					"' ",
					spawnHandler.Method.Name,
					"/",
					unspawnHandler.Method.Name
				}));
			}
			NetworkScene.s_SpawnHandlers[assetId] = spawnHandler;
			NetworkScene.s_UnspawnHandlers[assetId] = unspawnHandler;
		}
		internal static void UnregisterPrefab(GameObject prefab)
		{
			NetworkIdentity component = prefab.GetComponent<NetworkIdentity>();
			if (component == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("Could not unregister '" + prefab.get_name() + "' since it contains no NetworkIdentity component");
				}
				return;
			}
			NetworkScene.s_SpawnHandlers.Remove(component.assetId);
			NetworkScene.s_UnspawnHandlers.Remove(component.assetId);
		}
		internal static void RegisterPrefab(GameObject prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
		{
			NetworkIdentity component = prefab.GetComponent<NetworkIdentity>();
			if (component == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("Could not register '" + prefab.get_name() + "' since it contains no NetworkIdentity component");
				}
				return;
			}
			if (spawnHandler == null || unspawnHandler == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("RegisterPrefab custom spawn function null for " + component.assetId);
				}
				return;
			}
			if (!component.assetId.IsValid())
			{
				if (LogFilter.logError)
				{
					Debug.LogError("RegisterPrefab game object " + prefab.get_name() + " has no prefab. Use RegisterSpawnHandler() instead?");
				}
				return;
			}
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"Registering custom prefab '",
					prefab.get_name(),
					"' as asset:",
					component.assetId,
					" ",
					spawnHandler.Method.Name,
					"/",
					unspawnHandler.Method.Name
				}));
			}
			NetworkScene.s_SpawnHandlers[component.assetId] = spawnHandler;
			NetworkScene.s_UnspawnHandlers[component.assetId] = unspawnHandler;
		}
		internal static bool GetSpawnHandler(NetworkHash128 assetId, out SpawnDelegate handler)
		{
			if (NetworkScene.s_SpawnHandlers.ContainsKey(assetId))
			{
				handler = NetworkScene.s_SpawnHandlers[assetId];
				return true;
			}
			handler = null;
			return false;
		}
		internal static bool InvokeUnSpawnHandler(NetworkHash128 assetId, GameObject obj)
		{
			if (NetworkScene.s_UnspawnHandlers.ContainsKey(assetId) && NetworkScene.s_UnspawnHandlers[assetId] != null)
			{
				UnSpawnDelegate unSpawnDelegate = NetworkScene.s_UnspawnHandlers[assetId];
				unSpawnDelegate(obj);
				return true;
			}
			return false;
		}
		internal void DestroyAllClientObjects()
		{
			foreach (NetworkInstanceId current in this.m_LocalObjects.Keys)
			{
				NetworkIdentity networkIdentity = this.m_LocalObjects[current];
				if (networkIdentity != null && networkIdentity.get_gameObject() != null)
				{
					if (networkIdentity.sceneId.IsEmpty())
					{
						Object.Destroy(networkIdentity.get_gameObject());
					}
					else
					{
						networkIdentity.get_gameObject().SetActive(false);
					}
				}
			}
			this.ClearLocalObjects();
		}
		internal void DumpAllClientObjects()
		{
			foreach (NetworkInstanceId current in this.m_LocalObjects.Keys)
			{
				NetworkIdentity networkIdentity = this.m_LocalObjects[current];
				if (networkIdentity != null)
				{
					Debug.Log(string.Concat(new object[]
					{
						"ID:",
						current,
						" OBJ:",
						networkIdentity.get_gameObject(),
						" AS:",
						networkIdentity.assetId
					}));
				}
				else
				{
					Debug.Log("ID:" + current + " OBJ: null");
				}
			}
		}
	}
}
