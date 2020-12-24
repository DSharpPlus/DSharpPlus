using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DSharpPlus.VideoNext.Codec
{

    public struct RtpHeader
    {
        
        /// <summary>
        /// Constructs a RtpHeader based on the specified data.
        /// </summary>
        /// <param name="data"></param>
        public RtpHeader(byte[] data)
        {
            Bitfield = BitConverter.ToUInt16(data);
            _csrc = new uint[(Bitfield & 0b0000_1111)];
            Sequence = BitConverter.ToUInt16(data, 2);
            Timestamp = BitConverter.ToUInt32(data, 3);
            SSRC = BitConverter.ToUInt32(data, 5);
            for (int i = 0; i < _csrc.Length; i++)
            {
                _csrc[i] = data[7 + i];
            }

            //If an extension is present.
            if ((Bitfield & 0b0001_0000) == 0b0001_0000)
            {
                var extensionOffset = 7 + _csrc.Length;

                var profileDef = data[extensionOffset];
                var extensionLength = BitConverter.ToUInt16(data, extensionOffset);
                _extensionData = new ArraySegment<byte>(data, extensionOffset + 2, extensionLength).ToArray();
            }
            else
            {
                _extensionData = null;
            }
        }

        public byte[] Bytes
        {
            get
            {
                //Ensure the csrc header matches the data correctly.
                CSRC = _csrc;
                
                var arr = new List<byte>();
                arr.AddRange(BitConverter.GetBytes(Bitfield).Reverse());
                arr.AddRange(BitConverter.GetBytes(Timestamp).Reverse());
                arr.AddRange(BitConverter.GetBytes(SSRC).Reverse());
                //Todo: add CSRC.
                if (CSRC != null)
                {
                    foreach (var item in CSRC)
                    {
                        arr.AddRange(BitConverter.GetBytes(item).Reverse());
                    }
                }
                if (HasExtension && ExtensionData?.Length > 0)
                {
                    arr.Add((byte) ExtensionData.Length);
                    arr.AddRange(ExtensionData);
                }
                return arr.ToArray();
            }
        }

        public byte Size => (byte) (12 + (CSRC?.Length * 2 ?? 0));

        private ushort Bitfield;

        /// <summary>
        /// The rtp version being used.
        /// </summary>
        public byte Version
        {
            get
            {
                return (byte) ((Bitfield & 0b1100_0000_0000_0000) >> 14);
            }
            set
            {
                Bitfield |= (ushort) (value << 14);
            }
        }

        /// <summary>
        /// Whether or not the packet contains padding octets at the end.
        /// this padding is not part of the payload.
        /// </summary>
        public bool Padding
        {
            get
            {
                return (Bitfield & 0b0010_0000_0000_0000) != 0;
            }
            set
            {
                Bitfield |= (ushort) ((value ? 1: 0) << 13);
            }
        }

        /// <summary>
        /// Whether or not this header is followed by a secondary extension header.
        /// </summary>
        public bool HasExtension
        {
            get
            {
                return (Bitfield & 0b0001_0000) == 0b0001_0000;
            }
            set
            {
                Bitfield |= (ushort) ((value ? 1 : 0) << 12);
            }
        }

        /// <summary>
        /// the data associated with the extension.
        /// </summary>
        public byte[] ExtensionData
        {
            get => this._extensionData;
            set
            {
                if (value == null)
                    HasExtension = false; 
                else
                    HasExtension = true;
                
                _extensionData = value;
            }
        }

        private byte[] _extensionData;

        /// <summary>
        /// A marker that indicates frame boundaries in the packet stream.
        /// </summary>
        public bool Marker
        {
            get
            {
                return (Bitfield & 0b1000_0000) == 0b1000_0000;
            }
            set
            {
                Bitfield |= (byte) ((value ? 1 : 0) << 7);
            }
        }

        /// <summary>
        /// The type of payload.
        /// </summary>
        public byte PayloadType
        {
            get
            {
                return (byte) (Bitfield & 0b0111_1111);
            }
            set
            {

                Bitfield |= (value);
            }
        }
        
        /// <summary>
        /// The sequence number indicating what packet this is.
        /// Can be used to indicate packet loss or for re-ordering.
        /// </summary>
        public UInt16 Sequence;

        /// <summary>
        /// A timestamp for the packet.
        /// </summary>
        public UInt32 Timestamp;

        /// <summary>
        /// The SSRC used to identify the synchronization source.
        /// </summary>
        public UInt32 SSRC;

        public uint[] CSRC
        {
            get
            {
                return _csrc;
            }
            set
            {
                if(value == null)
                    Bitfield |= (ushort) (0b0000_1111) << 8;
                else
                {

                    Bitfield |= (ushort) ((value.Length & 0b0000_1111) << 8 );
                    _csrc = value;
                }

            }
        }
        
        private uint[] _csrc;

    }
    
}