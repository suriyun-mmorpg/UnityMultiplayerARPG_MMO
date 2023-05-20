using System.Security.Cryptography;

namespace MultiplayerARPG.MMO
{
    using LipingShare.LCLib.Asn1Processor;

    internal class RSAKey
    {
        public RSACryptoServiceProvider rsa { get; private set; }

        public RSAKey(Asn1Node n)
        {
            rsa = ParseNode(n);
        }

        public RSAKey(byte[] data)
        {
            using (var stm = new System.IO.MemoryStream(data))
            {
                Asn1Parser parser = new Asn1Parser();
                parser.LoadData(stm);
                rsa = ParseNode(parser.RootNode);
            }
        }

        /**
         * Public verification of a message
         */
        public bool Verify(byte[] message, byte[] signature)
        {
            var sha1hash = new SHA1Managed();
            var msgHash = sha1hash.ComputeHash(message);

            // The data is already hashed so we don't need to specify a hashing algorithm.
            return rsa.VerifyHash(msgHash, null, signature);
        }

        public bool Verify256(byte[] message, byte[] signature)
        {
            var sha256hash = new SHA256Managed();
            var msgHash = sha256hash.ComputeHash(message);

            // The data is already hashed so we don't need to specify a hashing algorithm.
            return rsa.VerifyHash(msgHash, CryptoConfig.MapNameToOID("SHA256"), signature);
        }

        /**
         * Parses an DER encoded RSA public key:
         * It will only try to get the mod and the exponent
         */
        private RSACryptoServiceProvider ParseNode(Asn1Node n)
        {
            if ((n.Tag & Asn1Tag.TAG_MASK) == Asn1Tag.SEQUENCE &&
                n.ChildNodeCount == 2 &&
                (n.GetChildNode(0).Tag & Asn1Tag.TAG_MASK) == Asn1Tag.SEQUENCE &&
                (n.GetChildNode(0).GetChildNode(0).Tag & Asn1Tag.TAG_MASK) == Asn1Tag.OBJECT_IDENTIFIER &&
                n.GetChildNode(0).GetChildNode(0).GetDataStr(false) == "1.2.840.113549.1.1.1" &&
                (n.GetChildNode(1).Tag & Asn1Tag.TAG_MASK) == Asn1Tag.BIT_STRING)
            {
                var seq = n.GetChildNode(1).GetChildNode(0);
                if (seq.ChildNodeCount == 2)
                {
                    byte[] data = seq.GetChildNode(0).Data;
                    byte[] rawMod = new byte[data.Length - 1];
                    System.Array.Copy(data, 1, rawMod, 0, data.Length - 1);

                    var modulus = System.Convert.ToBase64String(rawMod);
                    var exponent = System.Convert.ToBase64String(seq.GetChildNode(1).Data);
                    var result = new RSACryptoServiceProvider();
                    result.FromXmlString(ToXML(modulus, exponent));

                    return result;
                }
            }
            throw new InvalidRSAData();
        }

        private string ToXML(string modulus, string exponent)
        {
            return "<RSAKeyValue><Modulus>" + modulus + "</Modulus>" +
                "<Exponent>" + exponent + "</Exponent></RSAKeyValue>";
        }
    }

    /// <summary>
    /// An IAP Security exception indicating some invalid data parsing an RSA node.
    /// </summary>
    public class InvalidRSAData : IAPSecurityException { }
}
