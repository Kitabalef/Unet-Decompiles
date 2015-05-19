﻿// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.SyncList`1
// Assembly: UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CAEE5E9F-B085-483B-8DC7-7FEB12D926E7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\UnityExtensions\Unity\Networking\UnityEngine.Networking.dll

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.Networking
{
  /// <summary>
  /// 
  /// <para>
  /// This is the base class for type-specific SyncList classes.
  /// </para>
  /// 
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public abstract class SyncList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
  {
    private List<T> m_Objects = new List<T>();
    private NetworkBehaviour m_Behaviour;
    private int m_CmdHash;
    private SyncList<T>.SyncListChanged m_Callback;

    public int Count
    {
      get
      {
        return this.m_Objects.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public SyncList<T>.SyncListChanged Callback
    {
      get
      {
        return this.m_Callback;
      }
      set
      {
        this.m_Callback = value;
      }
    }

    public T this[int i]
    {
      get
      {
        return this.m_Objects[i];
      }
      set
      {
        this.SendMsg(SyncList<T>.Operation.OP_SET, i, value);
        this.m_Objects[i] = value;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this.GetEnumerator();
    }

    protected abstract void SerializeItem(NetworkWriter writer, T item);

    protected abstract T DeserializeItem(NetworkReader reader);

    public void InitializeBehaviour(NetworkBehaviour beh, int cmdHash)
    {
      this.m_Behaviour = beh;
      this.m_CmdHash = cmdHash;
    }

    private void SendMsg(SyncList<T>.Operation op, int itemIndex, T item)
    {
      if ((Object) this.m_Behaviour == (Object) null)
      {
        if (!LogFilter.logError)
          return;
        Debug.LogError((object) "SyncList not initialized");
      }
      else
      {
        NetworkIdentity component = this.m_Behaviour.GetComponent<NetworkIdentity>();
        if ((Object) component == (Object) null)
        {
          if (!LogFilter.logError)
            return;
          Debug.LogError((object) "SyncList no NetworkIdentity");
        }
        else
        {
          NetworkWriter writer = new NetworkWriter();
          writer.StartMessage((short) 9);
          writer.Write(component.netId);
          writer.WritePackedUInt32((uint) this.m_CmdHash);
          writer.Write((byte) op);
          writer.WritePackedUInt32((uint) itemIndex);
          this.SerializeItem(writer, item);
          writer.FinishMessage();
          NetworkServer.SendWriterToReady(component.gameObject, writer, 0);
          NetworkDetailStats.IncrementStat(NetworkDetailStats.NetworkDirection.Outgoing, (short) 9, op.ToString(), 1);
          if (!this.m_Behaviour.isServer || !this.m_Behaviour.isClient || this.m_Callback == null)
            return;
          this.m_Callback(op, itemIndex);
        }
      }
    }

    private void SendMsg(SyncList<T>.Operation op, int itemIndex)
    {
      this.SendMsg(op, itemIndex, default (T));
    }

    public void HandleMsg(NetworkReader reader)
    {
      byte num = reader.ReadByte();
      int index = (int) reader.ReadPackedUInt32();
      T obj = this.DeserializeItem(reader);
      switch (num)
      {
        case (byte) 0:
          this.m_Objects.Add(obj);
          break;
        case (byte) 1:
          this.m_Objects.Clear();
          break;
        case (byte) 2:
          this.m_Objects.Insert(index, obj);
          break;
        case (byte) 3:
          this.m_Objects.Remove(obj);
          break;
        case (byte) 4:
          this.m_Objects.RemoveAt(index);
          break;
        case (byte) 5:
        case (byte) 6:
          this.m_Objects[index] = obj;
          break;
      }
      if (this.m_Callback == null)
        return;
      this.m_Callback((SyncList<T>.Operation) num, index);
    }

    internal void AddInternal(T item)
    {
      this.m_Objects.Add(item);
    }

    public void Add(T item)
    {
      this.m_Objects.Add(item);
      this.SendMsg(SyncList<T>.Operation.OP_ADD, 0, item);
    }

    public void Clear()
    {
      this.m_Objects.Clear();
      this.SendMsg(SyncList<T>.Operation.OP_CLEAR, 0);
    }

    public bool Contains(T item)
    {
      return this.m_Objects.Contains(item);
    }

    public void CopyTo(T[] array, int index)
    {
      this.m_Objects.CopyTo(array, index);
    }

    public int IndexOf(T item)
    {
      return this.m_Objects.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      this.SendMsg(SyncList<T>.Operation.OP_INSERT, index, item);
      this.m_Objects.Insert(index, item);
    }

    public bool Remove(T item)
    {
      this.SendMsg(SyncList<T>.Operation.OP_REMOVE, 0, item);
      return this.m_Objects.Remove(item);
    }

    public void RemoveAt(int index)
    {
      this.SendMsg(SyncList<T>.Operation.OP_REMOVEAT, index);
      this.m_Objects.RemoveAt(index);
    }

    public void Dirty(int index)
    {
      this.SendMsg(SyncList<T>.Operation.OP_DIRTY, index, this.m_Objects[index]);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return (IEnumerator<T>) this.m_Objects.GetEnumerator();
    }

    /// <summary>
    /// 
    /// <para>
    /// The types of operations that can occur for SyncLists.
    /// </para>
    /// 
    /// </summary>
    public enum Operation
    {
      OP_ADD,
      OP_CLEAR,
      OP_INSERT,
      OP_REMOVE,
      OP_REMOVEAT,
      OP_SET,
      OP_DIRTY,
    }

    public delegate void SyncListChanged(SyncList<T>.Operation op, int itemIndex);
  }
}
