using System;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    using LipingShare.LCLib.Asn1Processor;

    internal class PKCS7
    {
        private Asn1Node root;
        public Asn1Node data { get; private set; }
        public List<SignerInfo> sinfos { get; private set; }
        public List<X509Cert> certChain { get; private set; }

        private bool validStructure;

        public static PKCS7 Load(byte[] data)
        {
            using (var stm = new System.IO.MemoryStream(data))
            {
                Asn1Parser parser = new Asn1Parser();
                parser.LoadData(stm);
                return new PKCS7(parser.RootNode);
            }
        }

        public PKCS7(Asn1Node node)
        {
            this.root = node;
            CheckStructure();
        }

        public bool Verify(X509Cert cert, DateTime certificateCreationTime)
        {
            if (validStructure)
            {
                bool ok = true;
                foreach (var sinfo in sinfos)
                {
                    X509Cert signCert = null;
                    foreach (var c in certChain)
                    {
                        if (c.SerialNumber == sinfo.IssuerSerialNumber)
                        {
                            signCert = c;
                            break;
                        }
                    }

                    if (signCert != null && signCert.PubKey != null)
                    {
                        ok = ok && signCert.CheckCertTime(certificateCreationTime);

                        if (IsStoreKitSimulatorData())
                        {
                            ok = ok && signCert.PubKey.Verify256(data.GetChildNode(0).Data, sinfo.EncryptedDigest);
                            ok = ok && ValidateStorekitSimulatorCertRoot(cert, signCert);
                        }
                        else
                        {
                            ok = ok && signCert.PubKey.Verify(data.Data, sinfo.EncryptedDigest);
                            ok = ok && ValidateChain(cert, signCert, certificateCreationTime);
                        }
                    }
                }

                return ok && sinfos.Count > 0;
            }

            return false;
        }

        bool IsStoreKitSimulatorData()
        {
            return data.IsIndefiniteLength && data.ChildNodeCount == 1;
        }

        bool ValidateStorekitSimulatorCertRoot(X509Cert root, X509Cert cert)
        {
            return cert.CheckSignature256(root);
        }

        private bool ValidateChain(X509Cert root, X509Cert cert, DateTime certificateCreationTime)
        {
            if (cert.Issuer.Equals(root.Subject))
                return cert.CheckSignature(root);

            /**
             * TODO: improve this logic
             */
            foreach (var c in certChain)
            {
                if (c != cert && c.Subject.Equals(cert.Issuer) && c.CheckCertTime(certificateCreationTime))
                {
                    if (c.Issuer.Equals(root.Subject) && c.SerialNumber == root.SerialNumber)
                        return c.CheckSignature(root);
                    else
                    {
                        // cert was issued by c
                        if (cert.CheckSignature(c))
                            return ValidateChain(root, c, certificateCreationTime);
                    }
                }
            }

            return false;
        }

        private void CheckStructure()
        {
            validStructure = false;
            if ((root.Tag & Asn1Tag.TAG_MASK) == Asn1Tag.SEQUENCE &&
                root.ChildNodeCount == 2)
            {
                Asn1Node tt = root.GetChildNode(0);
                if ((tt.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.OBJECT_IDENTIFIER ||
                    tt.GetDataStr(false) != "1.2.840.113549.1.7.2")
                {
                    throw new InvalidPKCS7Data();
                }

                tt = root.GetChildNode(1); // [0]
                if (tt.ChildNodeCount != 1)
                    throw new InvalidPKCS7Data();
                int curChild = 0;

                tt = tt.GetChildNode(curChild++); // Seq
                if (tt.ChildNodeCount < 4 || (tt.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.SEQUENCE)
                    throw new InvalidPKCS7Data();

                Asn1Node tt2 = tt.GetChildNode(0); // version
                if ((tt2.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.INTEGER)
                    throw new InvalidPKCS7Data();

                tt2 = tt.GetChildNode(curChild++); // digest algo
                                                   // TODO: check algo
                if ((tt2.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.SET)
                    throw new InvalidPKCS7Data();

                tt2 = tt.GetChildNode(curChild++); // pkcs7 data
                if ((tt2.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.SEQUENCE && tt2.ChildNodeCount != 2)
                    throw new InvalidPKCS7Data();
                data = tt2.GetChildNode(1).GetChildNode(0);

                if (tt.ChildNodeCount == 5)
                {
                    // cert chain, this is optional
                    certChain = new List<X509Cert>();
                    tt2 = tt.GetChildNode(curChild++);
                    if (tt2.ChildNodeCount == 0)
                        throw new InvalidPKCS7Data();
                    for (int i = 0; i < tt2.ChildNodeCount; i++)
                    {
                        certChain.Add(new X509Cert(tt2.GetChildNode(i)));
                    }
                }

                tt2 = tt.GetChildNode(curChild++); // signer's info
                if ((tt2.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.SET || tt2.ChildNodeCount == 0)
                    throw new InvalidPKCS7Data();

                sinfos = new List<SignerInfo>();
                for (int i = 0; i < tt2.ChildNodeCount; i++)
                {
                    sinfos.Add(new SignerInfo(tt2.GetChildNode(i)));
                }
                validStructure = true;
            }
        }
    }

    internal class SignerInfo
    {
        public int Version { get; private set; }
        public string IssuerSerialNumber { get; private set; }
        public byte[] EncryptedDigest { get; private set; }

        public SignerInfo(Asn1Node n)
        {
            if (n.ChildNodeCount != 5)
                throw new InvalidPKCS7Data();
            Asn1Node tt;

            // version
            tt = n.GetChildNode(0);
            if ((tt.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.INTEGER)
                throw new InvalidPKCS7Data();
            Version = tt.Data[0];
            if (Version != 1 || tt.Data.Length != 1)
                throw new UnsupportedSignerInfoVersion();

            // get the issuer SN
            tt = n.GetChildNode(1);
            if ((tt.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.SEQUENCE || tt.ChildNodeCount != 2)
                throw new InvalidPKCS7Data();
            tt = tt.GetChildNode(1);
            if ((tt.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.INTEGER)
                throw new InvalidPKCS7Data();
            IssuerSerialNumber = Asn1Util.ToHexString(tt.Data);

            // get the data
            tt = n.GetChildNode(4);
            if ((tt.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.OCTET_STRING)
                throw new InvalidPKCS7Data();
            EncryptedDigest = tt.Data;
        }
    }

    /// <summary>
    /// An IAP Security exception indicating some invalid data for PKCS7 checks.
    /// </summary>
    public class InvalidPKCS7Data : IAPSecurityException { }

    /// <summary>
    /// An IAP Security exception indicating unsupported signer information.
    /// </summary>
    public class UnsupportedSignerInfoVersion : IAPSecurityException { }
}
