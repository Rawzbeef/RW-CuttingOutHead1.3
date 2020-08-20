using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RWBeheading
{
    public class HeadShell : ThingWithComps, IHeadDataContainer
    {
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look(ref _headData, "headData");
        }

        public HumanlikeHeadData GetInnerHeadData()
        {
            return _headData;
        }

        public void SetHeadData(HumanlikeHeadData data)
        {
            if (_headData != null)
            {
                CustomLogger.NeedCheck("HeadShell set head data although has already data.");
            }

            _headData = data;
        }

        private HumanlikeHeadData _headData;
    }
}
