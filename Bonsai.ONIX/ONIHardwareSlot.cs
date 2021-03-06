﻿namespace Bonsai.ONIX
{
    public class ONIHardwareSlot
    {
        public string Driver = "";

        public int Index = 0;

        internal string MakeKey()
        {
            return string.Format("({0},{1})", Driver, Index);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Driver))
            {
                return "";
            }
            else
            {
                return string.Format("({0},{1})", Driver, Index);
            }
        }
    }
}
