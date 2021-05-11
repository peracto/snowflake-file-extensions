using System.Threading;
using System.Threading.Tasks;

namespace Snowflake.FileStream
{
    public interface IFileTask
    {
        Task<PutResult> Execute(CancellationToken token);
    }

}