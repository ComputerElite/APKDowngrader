using System;
using System.Collections.Generic;

namespace APKDowngrader
{
    public class UpdateFile
    {
        public List<UpdateEntry> Updates { get; set; } = new List<UpdateEntry>();
    }

    public class UpdateEntry
    {
        public List<string> Creators { get; set; } = new List<string>();
        public string Changelog { get; set; } = "N/A";
        public string Download { get; set; } = "N/A";
        public string Version { get; set; } = "1.0.0";

        public Version GetVersion()
        {
            return new Version(Version);
        }
    }
}