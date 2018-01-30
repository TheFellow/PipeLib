using System.Threading.Tasks;

namespace PipeLib.Interfaces

{
    public interface IWriteStringAsync
    {
        Task WriteStringAsync(string str);
    }
}