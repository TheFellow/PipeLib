using System.Threading.Tasks;

namespace PipeLib.Interfaces
{
    public interface IConnectable
    {
        void Connect();
        void Connect(int timeout);
        Task ConnectAsync();
        Task ConnectAsync(int timeout);
    }
}