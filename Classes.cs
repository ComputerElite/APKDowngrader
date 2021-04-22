using System.Collections.Generic;

namespace Classes
{
    public class Versions
    {
        public List<Version> versions { get; set; } = new List<Version>();

        public bool IsPresent(string SV, string TV, string appid)
        {
            foreach (Version v in versions)
            {
                
                if (v.SV == SV && v.TV == TV && appid == v.appid || v.SV == TV && v.TV == SV && v.SourceByteSize == v.TargetByteSize && appid == v.appid) return true;
            }
            return false;
        }

        public bool IsPresent(string TV, string appid)
        {
            foreach (Version v in versions)
            {
                if (v.TV == TV || v.SV == TV && v.SourceByteSize == v.TargetByteSize && appid == v.appid) return true;
            }
            return false;
        }

        public Version GetVersion(string SV, string TV, string appid)
        {
            foreach (Version v in versions)
            {
                if (v.SV == SV && v.TV == TV && appid == v.appid) return v;
                else if (v.SV == TV && v.TV == SV && v.SourceByteSize == v.TargetByteSize && appid == v.appid) return v;
            }
            return null;
        }
    }

    public class Version
    {
        public string SV { get; set; } = "";
        public string TV { get; set; } = "";
        public string SSHA256 { get; set; } = "";
        public string DSHA256 { get; set; } = "";
        public string TSHA256 { get; set; } = "";
        public string download { get; set; } = "";
        public string appid { get; set; } = "";
        //byte[] length for target
        public int TargetByteSize { get; set; } = 0;
        public int SourceByteSize { get; set; } = 0;
        //old def values. 
        //public string SV { get; set; } = "1.14.0";
        //public string TV { get; set; } = "1.13.2";
        //public string SSHA256 { get; set; } = "dfa40268dad7f13cae15c66948f5714cb0f407aedb88387e06a873acf3aaa74f";
        //public string TSHA256 { get; set; } = "29aa9a2511ccc6be634cd7887a273eb1b5538ed2ccb24442572adfa0e1e30854";
        //public string download { get; set; } = "";
        //public string appid { get; set; } = "com.beatgames.beatsaber";
        //public int TargetByteSize { get; set; } = 553331304;
        //public int SourceByteSize { get; set; } = 553331304;
        public bool inverted = false;

        public string GetDecrName()
        {
            return appid + "_" + SV + "TO" + TV + ".decr";
        }

        public override bool Equals(object obj)
        {
            Version v = (Version)obj;
            return v.SV == this.SV && v.TV == this.TV && v.SSHA256 == this.SSHA256 && v.DSHA256 == this.DSHA256 && v.TSHA256 == this.TSHA256 && v.appid == this.appid && v.TargetByteSize == this.TargetByteSize && v.SourceByteSize == this.SourceByteSize;
        }
    }
}