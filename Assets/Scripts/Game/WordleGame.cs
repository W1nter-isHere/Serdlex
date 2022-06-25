using System;
using System.IO;

namespace Game
{
    public readonly struct WordleGame
    {
        public static readonly WordleGame Default = new WordleGame("genna", false, 4);
        
        public readonly string Word;
        public readonly bool ValidateWord;
        public readonly int Chances;

        public WordleGame(string word, bool validateWord, int chances)
        {
            if (string.IsNullOrEmpty(word))
            {
                Word = Default.Word;
            }
            else
            {
                Word = word.ToLower().Split(" ")[0];
            }
            
            ValidateWord = validateWord;
            Chances = chances;
        }

        public static object Deserialize(byte[] data)
        {
            using var memoryStream = new MemoryStream(data);
            using var binaryReader = new BinaryReader(memoryStream);
            var str = binaryReader.ReadString();
            var b = binaryReader.ReadBoolean();
            var i = binaryReader.ReadInt32();
            return new WordleGame(str, b, i);
        }

        public static byte[] Serialize(object type)
        {
            var obj = (WordleGame) type;
            
            using var memoryStream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(obj.Word);
            binaryWriter.Write(obj.ValidateWord);
            binaryWriter.Write(obj.Chances);
            binaryWriter.Flush();
            
            return memoryStream.ToArray();
        }
    }
}