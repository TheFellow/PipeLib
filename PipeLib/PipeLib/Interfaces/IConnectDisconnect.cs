using System;

namespace PipeLib.Interfaces
{
    public interface IConnectDisconnect
    {
        Action OnConnect { get; set; }
        Action OnDisconnect { get; set; }
    }
}