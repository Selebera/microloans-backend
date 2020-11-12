using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace MicroLoan.Models
{
    public class LoanBundle
    {
        public LoanBundle(string userId, string loanName, string description)
        {
            UserId = userId;
            LoanName = loanName;
            Description = description;
        }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }

        [JsonProperty("loanName", NullValueHandling = NullValueHandling.Ignore)]
        public string LoanName { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public int Amount { get; set; }

        [JsonProperty("rate", NullValueHandling = NullValueHandling.Ignore)]
        public int Rate { get; set; }

        [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
        public int Duration { get; set; }

        [JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime Date { get; set; }

        [JsonProperty("amountpaid", NullValueHandling = NullValueHandling.Ignore)]
        public int AmountPaid { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }
    }
}