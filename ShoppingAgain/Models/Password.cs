using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ShoppingAgain.Models
{
    public class Password
    {
        public static readonly int DefaultRounds = 10000;
        public static readonly int SaltLength = 16;
        public static readonly int HashLength = 20;

        [Key]
        public Guid ID { get; set; }

        [Required]
        public string Hash { get; set; }

        [Required]
        public int Rounds { get; set; }

        [Required]
        public User User { get; set; }
        public Guid UserID { get; set; }

        // Based on https://stackoverflow.com/a/3745973/195833
        public static async Task<string> Generate(int wordCount)
        {
            StreamReader reader = File.OpenText("Static/wordlist.txt");

            string[] words = new string[wordCount];
            int lineCount = 0;
            Random rng = new Random();
            string word;
            while ((word = await reader.ReadLineAsync()) != null)
            {
                for (int i = 0; i < wordCount; i += 1)
                {
                    if (rng.Next(lineCount) == 0)
                    {
                        words[i] = word;
                    }

                }
                lineCount += 1;
            }
            return string.Join(" ", words);
        }

        public static Password Generate(string pw)
        {
            byte[] salt = new byte[SaltLength];
            new RNGCryptoServiceProvider().GetBytes(salt);

            var pbkdf2 = new Rfc2898DeriveBytes(pw, salt, DefaultRounds);
            byte[] hash = pbkdf2.GetBytes(HashLength);

            byte[] hashBytes = new byte[SaltLength + HashLength];
            Array.Copy(salt, 0, hashBytes, 0, SaltLength);
            Array.Copy(hash, 0, hashBytes, SaltLength, HashLength);

            return new Password
            {
                Hash = Convert.ToBase64String(hashBytes),
                Rounds = DefaultRounds,
            };
        }

        public bool Validate(string pw)
        {
            byte[] hashBytes = Convert.FromBase64String(Hash);
            byte[] salt = new byte[SaltLength];
            Array.Copy(hashBytes, 0, salt, 0, SaltLength);
            var pdkdf2 = new Rfc2898DeriveBytes(pw, salt, Rounds);
            byte[] hash = pdkdf2.GetBytes(HashLength);

            bool valid = true;

            for (int i = 0; i < HashLength; i += 1)
            {
                if (hashBytes[SaltLength + i] != hash[i])
                {
                    valid = false;
                }
            }

            return valid;
        }

    }
}
