using System;
using System.Collections.Generic;
using System.Net.Http;
using EduBot.Controllers;
using Newtonsoft.Json;

namespace EduBot.Service.GraphAssignmentsService
{
    public class GraphAssignmentsService
    {
        
        private static readonly HttpClient client = new HttpClient();


        public async void AddToAssignments(State state, string code)
        {
            

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", content);
        }
    }
    public class Resource
    {

        [JsonProperty("createdBy_token")]
        public string createdBy { get; set; }

        [JsonProperty("createdDateTime_token")]
        public string createdDateTime { get; set; }

        [JsonProperty("displayName_token")]
        public int displayName { get; set; }

        [JsonProperty("lastModifiedBy_token")]
        public string lastModifiedBy { get; set; }

        [JsonProperty("lastModifiedDateTime_token")]
        public string lastModifiedDateTime { get; set; }
    }

    public class Assignment
    {
        public string id { get; set; }
        public bool distributeForStudentWork { get; set; }
        public Resource resource { get; set; }
    }
}
