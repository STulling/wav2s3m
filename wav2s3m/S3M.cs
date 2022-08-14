using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wav2s3m.Structs;

namespace wav2s3m
{
    public class S3M
    {
        S3MHeader header;
        List<Instrument> instruments;
        List<Pattern> patterns;
        List<byte> order;
        public S3M(string filename)
        {
            FileStream fs = File.Open(filename, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            this.header = utils.ReadType<S3MHeader>(br);
            this.order = br.ReadBytes(this.header.orderCount).ToList();

            this.instruments = new List<Instrument>();
            for (int i = 0; i < this.header.instrumentCount; i++)
            {
                ushort instrumentOffset = br.ReadUInt16();
                instrumentOffset *= 16;
                long remember = br.BaseStream.Position;
                br.BaseStream.Position = instrumentOffset;
                InstrumentType type = (InstrumentType)br.PeekByte();
                if (type == InstrumentType.PCM || type == InstrumentType.Message)
                {
                    Instrument instrument = new PCM(br);
                    this.instruments.Add(instrument);
                }
                else if (type == InstrumentType.Adlib_Melody ||
                         type == InstrumentType.Adlib_Percussive_Bass ||
                         type == InstrumentType.Adlib_Percussive_Snare ||
                         type == InstrumentType.Adlib_Percussive_TomTom ||
                         type == InstrumentType.Adlib_Percussive_Cymbal ||
                         type == InstrumentType.Adlib_Percussive_HiHat)
                {
                    Instrument instrument = new Adlib(br);
                    this.instruments.Add(instrument);
                }
                else
                {
                    throw new Exception("Unknown instrument type");
                }
                br.BaseStream.Position = remember;
            }

            this.patterns = new List<Pattern>();
            for (int i = 0; i < this.header.patternPtrCount; i++)
            {
                long patternOffset = br.ReadUInt16() * 16;
                long remember = br.BaseStream.Position;
                br.BaseStream.Position = patternOffset;
                patterns.Add(new Pattern(br));
                br.BaseStream.Position = remember;
            }
            br.Close();
        }

        public void save(string filename)
        {
            FileStream fs = File.Open(filename, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(utils.StructToBytes(this.header));
            bw.Write(this.order.ToArray());
            // Reserve some space for instruments
            long instrumentOffset = bw.BaseStream.Position;
            ushort[] instrumentBuffer = new ushort[this.instruments.Count];
            foreach (ushort instrumentptr in instrumentBuffer)
            {
                bw.Write(instrumentptr);
            }
            // Reserve some space for patterns
            long patternOffset = bw.BaseStream.Position;
            ushort[] patternBuffer = new ushort[this.patterns.Count];
            foreach (ushort patternptr in patternBuffer)
            {
                bw.Write(patternptr);
            }

            Dictionary<long, byte[]> sampleLookup = new Dictionary<long, byte[]>();
            for (int i = 0; i < this.instruments.Count; i++)
            {
                instrumentBuffer[i] = (ushort)utils.Align(bw);
                if (this.instruments[i].type == InstrumentType.PCM)
                {
                    sampleLookup.Add(bw.BaseStream.Position + 13, ((PCM)this.instruments[i]).sample);
                }
                this.instruments[i].Write(bw);
            }
            for (int i = 0; i < this.patterns.Count; i++)
            {
                patternBuffer[i] = (ushort)utils.Align(bw);
                this.patterns[i].Write(bw);
            }

            // fix instrument and pattern ptrs
            long currPos = bw.BaseStream.Position;
            bw.BaseStream.Position = instrumentOffset;
            foreach (ushort instrumentptr in instrumentBuffer)
            {
                bw.Write(instrumentptr);
            }
            bw.BaseStream.Position = patternOffset;
            foreach (ushort patternptr in patternBuffer)
            {
                bw.Write(patternptr);
            }
            bw.BaseStream.Position = currPos;

            // Insert samples and fix positions
            foreach (KeyValuePair<long, byte[]> kvp in sampleLookup)
            {
                uint position = (uint)utils.Align(bw);
                bw.Write(kvp.Value);
                // write position in the key
                long remember = bw.BaseStream.Position;
                bw.BaseStream.Position = kvp.Key;
                bw.WriteUInt24(position);
                bw.BaseStream.Position = remember;
            }
            bw.Flush();
            bw.Close();
        }
    }
}
