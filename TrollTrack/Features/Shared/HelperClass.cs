using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.Features.Shared
{
    //
    // Summary:
    //     Generic class that contains helpful methods used in the MVVM framework
    public static class HelperClass
    {
        //
        // Summary:
        //     Answer from http://stackoverflow.com/questions/5450696 Simplifies correctly calculating
        //     hash codes based upon Jon Skeet's answer here http://stackoverflow.com/a/263416
        //
        //
        // Parameters:
        //   obj:
        //
        //   memberThunks:
        //     Thunks that return all the members upon which the hash code should depend.
        public static int CalculateHashCode(this object obj, params Func<object>[] memberThunks)
        {
            int num = 8059;
            foreach (Func<object> func in memberThunks)
            {
                num = num * 10007 + func().GetHashCode();
            }

            return num;
        }

    }
}
