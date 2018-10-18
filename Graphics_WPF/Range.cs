using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphics_WPF
{
    public class Range : ICloneable
    {
        public double LeftLimit { get; private set; }

        public double RightLimit { get; private set; }


        public Range(double leftLimit, double rightLimit)
        {
            LeftLimit = Math.Min(leftLimit, rightLimit);
            RightLimit = Math.Max(leftLimit, rightLimit);
        }


        public bool InRange(double point)
        {
            return point >= LeftLimit && point <= RightLimit;
        }

        public bool SetRange(double newLimit)
        {
            if (InRange(newLimit)) return false;

            var isLeftLimit = newLimit < LeftLimit;

            if (isLeftLimit)
                LeftLimit = newLimit;
            else
                RightLimit = newLimit;

            return true;

        }

        public Range GetAdditionalRange(double newLimit)
        {
            var inRange = InRange(newLimit);
            if (inRange) return null;

            Range additionalRange;

            var isLeftLimit = newLimit < LeftLimit;
            if (isLeftLimit)
            {
                additionalRange = new Range(newLimit, LeftLimit - 1);
            }
            else
            {
                additionalRange = new Range(RightLimit + 1, newLimit);
            }

            return additionalRange;
        }

        public double Length()
        {
            return RightLimit - LeftLimit;
        }

        public object Clone()
        {
            return new Range(LeftLimit, RightLimit);
        }
    }
}
