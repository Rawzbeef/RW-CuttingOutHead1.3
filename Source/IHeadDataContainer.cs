using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWBeheading
{
    public interface IHeadDataContainer
    {
        HumanlikeHeadData GetInnerHeadData();
        void SetHeadData(HumanlikeHeadData data);
    }
}
