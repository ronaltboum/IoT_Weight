using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hx711onUWP
{
    interface IHx711
    {
        long GetValue();
        long AverageValue(byte times);
        long AverageValue(); //default value 25
        void SetOffset(long offset);
        void SetScale(float scale);
        void SetScale(); //default value 2992f
        float GetGram();
    }
}
