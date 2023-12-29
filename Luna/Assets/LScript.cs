using System.IO;

namespace Luna.Assets {
    class LScript {
        public string Name;
        public long Index;
        public bool IsConstructor = false;

        public LScript(Game _game, BinaryReader _reader) {
            Name = _game.GetString(_reader.ReadInt32());
            Index = _reader.ReadInt32();

            if(Index < -1)
            {
                IsConstructor = true;
                Index = (int)((uint)Index & 2147483647u); //thanks undertale mod tool
            }
        }
    }
}
