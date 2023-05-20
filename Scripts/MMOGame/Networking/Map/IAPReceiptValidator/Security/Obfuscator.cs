using System;
using System.Linq;

namespace MultiplayerARPG.MMO
{
    /// <summary>
    /// This class will deobfuscate the tangled signature used for client-side receipt validation obfuscation.
    /// </summary>
    public static class Obfuscator
    {
        /// <summary>
        /// Deobfucscates tangle data.
        /// </summary>
        /// <param name="data"> The Apple or GooglePlay public key data to be deobfuscated. </param>
        /// <param name="order"> The array of the order of the data slices used to obfuscate the data when the tangle files were originally generated. </param>
        /// <param name="key"> The encryption key to deobfuscate the tangled data at runtime, previously generated with the tangle file. </param>
        /// <returns>The deobfucated public key</returns>
        public static byte[] DeObfuscate(byte[] data, int[] order, int key)
        {
            var res = new byte[data.Length];
            int slices = data.Length / 20 + 1;
            bool hasRemainder = data.Length % 20 != 0;

            Array.Copy(data, res, data.Length);
            for (int i = order.Length - 1; i >= 0; i--)
            {
                var j = order[i];
                int sliceSize = (hasRemainder && j == slices - 1) ? (data.Length % 20) : 20;
                var tmp = res.Skip(i * 20).Take(sliceSize).ToArray(); // tmp = res[i*20 .. slice]
                Array.Copy(res, j * 20, res, i * 20, sliceSize);      // res[i] = res[j*20 .. slice]
                Array.Copy(tmp, 0, res, j * 20, sliceSize);           // res[j] = tmp
            }
            return res.Select(x => (byte)(x ^ key)).ToArray();
        }
    }
}
