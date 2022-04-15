﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.Export.Json3D
{
    public class GridParameters
    {
        public List<double> origin { get; set; }
        public List<double> direction { get; set; }
        public double length { get; set; }
    }
}
