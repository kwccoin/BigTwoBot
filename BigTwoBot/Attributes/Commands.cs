﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Attributes
{
    class Command : Attribute
    {
        public string Trigger { get; set; }
        public bool AdminOnly { get; set; } = false;
        public bool DevOnly { get; set; } = false;
        public bool GroupOnly { get; set; } = false;
    }
}