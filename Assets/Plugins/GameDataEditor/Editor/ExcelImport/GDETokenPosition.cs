using System;

namespace GameDataEditor
{
    public class GDETokenPosition
    {
        public GDETokenPosition(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }

        public int Column { get; set; }
        public int Index { get; set; }
        public int Line { get; set; }
    }
}
