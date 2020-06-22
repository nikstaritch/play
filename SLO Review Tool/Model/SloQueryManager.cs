using Kusto.Cloud.Platform.Utils;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Kusto.Ingest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SloReviewTool.Model
{
    class SloQueryManager
    {
        ICslQueryProvider client_;

        readonly string kustoUrl_ = "https://azurequality.westus2.kusto.windows.net/AzureQuality";
        readonly string kustoDb_ = "AzureQuality";
        readonly string kustoManualReviewTable_ = "SloDefinitionManualReview";

        public SloQueryManager()
        {
            Initialize();
        }

        void Initialize()
        {
            var kcsb = new KustoConnectionStringBuilder(kustoUrl_).WithAadUserPromptAuthentication();
            client_ = KustoClientFactory.CreateCslQueryProvider(kcsb);
        }

        /// <summary>
        /// Executes Kusto query obtain combined service and manual review data based on <paramref name="query"/>.
        /// </summary>
        /// <param name="query">A query with the initial criteria.</param>
        /// <returns>
        /// A list of <see cref="SloRecord"/> objects with a potential <see cref="SloValidationException"/> list that is
        /// returned as a tuple.
        /// </returns>
        /// <remarks>
        /// The constructed query obtains service data using <c>GetSloJsonActionItemReport</c> Kusto query and review data
        /// using <c>GetLatestManualReviewDecision</c> Kusto query via a table join. If the initial criteria was blank, then
        /// the data is obtained for the whole list of services.
        /// </remarks>
        public Tuple<List<SloRecord>, List<SloValidationException>> ExecuteQuery(string query)
        {
            var items = new List<SloRecord>();
            var errors = new List<SloValidationException>();
            var uniqueServiceIds = new SortedSet<string>();

            query += $@"| project ServiceId, OrganizationName, ServiceGroupName, TeamGroupName, ServiceName, YamlValue | join kind = leftouter GetLatestManualReviewDecision() on $left.ServiceId == $right.ServiceId | project-away ServiceId1";

            // "GetSloJsonActionItemReport() | where YamlValue contains ServiceId"
            using (var results = client_.ExecuteQuery(query)) {
                for (int i = 0; results.Read(); i++) {
                    try {
                        var result = ReadSingleResult((IDataRecord)results);

                        // Only add the latest value
                        if (!uniqueServiceIds.Contains(result.ServiceId)) {
                            items.Add(result);
                            uniqueServiceIds.Add(result.ServiceId);
                        }
                    } catch(SloValidationException ex) {
                        errors.Add(ex);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Schema violation: {ex.Message}");
                    }
                }
            }

            return Tuple.Create(items, errors);
        }

        /// <summary>
        /// Populate <see cref="SloRecord"/> model with the <paramref name="record"/> data obtained from running the Kusto query.
        /// </summary>
        /// <param name="record">Kusto data mapped as <see cref="IDataRecord"/> from a single entry.</param>
        /// <returns><see cref="SloRecord"/> object that contains service and review data.</returns>
        SloRecord ReadSingleResult(IDataRecord record)
        {
            var slo = new SloRecord();
            ThreadContext<SloParsingContext>.Set(new SloParsingContext(slo));
            slo.ServiceId = record["ServiceId"] as string;
            slo.OrganizationName = record["OrganizationName"] as string;
            slo.ServiceGroupName = record["ServiceGroupName"] as string;
            slo.TeamGroupName = record["TeamGroupName"] as string;
            slo.ServiceName = record["ServiceName"] as string;
            slo.SetYamlValue(record["YamlValue"] as string);

            slo.ReviewPassed = Convert.ToBoolean(record["ReviewPassed"]);
            slo.ReviewDetails = record["ReviewDetails"].ToString();
            slo.ReviewDate = !record.IsDBNull(record.GetOrdinal("ReviewDate"))
                ? (DateTime)record["ReviewDate"]
                : new DateTime();
            slo.ReviewedBy = record["ReviewedBy"].ToString();
            slo.AdvancedReviewRequired = !record.IsDBNull(record.GetOrdinal("AdvancedReviewRequired"))
                ? Convert.ToBoolean(record["AdvancedReviewRequired"])
                : false;
            slo.AcknowledgmentDetails = record["AcknowledgmentDetails"].ToString();
            slo.AcknowledgmentDate = !record.IsDBNull(record.GetOrdinal("AcknowledgmentDate"))
                ? (DateTime)record["AcknowledgmentDate"]
                : new DateTime();
            slo.AcknowledgedBy = record["AcknowledgedBy"].ToString();
            slo.AcknowledgedYamlValue = record["SloDefinition"].ToString();

            return slo;
        }

        /// <summary>
        /// Adds a record with <paramref name="results"/> in <see cref="kustoManualReviewTable_"/> table.
        /// </summary>
        /// <param name="results">Update manual review data.</param>
        /// <returns></returns>
        public async Task PublishManualReviews(IEnumerable<SloManualReview> results)
        {
            var dt = results.ToDataTable();

            var kustoConnectionStringBuilderEngine = new KustoConnectionStringBuilder(kustoUrl_).WithAadUserPromptAuthentication();

            using (var client = KustoIngestFactory.CreateDirectIngestClient(kustoConnectionStringBuilderEngine))
            {
                //Ingest from blobs according to the required properties
                var kustoIngestionProperties = new KustoIngestionProperties(
                    databaseName: kustoDb_,
                    tableName: kustoManualReviewTable_
                );

                var reader = dt.CreateDataReader();
                var result = await client.IngestFromDataReaderAsync(reader, kustoIngestionProperties);
            }
        }

    }
}
