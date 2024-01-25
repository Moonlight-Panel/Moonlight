using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Moonlight.Core.Helpers;

// Src: https://codereview.stackexchange.com/questions/176697/net-core-mvc-future-proof-hashing-of-passwords
public static class HashHelper
{
    /// <summary>
    /// The default number of Iterations
    /// </summary>
    private const int DefaultIterations = 10000;

    /// <summary>
    /// Provides Information about a specific Hash Version
    /// </summary>
    private class HashVersion
    {
        public short Version { get; set; }
        public int SaltSize { get; set; }
        public int HashSize { get; set; }
        public KeyDerivationPrf KeyDerivation { get; set; }
    }

    /// <summary>
    /// Holds all possible Hash Versions
    /// </summary>
    private static readonly Dictionary<short, HashVersion> _versions = new Dictionary<short, HashVersion>
    {
        {
            1, new HashVersion
            {
                Version = 1,
                KeyDerivation = KeyDerivationPrf.HMACSHA512,
                HashSize = 256 / 8,
                SaltSize = 128 / 8
            }
        }
    };

    /// <summary>
    /// The default Hash Version, which should be used, if a new Hash is Created
    /// </summary>
    private static HashVersion DefaultVersion => _versions[1];

    /// <summary>
    /// Checks if a given hash uses the latest version
    /// </summary>
    /// <param name="data">The hash</param>
    /// <returns>Is the hash of the latest version?</returns>
    public static bool IsLatestHashVersion(byte[] data)
    {
        var version = BitConverter.ToInt16(data, 0);
        return version == DefaultVersion.Version;
    }

    /// <summary>
    /// Checks if a given hash uses the latest version
    /// </summary>
    /// <param name="data">The hash</param>
    /// <returns>Is the hash of the latest version?</returns>
    public static bool IsLatestHashVersion(string data)
    {
        var dataBytes = Convert.FromBase64String(data);
        return IsLatestHashVersion(dataBytes);
    }

    /// <summary>
    /// Gets a random byte array
    /// </summary>
    /// <param name="length">The length of the byte array</param>
    /// <returns>The random byte array</returns>
    public static byte[] GetRandomBytes(int length)
    {
        var data = new byte[length];
        using (var randomNumberGenerator = RandomNumberGenerator.Create())
        {
            randomNumberGenerator.GetBytes(data);
        }

        return data;
    }

    /// <summary>
    /// Creates a Hash of a clear text
    /// </summary>
    /// <param name="clearText">the clear text</param>
    /// <param name="iterations">the number of iteration the hash alogrythm should run</param>
    /// <returns>the Hash</returns>
    public static byte[] Hash(string clearText, int iterations = DefaultIterations)
    {
        //get current version
        var currentVersion = DefaultVersion;

        //get the byte arrays of the hash and meta information
        var saltBytes = GetRandomBytes(currentVersion.SaltSize);
        var versionBytes = BitConverter.GetBytes(currentVersion.Version);
        var iterationBytes = BitConverter.GetBytes(iterations);
        var hashBytes = KeyDerivation.Pbkdf2(clearText, saltBytes, currentVersion.KeyDerivation, iterations,
            currentVersion.HashSize);

        //calculate the indexes for the combined hash
        var indexVersion = 0;
        var indexIteration = indexVersion + 2;
        var indexSalt = indexIteration + 4;
        var indexHash = indexSalt + currentVersion.SaltSize;

        //combine all data to one result hash
        var resultBytes = new byte[2 + 4 + currentVersion.SaltSize + currentVersion.HashSize];
        Array.Copy(versionBytes, 0, resultBytes, indexVersion, 2);
        Array.Copy(iterationBytes, 0, resultBytes, indexIteration, 4);
        Array.Copy(saltBytes, 0, resultBytes, indexSalt, currentVersion.SaltSize);
        Array.Copy(hashBytes, 0, resultBytes, indexHash, currentVersion.HashSize);
        return resultBytes;
    }

    /// <summary>
    /// Creates a Hash of a clear text and convert it to a Base64 String representation
    /// </summary>
    /// <param name="clearText">the clear text</param>
    /// <param name="iterations">the number of iteration the hash alogrythm should run</param>
    /// <returns>the Hash</returns>
    public static string HashToString(string clearText, int iterations = DefaultIterations)
    {
        var data = Hash(clearText, iterations);
        return Convert.ToBase64String(data);
    }

    /// <summary>
    /// Verifies a given clear Text against a hash
    /// </summary>
    /// <param name="clearText">The clear text</param>
    /// <param name="data">The hash</param>
    /// <returns>Is the hash equal to the clear text?</returns>
    public static bool Verify(string clearText, byte[] data)
    {
        //Get the current version and number of iterations
        var currentVersion = _versions[BitConverter.ToInt16(data, 0)];
        var iteration = BitConverter.ToInt32(data, 2);

        //Create the byte arrays for the salt and hash
        var saltBytes = new byte[currentVersion.SaltSize];
        var hashBytes = new byte[currentVersion.HashSize];

        //Calculate the indexes of the salt and the hash
        var indexSalt = 2 + 4; // Int16 (Version) and Int32 (Iteration)
        var indexHash = indexSalt + currentVersion.SaltSize;

        //Fill the byte arrays with salt and hash
        Array.Copy(data, indexSalt, saltBytes, 0, currentVersion.SaltSize);
        Array.Copy(data, indexHash, hashBytes, 0, currentVersion.HashSize);

        //Hash the current clearText with the parameters given via the data
        var verificationHashBytes = KeyDerivation.Pbkdf2(clearText, saltBytes, currentVersion.KeyDerivation, iteration,
            currentVersion.HashSize);

        //Check if generated hashes are equal
        return hashBytes.SequenceEqual(verificationHashBytes);
    }

    /// <summary>
    /// Verifies a given clear Text against a hash
    /// </summary>
    /// <param name="clearText">The clear text</param>
    /// <param name="data">The hash</param>
    /// <returns>Is the hash equal to the clear text?</returns>
    public static bool Verify(string clearText, string data)
    {
        var dataBytes = Convert.FromBase64String(data);
        return Verify(clearText, dataBytes);
    }
}