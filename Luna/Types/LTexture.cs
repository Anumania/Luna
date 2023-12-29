using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;
using OpenTK.Graphics.ES11;
using UndertaleModLib.Util;

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

        //thanks undertale mod tool
        /// <summary>
        /// Header used for PNG files.
        /// </summary>
        public static readonly byte[] PNGHeader = { 137, 80, 78, 71, 13, 10, 26, 10 };

        /// <summary>
        /// Header used for GameMaker QOI + BZ2 files.
        /// </summary>
        public static readonly byte[] QOIAndBZip2Header = { 50, 122, 111, 113 };

        /// <summary>
        /// Header used for GameMaker QOI files.
        /// </summary>
        public static readonly byte[] QOIHeader = { 102, 105, 111, 113 };


        public LTexture(BinaryReader _reader) {
            this.Scaled = _reader.ReadInt32();
            this.Mipmaps = _reader.ReadInt32();
            this.Length = _reader.ReadInt32(); //length is now where the offset used to be, offset is now found by just adding all the lengths?
            this.Base = _reader.BaseStream.Position;
            this.TextureWidth = _reader.ReadInt32();
            this.TextureHeight = _reader.ReadInt32();
            this.GroupIndex = _reader.ReadInt32();
            this.Offset = _reader.ReadInt32(); //?????

        }

        public void LoadImage(BinaryReader _reader)
        {
            _reader.BaseStream.Seek(this.Offset, SeekOrigin.Begin);
            byte[] data = _reader.ReadBytes(this.Length);
            _reader.BaseStream.Seek(this.Offset, SeekOrigin.Begin);
            byte[] header = _reader.ReadBytes(8);

            if (header.SequenceEqual(PNGHeader)) //if its a png
            {
                BitmapData = new Bitmap(new MemoryStream(data));
                return;
            }
            
            if(header.Take(4).SequenceEqual(QOIAndBZip2Header))
            {
                //thank you undertale mod tool
                _reader.BaseStream.Seek(this.Offset, SeekOrigin.Begin);
                _reader.BaseStream.Position += 12; //theres a header with some shit i dont care about 

                MemoryStream stream2 = new MemoryStream();
                stream2.Seek(0, SeekOrigin.Begin); //make sure the stream is set to the beginning cuz utmt does this

                BZip2.Decompress(_reader.BaseStream, stream2, false);
                stream2.Seek(0, SeekOrigin.Begin);
                Bitmap bmp = QoiConverter.GetImageFromStream(stream2);
                BitmapData = bmp;
                return;
            }

            throw new Exception("shit dont work");
        }
    }
}
