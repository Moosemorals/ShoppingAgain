using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

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

            for (int i =0; i < HashLength; i += 1)
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
