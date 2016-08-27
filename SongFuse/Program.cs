﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BFForever;
using BFForever.Riff;

namespace SongFuse
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 2) return;

            // Loads song resources
            SongManager sm = new SongManager(args[0]);

            // Loads single rif file
            RiffFile rif = sm.LoadRiffFile(args[1]);
        }
    }
}
