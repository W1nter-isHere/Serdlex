﻿using System.Collections.Generic;
using System.IO;

namespace Game
{
    public class WordleGame
    {
        public static WordleGame Default => new WordleGame("genna", 4, false);

        public string Word;
        public int Tries;
        public List<string> Guesses;

        public readonly int CharactersCount;
        public readonly bool ValidateWord;
        public readonly int TotalChances;

        public WordleGame(string word, int totalChances, bool validateWord)
        {
            if (string.IsNullOrEmpty(word))
            {
                Word = Default.Word;
            }
            else
            {
                Word = word.ToLower().Split(" ")[0];
            }
            
            TotalChances = totalChances;
            ValidateWord = validateWord;
            CharactersCount = Word.Length;
            
            Tries = 0;
            Guesses = new List<string>();
        }

        public WordleGame(WordleGame game) : this(game.Word, game.TotalChances, game.ValidateWord)
        {
        }
        
        public void Encrypt()
        {
            Word = Word.Replace("a", "u").Replace("e", "i").Replace("l", "t");
        }

        public WordleGame Decrypt()
        {
            Word = Word.Replace("t", "l").Replace("i", "e").Replace("u", "a");
            return this;
        }

        public static object Deserialize(byte[] data)
        {
            using var memoryStream = new MemoryStream(data);
            using var binaryReader = new BinaryReader(memoryStream);
            var str = binaryReader.ReadString();
            var i = binaryReader.ReadInt32();
            var b = binaryReader.ReadBoolean();
            return new WordleGame(str, i, b);
        }

        public static byte[] Serialize(object type)
        {
            var obj = (WordleGame) type;
            
            using var memoryStream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(obj.Word);
            binaryWriter.Write(obj.TotalChances);
            binaryWriter.Write(obj.ValidateWord);
            binaryWriter.Flush();
            
            return memoryStream.ToArray();
        }

        public static bool operator ==(WordleGame game1, WordleGame game2)
        {
            return game2 != null && game1 != null && game1.Word == game2.Word;
        }

        public static bool operator !=(WordleGame game1, WordleGame game2)
        {
            return !(game1 == game2);
        }
        
        
        protected bool Equals(WordleGame other)
        {
            return Word == other.Word;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WordleGame) obj);
        }

        public override int GetHashCode()
        {
            return (Word != null ? Word.GetHashCode() : 0);
        }
    }
}