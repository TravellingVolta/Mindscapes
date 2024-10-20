﻿using System;
using System.Text.RegularExpressions;

namespace GameDataEditor
{
    public class GDETokenDefinition
    {
        public GDETokenDefinition(
            string type,
            Regex regex)
            : this(type, regex, false)
        {
        }

        public GDETokenDefinition(
            string type,
            Regex regex,
            bool isIgnored)
        {
            Type = type;
            Regex = regex;
            IsIgnored = isIgnored;
        }

        public bool IsIgnored { get; set; }
        public Regex Regex { get; set; }
        public string Type { get; set; }
    }
}
