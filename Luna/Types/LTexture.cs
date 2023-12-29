using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;

namespace Luna.Types {
    class LTexture {
        public Int32 Scaled;
        public Int32 Mipmaps;
        public Int32 Offset;
        public Int32 Length;
        public long Base;
        public Bitmap BitmapData;
        public Int32 TextureWidth; //4 additional fields in newer data.wins
        public Int32 TextureHeight;
        public Int32 GroupIndex;


        public LTexture(BinaryReader _reader) {
            this.Scaled = _reader.ReadInt32();
            this.Mipmaps = _reader.ReadInt32();
            this.Length = _reader.ReadInt32(); //length is now where the offset used to be, offset is now found by just adding all the lengths?
            this.Base = _reader.BaseStream.Position;
            this.TextureWidth = _reader.ReadInt32();
            this.TextureHeight = _reader.ReadInt32();
            this.Offset = _reader.ReadInt32(); //?????

        }

        public void LoadImage(BinaryReader _reader)
        {
            _reader.BaseStream.Seek(this.Offset, SeekOrigin.Begin);
            byte[] data = _reader.ReadBytes(this.Length);
            try
            {
                BitmapData = new Bitmap(new MemoryStream(data));
            }
            catch { //there are types, we can only parse png rn
                
            }
        
        }
    }
}
