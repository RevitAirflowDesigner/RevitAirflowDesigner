﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Objects
{
    public class Edge
    {
        public Node pt1 { get; set; }
        public Node pt2 { get; set; }

        public double Distance { get; set; }
    }
}