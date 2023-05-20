using System;

namespace MultiplayerARPG.MMO
{
    using LipingShare.LCLib.Asn1Processor;

    internal class DistinguishedName
    {
        public string Country { get; set; }
        public string Organization { get; set; }
        public string OrganizationalUnit { get; set; }
        public string Dnq { get; set; }
        public string State { get; set; }
        public string CommonName { get; set; }
        public string SerialNumber { get; set; }

        public DistinguishedName(Asn1Node n)
        {
            /* Name:
             * SET
             *   SEQ (attr)
             *     Object Identifier
             *     Printable String || UTF8String
             */
            if (n.MaskedTag == Asn1Tag.SEQUENCE)
            {
                for (int i = 0; i < n.ChildNodeCount; i++)
                {
                    Asn1Node tt = n.GetChildNode(i);
                    if (tt.MaskedTag != Asn1Tag.SET || tt.ChildNodeCount != 1)
                        throw new InvalidX509Data();

                    tt = tt.GetChildNode(0);
                    if (tt.MaskedTag != Asn1Tag.SEQUENCE || tt.ChildNodeCount != 2)
                        throw new InvalidX509Data();

                    Asn1Node oi = tt.GetChildNode(0);
                    Asn1Node txt = tt.GetChildNode(1);

                    if (oi.MaskedTag != Asn1Tag.OBJECT_IDENTIFIER ||
                        !(
                            (txt.MaskedTag == Asn1Tag.PRINTABLE_STRING) ||
                            (txt.MaskedTag == Asn1Tag.UTF8_STRING) ||
                            (txt.MaskedTag == Asn1Tag.IA5_STRING)))
                    {
                        throw new InvalidX509Data();
                    }
                    var xoid = new LipingShare.LCLib.Asn1Processor.Oid();
                    string oiName = xoid.Decode(oi.Data);
                    var enc = new System.Text.UTF8Encoding();

                    switch (oiName)
                    {
                        case "2.5.4.6": // countryName
                            Country = enc.GetString(txt.Data);
                            break;
                        case "2.5.4.10": // organizationName
                            Organization = enc.GetString(txt.Data);
                            break;
                        case "2.5.4.11": // organizationalUnit
                            OrganizationalUnit = enc.GetString(txt.Data);
                            break;
                        case "2.5.4.3": // commonName
                            CommonName = enc.GetString(txt.Data);
                            break;
                        case "2.5.4.5": // serial number
                            SerialNumber = Asn1Util.ToHexString(txt.Data);
                            break;
                        case "2.5.4.46": // dnq
                            Dnq = enc.GetString(txt.Data);
                            break;
                        case "2.5.4.8": // state
                            State = enc.GetString(txt.Data);
                            break;
                    }
                }
            }
        }

        public bool Equals(DistinguishedName n2)
        {
            return this.Organization == n2.Organization &&
                this.OrganizationalUnit == n2.OrganizationalUnit &&
                this.Dnq == n2.Dnq &&
                this.Country == n2.Country &&
                this.State == n2.State &&
                this.CommonName == n2.CommonName;
        }

        public override string ToString()
        {
            return "CN: " + CommonName + "\n" +
                "ON: " + Organization + "\n" +
                "Unit Name: " + OrganizationalUnit + "\n" +
                "Country: " + Country;
        }
    }

    internal class X509Cert
    {
        public string SerialNumber { get; private set; }
        public DateTime ValidAfter { get; private set; }
        public DateTime ValidBefore { get; private set; }
        public RSAKey PubKey { get; private set; }
        public bool SelfSigned { get; private set; }
        public DistinguishedName Subject { get; private set; }
        public DistinguishedName Issuer { get; private set; }
        private Asn1Node TbsCertificate;
        public Asn1Node Signature { get; private set; }
        public byte[] rawTBSCertificate;

        public X509Cert(Asn1Node n)
        {
            ParseNode(n);
        }

        public X509Cert(byte[] data)
        {
            using (var stm = new System.IO.MemoryStream(data))
            {
                Asn1Parser parser = new Asn1Parser();
                parser.LoadData(stm);
                ParseNode(parser.RootNode);
            }
        }

        public bool CheckCertTime(DateTime time)
        {
            return time.CompareTo(ValidAfter) >= 0 && time.CompareTo(ValidBefore) <= 0;
        }

        public bool CheckSignature(X509Cert signer)
        {
            if (Issuer.Equals(signer.Subject))
            {
                return signer.PubKey.Verify(rawTBSCertificate, Signature.Data);
            }
            return false;
        }

        public bool CheckSignature256(X509Cert signer)
        {
            if (Issuer.Equals(signer.Subject))
            {
                return signer.PubKey.Verify256(rawTBSCertificate, Signature.Data);
            }

            return false;
        }

        private void ParseNode(Asn1Node root)
        {
            if ((root.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.SEQUENCE || root.ChildNodeCount != 3)
                throw new InvalidX509Data();



            // TBS cert
            TbsCertificate = root.GetChildNode(0);
            if (TbsCertificate.ChildNodeCount < 7)
                throw new InvalidX509Data();

            rawTBSCertificate = new byte[TbsCertificate.DataLength + 4];
            Array.Copy(root.Data, 0, rawTBSCertificate, 0, rawTBSCertificate.Length);

            // get the serial number
            Asn1Node sn = TbsCertificate.GetChildNode(1);
            if ((sn.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.INTEGER)
                throw new InvalidX509Data();
            SerialNumber = Asn1Util.ToHexString(sn.Data);

            // get the issuer
            Issuer = new DistinguishedName(TbsCertificate.GetChildNode(3));

            // get the subject
            Subject = new DistinguishedName(TbsCertificate.GetChildNode(5));

            // get the dates
            Asn1Node validTimes = TbsCertificate.GetChildNode(4);
            if ((validTimes.Tag & Asn1Tag.TAG_MASK) != Asn1Tag.SEQUENCE || validTimes.ChildNodeCount != 2)
                throw new InvalidX509Data();
            ValidAfter = ParseTime(validTimes.GetChildNode(0));
            ValidBefore = ParseTime(validTimes.GetChildNode(1));

            // is this self signed?
            SelfSigned = Subject.Equals(Issuer);

            // get the pub key
            PubKey = new RSAKey(TbsCertificate.GetChildNode(6));

            // set the tbs cert & signature data for signature verification
            Signature = root.GetChildNode(2);
        }

        /**
         * According to rfc5280, time should be specified in GMT:
         * https://tools.ietf.org/html/rfc5280#section-4.1.2.5
         */
        private DateTime ParseTime(Asn1Node n)
        {
            string time = (new System.Text.UTF8Encoding()).GetString(n.Data);

            if (!(time.Length == 13 || time.Length == 15))
                throw new InvalidTimeFormat();

            // only accept Zulu time
            if (time[time.Length - 1] != 'Z')
                throw new InvalidTimeFormat();

            int curIdx = 0;

            int year = 0;
            if (time.Length == 13)
            {
                year = Int32.Parse(time.Substring(0, 2));
                if (year >= 50)
                    year += 1900;
                else if (year < 50)
                    year += 2000;
                curIdx += 2;
            }
            else
            {
                year = Int32.Parse(time.Substring(0, 4));
                curIdx += 4;
            }

            int month = Int32.Parse(time.Substring(curIdx, 2)); curIdx += 2;
            int dom = Int32.Parse(time.Substring(curIdx, 2)); curIdx += 2;
            int hour = Int32.Parse(time.Substring(curIdx, 2)); curIdx += 2;
            int min = Int32.Parse(time.Substring(curIdx, 2)); curIdx += 2;
            int secs = Int32.Parse(time.Substring(curIdx, 2)); curIdx += 2;

            return new DateTime(year, month, dom, hour, min, secs, DateTimeKind.Utc);
        }
    }

    /// <summary>
    /// An IAP Security exception indicating some invalid time format.
    /// </summary>
    public class InvalidTimeFormat : IAPSecurityException { }

    /// <summary>
    /// An IAP Security exception indicating some invalid data for X509 certification checks.
    /// </summary>
    public class InvalidX509Data : IAPSecurityException { }
}
