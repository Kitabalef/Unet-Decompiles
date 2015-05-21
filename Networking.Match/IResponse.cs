// Decompiled with JetBrains decompiler
// Type: UnityEngine.Networking.Match.IResponse
// Assembly: UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B9965B63-74A2-480C-BCFC-887FBCF7E9A7
// Assembly location: C:\Program Files\Unity 5.1.0b6\Editor\Data\Managed\UnityEngine.dll

namespace UnityEngine.Networking.Match
{
  public interface IResponse
  {
    void SetSuccess();

    void SetFailure(string info);
  }
}
