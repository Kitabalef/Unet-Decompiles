using System;
using System.Collections.Generic;
namespace UnityEngine.Networking
{
	internal class NetworkMessageHandlers
	{
		private Dictionary<short, NetworkMessageDelegate> m_MsgHandlers = new Dictionary<short, NetworkMessageDelegate>();
		private NetworkMessage m_MessageInfo = new NetworkMessage();
		internal void RegisterHandlerSafe(short msgType, NetworkMessageDelegate handler)
		{
			if (handler == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("RegisterHandlerSafe id:" + msgType + " handler is null");
				}
				return;
			}
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"RegisterHandlerSafe id:",
					msgType,
					" handler:",
					handler.Method.Name
				}));
			}
			if (this.m_MsgHandlers.ContainsKey(msgType))
			{
				return;
			}
			this.m_MsgHandlers.Add(msgType, handler);
		}
		public void RegisterHandler(short msgType, NetworkMessageDelegate handler)
		{
			if (handler == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("RegisterHandler id:" + msgType + " handler is null");
				}
				return;
			}
			if (msgType <= 31)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("RegisterHandler: Cannot replace system message handler " + msgType);
				}
				return;
			}
			if (this.m_MsgHandlers.ContainsKey(msgType))
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("RegisterHandler replacing " + msgType);
				}
				this.m_MsgHandlers.Remove(msgType);
			}
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"RegisterHandler id:",
					msgType,
					" handler:",
					handler.Method.Name
				}));
			}
			this.m_MsgHandlers.Add(msgType, handler);
		}
		public void UnregisterHandler(short msgType)
		{
			this.m_MsgHandlers.Remove(msgType);
		}
		internal bool InvokeHandlerNoData(short msgType, NetworkConnection conn)
		{
			return this.InvokeHandler(msgType, conn, null, 0);
		}
		internal bool InvokeHandler(short msgType, NetworkConnection conn, NetworkReader reader, int channelId)
		{
			if (this.m_MsgHandlers.ContainsKey(msgType))
			{
				this.m_MessageInfo.msgType = msgType;
				this.m_MessageInfo.conn = conn;
				this.m_MessageInfo.reader = reader;
				this.m_MessageInfo.channelId = channelId;
				NetworkMessageDelegate networkMessageDelegate = this.m_MsgHandlers[msgType];
				networkMessageDelegate(this.m_MessageInfo);
				return true;
			}
			return false;
		}
		internal bool InvokeHandler(NetworkMessage netMsg)
		{
			if (this.m_MsgHandlers.ContainsKey(netMsg.msgType))
			{
				NetworkMessageDelegate networkMessageDelegate = this.m_MsgHandlers[netMsg.msgType];
				networkMessageDelegate(netMsg);
				return true;
			}
			return false;
		}
		internal NetworkMessageDelegate GetHandler(short msgType)
		{
			if (this.m_MsgHandlers.ContainsKey(msgType))
			{
				return this.m_MsgHandlers[msgType];
			}
			return null;
		}
		public Dictionary<short, NetworkMessageDelegate> GetHandlers()
		{
			return this.m_MsgHandlers;
		}
		internal void ClearMessageHandlers()
		{
			this.m_MsgHandlers.Clear();
		}
	}
}
