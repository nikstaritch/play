using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SloReviewTool.Model
{
    public class ManualReviewRecord
    {
        public string ServiceId { get; set; }

        public string ReviewDetails { get; set; }

        public bool ReviewPassed { get; set; }

        public DateTime ReviewDate { get; set; }

        public string ReviewedBy { get; set; }

        public bool AdvancedReviewRequired { get; set; }

        public string AcknowledgmentDetails { get; set; }

        public DateTime AcknowledgmentDate { get; set; }

        public string AcknowledgedBy { get; set; }

        public string AcknowledgedYamlValue { get; set; }

    }
}
