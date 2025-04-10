using System;
using System.Collections.Generic;
using System.Text;

namespace AES2
{
    public class AES256CBC
    {
        // Conversion utilities
        public static uint[] StringToUintArray(string text)
        {
            // Convert string to byte array
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);

            // PKCS7 padding
            int blockSize = 16; // 128 bit = 16 byte
            int padding = blockSize - (bytes.Length % blockSize);
            byte[] paddedBytes = new byte[bytes.Length + padding];
            Array.Copy(bytes, paddedBytes, bytes.Length);
            for (int i = bytes.Length; i < paddedBytes.Length; i++)
            {
                paddedBytes[i] = (byte)padding;
            }

            // Convert byte array to uint array
            uint[] result = new uint[paddedBytes.Length / 4];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (uint)(paddedBytes[i * 4] << 24 |
                                  paddedBytes[i * 4 + 1] << 16 |
                                  paddedBytes[i * 4 + 2] << 8 |
                                  paddedBytes[i * 4 + 3]);
            }
            return result;
        }

        public static string UintArrayToString(uint[] data)
        {
            try
            {
                // Convert uint array to byte array
                byte[] bytes = new byte[data.Length * 4];
                for (int i = 0; i < data.Length; i++)
                {
                    bytes[i * 4] = (byte)(data[i] >> 24);
                    bytes[i * 4 + 1] = (byte)(data[i] >> 16);
                    bytes[i * 4 + 2] = (byte)(data[i] >> 8);
                    bytes[i * 4 + 3] = (byte)(data[i]);
                }

                // Handle PKCS7 padding
                int lastByte = bytes[bytes.Length - 1];
                if (lastByte > 0 && lastByte <= 16) // Valid padding value
                {
                    // Verify all padding bytes have same value
                    bool validPadding = true;
                    for (int i = bytes.Length - lastByte; i < bytes.Length; i++)
                    {
                        if (bytes[i] != lastByte)
                        {
                            validPadding = false;
                            break;
                        }
                    }

                    if (validPadding)
                    {
                        byte[] unpaddedBytes = new byte[bytes.Length - lastByte];
                        Array.Copy(bytes, unpaddedBytes, unpaddedBytes.Length);
                        return System.Text.Encoding.UTF8.GetString(unpaddedBytes);
                    }
                }

                // If padding is invalid, return the complete data
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decoding: {ex.Message}");
                return string.Empty;
            }
        }

        // Key and IV generation
        public static uint[] GenerateRandomKey()
        {
            uint[] key = new uint[8]; // 256-bit = 8 x 32-bit words
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[32];
                rng.GetBytes(bytes);
                for (int i = 0; i < 8; i++)
                {
                    key[i] = BitConverter.ToUInt32(bytes, i * 4);
                }
            }
            return key;
        }

        public static uint[] GenerateIV()
        {
            uint[] iv = new uint[4]; // 128-bit IV
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[16];
                rng.GetBytes(bytes);
                for (int i = 0; i < 4; i++)
                {
                    iv[i] = BitConverter.ToUInt32(bytes, i * 4);
                }
            }
            return iv;
        }

        // Block operations
        public static uint[] XorBlocks(uint[] block1, uint[] block2)
        {
            uint[] result = new uint[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = block1[i] ^ block2[i];
            }
            return result;
        }

        public static uint[][] SplitIntoBlocks(string message)
        {
            // Convert message to bytes
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

            // Calculate required blocks (16 bytes per block)
            int blockCount = (messageBytes.Length + 15) / 16;
            uint[][] blocks = new uint[blockCount][];

            // Process each block
            for (int i = 0; i < blockCount; i++)
            {
                byte[] blockBytes = new byte[16];
                int bytesToCopy = Math.Min(16, messageBytes.Length - i * 16);
                Array.Copy(messageBytes, i * 16, blockBytes, 0, bytesToCopy);

                // Add PKCS7 padding if needed
                if (bytesToCopy < 16)
                {
                    byte paddingValue = (byte)(16 - bytesToCopy);
                    for (int j = bytesToCopy; j < 16; j++)
                    {
                        blockBytes[j] = paddingValue;
                    }
                }

                // Convert to uint array
                blocks[i] = new uint[4];
                for (int j = 0; j < 4; j++)
                {
                    blocks[i][j] = BitConverter.ToUInt32(blockBytes, j * 4);
                }
            }
            return blocks;
        }

        // CBC mode encryption
        public static uint[][] EncryptCBC(string message, uint[] key, uint[] iv)
        {
            uint[][] blocks = SplitIntoBlocks(message);
            uint[][] encryptedBlocks = new uint[blocks.Length + 1][];

            // Store IV as first block
            encryptedBlocks[0] = iv;

            // Previous block starts as IV
            uint[] previousBlock = iv;

            // Encrypt each block
            for (int i = 0; i < blocks.Length; i++)
            {
                uint[] xoredBlock = XorBlocks(blocks[i], previousBlock);
                uint[] encryptedBlock = AES256Core.MahoaAES(xoredBlock, key);
                encryptedBlocks[i + 1] = encryptedBlock;
                previousBlock = encryptedBlock;
            }

            return encryptedBlocks;
        }

        // CBC mode decryption
        public static string DecryptCBC(uint[][] encryptedBlocks, uint[] key)
        {
            if (encryptedBlocks.Length < 2)
                throw new ArgumentException("Invalid encrypted data");

            // First block is IV
            uint[] iv = encryptedBlocks[0];
            List<byte> decryptedBytes = new List<byte>();

            // Previous block starts as IV
            uint[] previousBlock = iv;

            // Decrypt each block
            for (int i = 1; i < encryptedBlocks.Length; i++)
            {
                uint[] decryptedBlock = AES256Core.GiaimaAES(encryptedBlocks[i], key);
                uint[] xoredBlock = XorBlocks(decryptedBlock, previousBlock);

                // Convert block to bytes
                byte[] blockBytes = new byte[16];
                for (int j = 0; j < 4; j++)
                {
                    BitConverter.GetBytes(xoredBlock[j]).CopyTo(blockBytes, j * 4);
                }

                // Add to result
                decryptedBytes.AddRange(blockBytes);
                previousBlock = encryptedBlocks[i];
            }

            // Remove PKCS7 padding
            int paddingLength = decryptedBytes[decryptedBytes.Count - 1];
            if (paddingLength > 0 && paddingLength <= 16)
            {
                decryptedBytes.RemoveRange(decryptedBytes.Count - paddingLength, paddingLength);
            }

            return Encoding.UTF8.GetString(decryptedBytes.ToArray());
        }
    }
}
