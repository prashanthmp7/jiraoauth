using JiraOAuthConnectLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace JiraOauthConnectLib
{
    public class JiraClientConnect
    {
        public string RequestToken { get; set; }
        public string RequestTokenUrl { get; set; }
        public string AuthroizeUrl { get; set; }
        public string AccessTokenUrl { get; set; }
        public JiraClientConnect(string requestTokenUrl, string authUrl, string accessTokenUrl)
        {

            RequestTokenUrl = requestTokenUrl;
            AuthroizeUrl = authUrl;
            AccessTokenUrl = accessTokenUrl;


        }

        public string GetRequestToken()
        {
            Console.WriteLine("---------------Request Token-------------\n");
            var restService = new RestServiceClass();
            var response = restService.SignUrlAndSendRequest(RequestTokenUrl, "POST", string.Empty, string.Empty);
            var token = response.Split('&');
            RequestToken = token[0];

            Console.WriteLine(RequestToken);

            return response;
        }

        public void AuthorizeRequestToken()
        {
            //Launch the browser to authorize

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = AuthroizeUrl + RequestToken,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        public string GetAccessToken(string verificationCode)
        {
            Console.WriteLine("---------------Access Token-------------\n");
            var restService = new RestServiceClass();
            var response = restService.SignUrlAndSendRequest(AccessTokenUrl, "POST", verificationCode.Trim(), RequestToken.Split('=')[1]);
            var result = response.Split('&');
            Console.WriteLine($"{result[0]} \n {result[1]} \n  {result[2]} \n {result[3]} \n {result[4]} ");
            RequestToken = result[0].Split('=')[1];
            return RequestToken;
        }

        public string GetProjects(string testUrl = "")
        {
            var url = string.IsNullOrEmpty(testUrl) ? "https://gsep-int.xxxx.com/jira/rest/api/latest/project" : testUrl;
            var restService = new RestServiceClass();
            var response = restService.SignUrlAndSendRequest(url, "GET", "", RequestToken);

            var projects = JsonConvert.DeserializeObject<IEnumerable<Project>>(response, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            int count = 0;
            Console.WriteLine("--------------Projects------------------\n");
            foreach (var project in projects)
            {
                count++;

                Console.WriteLine(count + "." + project.name + "\n");
            }

            return response;
        }

        public string GetIssue(string issueUrl = "")
        {
            try
            {
                Console.WriteLine("---------------Issue------------------ \n");
                var url = string.IsNullOrEmpty(issueUrl) ? "https://gsep-int.xxxx.com/jira/rest/api/2/issue/TEST-5383" : issueUrl;
                var restService = new RestServiceClass();
                var response = restService.SignUrlAndSendRequest(url, "GET", "", RequestToken);

                var issue = JsonConvert.DeserializeObject<Root>(response, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                Console.WriteLine($"Issue Title: {issue.fields.summary} \n Assignee:  {issue.fields.assignee.name} \n Email: {issue.fields.assignee.emailAddress} ");

                return response;
            }
            catch (Exception err)
            {

            }
            return string.Empty;
        }

        public string CreateTask()
        {
            var randomNumber = new Random();
            var number = randomNumber.Next();
            var url = "https://gsep-int.xxxx.com/jira/rest/api/2/issue";
            var task = @"{
            ""fields"": {
                    ""project"": { ""key"": ""TEST"" },
            ""summary"": ""Created from JiraOAuth 33"",
            ""description"": ""Testing Task creation from Jira Oauth!"",
            ""reporter"": { ""name"": ""JIRAUSERID"" },
            ""assignee"": { ""name"": ""JIRAUSERID"" },
            ""issuetype"": { ""name"": ""Task"" },
            ""priority"": { ""id"": ""1"" },
            ""labels"": [ ""Jira"", ""OAuth"", ""TestOAuth"" ],
            ""duedate"":""2021-01-20"",
            ""fixVersions"":[{""self"":""https://gsep-int.xxxx.com/jira/rest/api/2/version/51716"",""id"":""51716"",""name"":""20.10"",""archived"":false,""released"":false}]
            }
            }";

            var restService = new RestServiceClass();
            var response = restService.SignUrlAndSendRequest(url, "POST", "", RequestToken, task.ToString());

            var result = JsonConvert.DeserializeObject<TaskCreationResponse>(response);

            Console.WriteLine($"\n Key :  { result.key} \n URL : https://gsep-int.xxxx.com/jira/browse/{result.key}");
            return result.key;

        }

        public bool AddAttachments(string issueKey, IEnumerable<string> filePaths)
        {
            string restUrl = "https://gsep-int.xxxx.com/jira/rest/api/2";
            string issueLinkUrl = String.Format("{0}/issue/{1}/attachments", restUrl, issueKey);

            var filesToUpload = new List<FileInfo>();
            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File '{0}' doesn't exist", filePath);
                    return false;
                }

                var file = new FileInfo(filePath);
                if (file.Length > 10485760) // TODO Get Actual Limit
                {
                    Console.WriteLine("Attachment too large");
                    return false;

                }

                filesToUpload.Add(file);
            }

            if (filesToUpload.Count <= 0)
            {
                Console.WriteLine("No file to Upload");
                return false;
            }
            var restService = new RestServiceClass();
            var result = restService.PostAttachments(issueLinkUrl, filesToUpload, RequestToken);
            return result;
        }

        public string GetRequiredFields(string projectKey, string issueType)
        {
            string restUrl = $"https://gsep-int.xxxx.com/jira/rest/api/2/issue/createmeta?projectKeys={projectKey}&issueTypeNames={issueType}&expand=projects.issuetypes.fields";
            var restService = new RestServiceClass();
            var result = restService.SignUrlAndSendRequest(restUrl, "GET", "", "ACCESSTOKEN");
            return result;
        }



        public string EncryptedAccessToken(string accessToken)
        {
            var encrypt = new EncryptAccessToken();
            return encrypt.GetEncryptedAccessToken(accessToken);
        }

        public string DecryptedAccessToken(string encrypterAccessToken)
        {
            var deCrypt = new DecryptAccessToken();
            return deCrypt.GetDecryptedAccessToken(encrypterAccessToken);
        }
    }
}

