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
        public string SV { get; set; } = "1.14.0";
        public string TV { get; set; } = "1.13.2";
        public string appid { get; set; } = "com.beatgames.beatsaber";
        //byte[] length for target
        public int TargetByteSize { get; set; } = 553331304;
        public int SourceByteSize { get; set; } = 553331304;
        public bool inverted = false;

        public string GetDecrName()
        {
            return appid + "_" + SV + "TO" + TV + ".decr";
        }
    }
}