using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;


namespace ReliableDownloader
{
    public enum INTEGRITY
    {
        NOT_VERIFIED,
        VERIFIED,
        COMPROMISED
    }

    public static class IntegrityVerifier
    {
        // Added on 14 Sept 2021
        // MD5 comparison
        public static INTEGRITY CheckMD5(byte[] headerHash, string fileName) 
        {
            if (headerHash?.Length > 0)
            {
                using var fileStream = File.OpenRead(fileName);
                using var md5 = MD5.Create();
                var actualHash = md5.ComputeHash(fileStream);

                Console.WriteLine("\n\nHeader hash: {0}", Convert.ToBase64String(headerHash));
                Console.WriteLine(    "Actual hash: {0}", Convert.ToBase64String(actualHash));

                if (headerHash.SequenceEqual(actualHash))
                {
                    return INTEGRITY.VERIFIED;
                }
                else
                {
                    return INTEGRITY.COMPROMISED;
                }
            }
            else
            {
                return INTEGRITY.NOT_VERIFIED;
            }
        }
    }
}
