using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWBeheading
{
    interface IHeadDataContainer
    {
        HumanlikeHeadData GetInnerHeadData();
        void SetHeadData(HumanlikeHeadData data);
    }
}
