using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace wav2s3m.Structs
{
    public enum InstrumentType : byte
    {
        Message = 0,
        PCM = 1,
        Adlib_Melody = 2,
        Adlib_Percussive_Bass = 3,
        Adlib_Percussive_Snare = 4,
        Adlib_Percussive_TomTom = 5,
        Adlib_Percussive_Cymbal = 6,
        Adlib_Percussive_HiHat = 7,
    }

    internal abstract class Instrument
    {
        public InstrumentType type;
        public char[] filename;
        public uint c2spd;
        protected byte[] unused;
        public char[] title;
        public char[] sig;

        public abstract void Write(BinaryWriter bw);
    }

    internal class PCM : Instrument
    {
        public byte[] sample;
        public uint samplePtr;
        public uint sampleLen;
        public uint loopStart;
        public uint loopEnd;
        public byte volume;
        private byte reserved;
        public byte pack;
        public byte flags;

        public PCM(BinaryReader br)
        {
            this.type = (InstrumentType)br.ReadByte();
            this.filename = br.ReadChars(12);
            this.samplePtr = br.ReadUInt24();
            this.sampleLen = br.ReadUInt32();
            this.loopStart = br.ReadUInt32();
            this.loopEnd = br.ReadUInt32();
            this.volume = br.ReadByte();
            this.reserved = br.ReadByte();
            this.pack = br.ReadByte();
            this.flags = br.ReadByte();
            this.c2spd = br.ReadUInt32();
            this.unused = br.ReadBytes(12);
            this.title = br.ReadChars(28);
            this.sig = br.ReadChars(4);

            long tmp = br.BaseStream.Position;
            br.BaseStream.Position = this.samplePtr * 16;
            this.sample = br.ReadBytes((int)this.sampleLen);
            br.BaseStream.Position = tmp;
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write((byte)this.type);
            bw.Write(this.filename);
            bw.WriteUInt24(this.samplePtr);
            bw.Write(this.sampleLen);
            bw.Write(this.loopStart);
            bw.Write(this.loopEnd);
            bw.Write(this.volume);
            bw.Write(this.reserved);
            bw.Write(this.pack);
            bw.Write(this.flags);
            bw.Write(this.c2spd);
            bw.Write(this.unused);
            bw.Write(this.title);
            bw.Write(this.sig);
        }
    }

    internal struct OPL
    {
        public bool tremolo;
        public bool vibrato;
        public bool sustain;
        public bool ksr;
        public byte freq_mult;
        public byte scale;
        public byte output_level;
        public byte attack_rate;
        public byte decay_rate;
        public byte sustain_rate;
        public byte release_rate;
        public byte wave_select;
        public byte? feedback;
        public bool? connection;
    }

    internal class Adlib : Instrument
    {

        private byte[] reserved;
        public byte[] oplValues;
        public byte volume;
        private byte dsk;
        private short reserved2;
        public OPL modulator;
        public OPL carrier;

        public Adlib(BinaryReader br)
        {
            this.type = (InstrumentType)br.ReadByte();
            this.filename = br.ReadChars(12);
            this.reserved = br.ReadBytes(3);

            this.oplValues = br.ReadBytes(12);

            this.volume = br.ReadByte();
            this.dsk = br.ReadByte();
            this.reserved2 = br.ReadInt16();
            this.c2spd = br.ReadUInt32();
            this.unused = br.ReadBytes(12);
            this.title = br.ReadChars(28);
            this.sig = br.ReadChars(4);
        }

        private void readOplValues(BinaryReader br)
        {
            this.modulator = new OPL();
            this.carrier = new OPL();
            byte byte1 = br.ReadByte();
            this.modulator.tremolo = (byte1 & 0b10000000) != 0;
            this.modulator.vibrato = (byte1 & 0b01000000) != 0;
            this.modulator.sustain = (byte1 & 0b00100000) != 0;
            this.modulator.ksr = (byte1 & 0b00010000) != 0;
            this.modulator.freq_mult = (byte)(byte1 & 0b00001111);
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write((byte)this.type);
            bw.Write(this.filename);
            bw.Write(this.reserved);
            bw.Write(this.oplValues);
            bw.Write(this.volume);
            bw.Write(this.dsk);
            bw.Write(this.reserved2);
            bw.Write(this.c2spd);
            bw.Write(this.unused);
            bw.Write(this.title);
            bw.Write(this.sig);
        }
    }
}
