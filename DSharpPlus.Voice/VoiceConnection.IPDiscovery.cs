#pragma warning disable IDE0040

using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Voice;

partial class VoiceConnection
{
    private async Task<IPEndPoint> PerformIPDiscoveryAsync(uint ssrc)
    {
        byte[] request = new byte[74];
        Span<byte> requestSpan = request;

        ArrayPoolBufferWriter<byte> receive = new();

        requestSpan.Clear();

        // request type 1, request length 70 after the length field
        BinaryPrimitives.WriteUInt16BigEndian(requestSpan, 1);
        BinaryPrimitives.WriteUInt16BigEndian(requestSpan[2..], 70);
        BinaryPrimitives.WriteUInt32BigEndian(requestSpan[4..], ssrc);

        await this.mediaTransport.SendAsync(request);
        await this.mediaTransport.ReceiveAsync(receive);

        // ensure the packet is well-formed

        if (BinaryPrimitives.ReadUInt16BigEndian(receive.WrittenSpan) != 2 || BinaryPrimitives.ReadUInt16BigEndian(receive.WrittenSpan[2..]) != 70)
        {
            throw new InvalidDataException("The IP discovery response was malformed.");
        }

        if (BinaryPrimitives.ReadUInt32BigEndian(receive.WrittenSpan[4..]) != ssrc)
        {
            throw new InvalidDataException("Reveived IP discovery packet with a different SSRC from ours.");
        }

        // the IP address is fixed-length, length-prefixed and null-terminated for. some reason. find the null-terminator and slice it off
        ReadOnlySpan<byte> addressSpan = receive.WrittenSpan[8..^2];
        int nullTerminator = addressSpan.IndexOf((byte)0);

        if (nullTerminator == -1)
        {
            throw new InvalidDataException("The received IP address was not null-terminated.");
        }

        addressSpan = addressSpan[..nullTerminator];
        IPAddress address = IPAddress.Parse(addressSpan);
        ushort port = BinaryPrimitives.ReadUInt16BigEndian(receive.WrittenSpan[72..]);

        return new(address, port);
    }
}
