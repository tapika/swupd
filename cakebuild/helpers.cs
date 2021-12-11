using System;
using System.Collections.Generic;
using System.Text;

namespace cakebuild
{
    public class helpers
    {
        public static string[] split(string s)
        {
            return s.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
