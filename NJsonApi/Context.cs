﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NJsonApi
{
    public class Context
    {
        public Configuration Configuration { get; set; }
        public string RoutePrefix { get; set; }
    }
}
