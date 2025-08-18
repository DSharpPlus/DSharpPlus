using System.Buffers;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Transport;
public interface IMediaTransportService
{
    public Task SendAsync(ReadOnlySequence<byte> buffer);
    public Task ReceiveAsync(IBufferWriter<byte> bufferWriter);
}
