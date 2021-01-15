using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K102PhaymarcyApp.Helpers
{
   static class Utilites
    {
        public static bool IsEmpty(string[] arr)
        {
            foreach (var item in arr)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    return false;
                }
            }
            return true;
        } 
    }
}
