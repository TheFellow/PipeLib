using System.Threading.Tasks;

namespace PipeLib.Interfaces
{
    public interface IWriteBytesAsync
    {
        Task WriteBytesAsync(byte[] bytes);
    }
}