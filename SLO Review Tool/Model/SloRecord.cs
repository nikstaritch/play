using System;

namespace SloReviewTool.Model
{
    public class SloRecord
    {
        SloDefinition slo_;

        public string ServiceId { get; set; }

        public string OrganizationName { get; set; }

        public string ServiceGroupName { get; set; }

        public string TeamGroupName { get; set; }

        public string ServiceName { get; set; }

        public string YamlValue { get; private set; }

        public string ReviewDetails { get; set; }

        public bool ReviewPassed { get; set; }

        public DateTime ReviewDate { get; set; }

        public string ReviewedBy { get; set; }

        public bool AdvancedReviewRequired { get; set; }

        public string AcknowledgmentDetails { get; set; }

        public DateTime AcknowledgmentDate { get; set; }

        public string AcknowledgedBy { get; set; }

        public string AcknowledgedYamlValue { get; set; }

        public void AddManualReview(ManualReviewRecord manualReviewRecord)
        {
            ReviewDetails = manualReviewRecord.ReviewDetails;
            ReviewPassed = manualReviewRecord.ReviewPassed;
            ReviewDate = manualReviewRecord.ReviewDate;
            ReviewedBy = manualReviewRecord.ReviewedBy;
            AdvancedReviewRequired = manualReviewRecord.AdvancedReviewRequired;
            AcknowledgmentDetails = manualReviewRecord.AcknowledgmentDetails;
            AcknowledgmentDate = manualReviewRecord.AcknowledgmentDate;
            AcknowledgedBy = manualReviewRecord.AcknowledgedBy;
            AcknowledgedYamlValue = manualReviewRecord.AcknowledgedYamlValue;
        }

        public void SetYamlValue(string yaml)
        {
            if (yaml == "") throw new SloValidationException(ThreadContext<SloParsingContext>.ForThread(), "YamlValue", null, "YamlValue missing");

            YamlValue = yaml;
            slo_ = SloDefinition.CreateFromServiceTreeJson(YamlValue);
        }

        public SloDefinition SloDefinition
        {
            get { return slo_; }
        }
    }
}
