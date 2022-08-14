using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wav2s3m.Structs
{
    internal class Pattern
    {
        // Max 64 rows
        public List<Row> rows;
        private short length;

        public Pattern(BinaryReader binaryReader)
        {
            long startindex = binaryReader.BaseStream.Position;
            this.length = binaryReader.ReadInt16();
            this.rows = new List<Row>();
            while (binaryReader.BaseStream.Position - startindex < this.length)
            {
                this.rows.Add(new Row(binaryReader));
            }
        }

        public List<byte> Pack()
        {
            List<byte> result = new List<byte>();
            foreach (Row row in this.rows)
            {
                result.AddRange(row.Pack());
            }
            return result;
        }

        public void Write(BinaryWriter bw)
        {
            byte[] data = this.Pack().ToArray();
            bw.Write((ushort)(data.Length + 2));
            bw.Write(data);
        }
    }

    internal class Row
    {
        public List<Command> commands;

        public Row(BinaryReader binaryReader)
        {
            this.commands = new List<Command>();
            while (true)
            {
                byte command = binaryReader.ReadByte();
                if (command == 0) return;
                binaryReader.BaseStream.Position -= 1;
                this.commands.Add(new Command(binaryReader));
            }
        }

        public List<byte> Pack()
        {
            List<byte> result = new List<byte>();
            foreach (Command command in this.commands)
            {
                result.AddRange(command.Pack());
            }
            result.Add(0);
            return result;
        }
    }

    internal class Command
    {
        public byte what;
        public byte? note;
        public byte? instrument;
        public byte? volume;
        public byte? command;
        public byte? info;

        public byte Channel
        {
            get => (byte)(what & 0b00011111);
            set => what = (byte)((value & 0b00011111) | (what & 0b11100000));
        }

        public Command(BinaryReader binaryReader)
        {
            what = binaryReader.ReadByte();
            if ((what & 0b00100000) != 0)
            {
                note = binaryReader.ReadByte();
                instrument = binaryReader.ReadByte();
            }
            if ((what & 0b01000000) != 0)
            {
                volume = binaryReader.ReadByte();
            }
            if ((what & 0b10000000) != 0)
            {
                command = binaryReader.ReadByte();
                info = binaryReader.ReadByte();
            }
        }

        public List<byte> Pack()
        {
            List<byte> result = new List<byte> { what, };
            if ((what & 0b00100000) != 0 && note != null && instrument != null)
            {
                result.Add(note.Value);
                result.Add(instrument.Value);
            }
            if ((what & 0b01000000) != 0 && volume != null)
            {
                result.Add(volume.Value);
            }
            if ((what & 0b10000000) != 0 && command != null && info != null)
            {
                result.Add(command.Value);
                result.Add(info.Value);
            }
            return result;
        }
    }
}
