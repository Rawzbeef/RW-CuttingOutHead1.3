using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RWBeheading
{
    public class SpecialThingFilterWorker_HeadsColonist : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            HumanlikeHead head = t as HumanlikeHead;
            if (head == null)
            {
                return false;
            }

            return head.IsColonist;
        }
    }

    public class SpecialThingFilterWorker_HeadsStranger : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            HumanlikeHead head = t as HumanlikeHead;
            if (head == null)
            {
                return false;
            }

            return !head.IsColonist;
        }
    }
}
