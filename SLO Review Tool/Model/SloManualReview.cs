using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SloReviewTool.Model
{
    class SloManualReview
    {
        public SloManualReview(SloRecord record)
        {
            ServiceId = Guid.Parse(record.ServiceId);
            ReviewPassed = record.ReviewPassed;
            ReviewDetails = record.ReviewDetails;
            // reset Advanced review required flag if the reviewer resets the manual review
            // so service owners can address new comments
            AdvancedReviewRequired = record.ReviewPassed ? record.AdvancedReviewRequired : false;
            AcknowledgmentDetails = record.AcknowledgmentDetails;
            AcknowledgmentDate = record.AcknowledgmentDate;
            AcknowledgedBy = record.AcknowledgedBy;
            AcknowledgedYamlValue = record.AcknowledgedYamlValue;

            // Update Review Date to be the current date
            ReviewDate = DateTime.Now;
            record.ReviewDate = ReviewDate;

            // Update Reviewed By to be the current user
            ReviewedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            record.ReviewedBy = ReviewedBy;
        }

        public Guid ServiceId { get; set; }

        public bool ReviewPassed{ get; set; }

        public string ReviewDetails { get; set; }

        public DateTime ReviewDate { get; set; }

        public string ReviewedBy { get; set; }

        public bool AdvancedReviewRequired { get; set; }

        public string AcknowledgmentDetails { get; set; }

        public DateTime AcknowledgmentDate { get; set; }

        public string AcknowledgedBy { get; set; }

        public string AcknowledgedYamlValue { get; set; }
    }
}
