﻿using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;


namespace SloReviewTool.Model
{
    class SloQueryManager
    {
        ICslQueryProvider client_;

        public SloQueryManager()
        {
            Initialize();
        }

        void Initialize()
        {
            var kcsb = new KustoConnectionStringBuilder("https://azurequality.westus2.kusto.windows.net/AzureQuality").WithAadUserPromptAuthentication();
            client_ = KustoClientFactory.CreateCslQueryProvider(kcsb);
        }

        public List<SloRecord> ExecuteQuery(string query)
        {
            var items = new List<SloRecord>();

            // "GetSloJsonActionItemReport() | where YamlValue contains ServiceId"
            using (var results = client_.ExecuteQuery(query))
            {
                for (int i = 0; results.Read(); i++)
                {
                    items.Add(ReadSingleResult((IDataRecord)results));
                }
            }

            return items;
        }

        SloRecord ReadSingleResult(IDataRecord record)
        {
            var slo = new SloRecord();
            slo.ServiceId = record["ServiceId"] as string;
            slo.OrganizationName = record["OrganizationName"] as string;
            slo.ServiceGroupName = record["ServiceGroupName"] as string;
            slo.TeamGroupName = record["TeamGroupName"] as string;
            slo.ServiceName = record["ServiceName"] as string;
            slo.ServiceId = record["ServiceId"] as string;
            slo.SetYamlValue(record["YamlValue"] as string);

            return slo;
        }

    }
}