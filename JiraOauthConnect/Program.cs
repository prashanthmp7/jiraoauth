using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace JiraOauthConnectLib
{
    class Program
    {

        static void Main(string[] args)
        {
            StartOAuthProcess();
        }

        private static void StartOAuthProcess()
        {
            var baseAddress = ConfigurationManager.AppSettings["BaseAddress"].ToString();
            var requestTokenUrl = baseAddress + ConfigurationManager.AppSettings["RequestTokenAddress"].ToString();
            var authroizeUrl = baseAddress + ConfigurationManager.AppSettings["AuthorizeAddress"].ToString();
            var accessTokenUrl = baseAddress + ConfigurationManager.AppSettings["AccessTokenAddress"].ToString();

            Console.WriteLine("--------------Starting authentication process------\n");
            JiraClientConnect connect = new JiraClientConnect(requestTokenUrl, authroizeUrl, accessTokenUrl);

            Console.WriteLine("--------------Getting Request token.---------------\n");
            connect.GetRequestToken();

            Console.WriteLine("\n------------Authorize Request token.------------\n");
            connect.AuthorizeRequestToken();
            Console.WriteLine("--------------Enter the verification code--------- \n");
            var verificationCode = Console.ReadLine();
            Console.WriteLine("\n------------Getting access token-----------------\n");
            var accessToken = connect.GetAccessToken(verificationCode);

            Console.WriteLine("\nEncrypted Access Token\n");
            var encryptedToken = connect.EncryptedAccessToken(accessToken);
            Console.WriteLine(encryptedToken);
            Console.WriteLine("\n");

            Console.WriteLine("\n Decrypted Access Token\n");
            var deCryptedToken = connect.DecryptedAccessToken(encryptedToken);
            Console.WriteLine(deCryptedToken);

            Console.WriteLine("\n");
            Console.WriteLine("\nMaking a test Jira Rest API call with the received access token.\n");
            connect.GetProjects();
            Console.WriteLine("\n");
            connect.GetIssue();
            Console.WriteLine("\n");
            Console.WriteLine("Creating a task\n");

            var result = connect.CreateTask();

            Console.WriteLine("\n");
            Console.WriteLine("Adding attachments\n");
            connect.AddAttachments(result, new[] {  @"C:\temp\EmailTemplates\TaskEmails\Agenda.png",
                                                    @"C:\temp\EmailTemplates\TaskEmails\Asset 1.png",
                                                    @"C:\temp\EmailTemplates\TaskEmails\Asset 5.png",
                                                    @"C:\temp\EmailTemplates\TaskEmails\Deputy.png",
                                                    @"C:\temp\EmailTemplates\TaskEmails\TaskAssignment.png",
                                                    @"C:\temp\EmailTemplates\TaskEmails\TaskDelete.png",
                                                    @"C:\temp\EmailTemplates\TaskEmails\TaskStatus.png"});
            Console.WriteLine("\n");


            //Get mandatory fields from jira based on project
            var data = connect.GetRequiredFields("PROJECTID", "Task");
            var arrays = JArray.Parse(data);
            var root = JsonConvert.DeserializeObject<Root>(data);
            var fields = new List<Issuetype>();
            var query = root.projects.Select(x => x.issuetypes).SelectMany(y => y).Where(x => x.name == "Task").Select(x => x.fields);
            foreach (var field in query)
            {

                PropertyInfo[] properties = typeof(Fields).GetProperties();
                foreach (PropertyInfo property in properties)
                {

                }
            }

        }

        public static Object GetPropValue(String name, object obj, Type type)
        {
            var parts = name.Split('.').ToList();
            var currentPart = parts[0];
            PropertyInfo info = type.GetProperty(currentPart);
            if (info == null) { return null; }
            if (name.IndexOf(".") > -1)
            {
                parts.Remove(currentPart);
                return GetPropValue(String.Join(".", parts), info.GetValue(obj, null), info.PropertyType);
            }
            else
            {
                return info.GetValue(obj, null).ToString();
            }
        }


    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class AvatarUrls
    {
        public string _48x48 { get; set; }
        public string _24x24 { get; set; }
        public string _16x16 { get; set; }
        public string _32x32 { get; set; }
    }

    public class Schema
    {
        public string type { get; set; }
        public string system { get; set; }
        public string custom { get; set; }
        public int customId { get; set; }
        public string items { get; set; }
    }

    public class Summary
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Timetracking
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield18402
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class AllowedValue
    {
        public string self { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public bool archived { get; set; }
        public bool released { get; set; }
        public int projectId { get; set; }
        public string releaseDate { get; set; }
        public bool? overdue { get; set; }
        public string userReleaseDate { get; set; }
        public string startDate { get; set; }
        public string userStartDate { get; set; }
        public string value { get; set; }
        public bool disabled { get; set; }
        public string iconUrl { get; set; }
        public bool subtask { get; set; }
        public int? avatarId { get; set; }
        public string key { get; set; }
        public string projectTypeKey { get; set; }
        public AvatarUrls avatarUrls { get; set; }
    }

    public class Versions
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Attachment
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<object> operations { get; set; }
    }

    public class Components
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<object> allowedValues { get; set; }
    }

    public class Duedate
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Environment
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield10006
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class FixVersions
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield10000
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield18410
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield18411
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Issuetype2
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<object> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Labels
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public string autoCompleteUrl { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Issuelinks
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public string autoCompleteUrl { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield18412
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class DefaultValue
    {
        public string self { get; set; }
        public string iconUrl { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public string value { get; set; }
        public bool disabled { get; set; }
    }

    public class Priority
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
        public DefaultValue defaultValue { get; set; }
    }

    public class Customfield18418
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield18419
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Reporter
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public string autoCompleteUrl { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield18424
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield10005
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield18435
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield18436
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield18432
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield18439
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield18440
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Description
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield18431
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield18429
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield18421
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield18443
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield18444
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield26024
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Assignee
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public string autoCompleteUrl { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Project2
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield18430
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield18433
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield18437
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
    }

    public class Customfield18438
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public string autoCompleteUrl { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield10002
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield10007
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield10008
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
        public DefaultValue defaultValue { get; set; }
    }

    public class Parent
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26018
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26019
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26020
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26026
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public string autoCompleteUrl { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26016
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26028
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26012
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26001
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
        public DefaultValue defaultValue { get; set; }
    }

    public class Customfield26002
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
        public DefaultValue defaultValue { get; set; }
    }

    public class Customfield26003
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26004
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26005
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26008
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26009
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26010
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26013
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26022
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
        public List<AllowedValue> allowedValues { get; set; }
        public DefaultValue defaultValue { get; set; }
    }

    public class Customfield26023
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26025
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Customfield26027
    {
        public bool required { get; set; }
        public Schema schema { get; set; }
        public string name { get; set; }
        public string fieldId { get; set; }
        public bool hasDefaultValue { get; set; }
        public List<string> operations { get; set; }
    }

    public class Fields
    {
        public Summary summary { get; set; }
        public Timetracking timetracking { get; set; }
        public Customfield18402 customfield_18402 { get; set; }
        public Versions versions { get; set; }
        public Attachment attachment { get; set; }
        public Components components { get; set; }
        public Duedate duedate { get; set; }
        public Environment environment { get; set; }
        public Customfield10006 customfield_10006 { get; set; }
        public FixVersions fixVersions { get; set; }
        public Customfield10000 customfield_10000 { get; set; }
        public Customfield18410 customfield_18410 { get; set; }
        public Customfield18411 customfield_18411 { get; set; }
        public Issuetype issuetype { get; set; }
        public Labels labels { get; set; }
        public Issuelinks issuelinks { get; set; }
        public Customfield18412 customfield_18412 { get; set; }
        public Priority priority { get; set; }
        public Customfield18418 customfield_18418 { get; set; }
        public Customfield18419 customfield_18419 { get; set; }
        public Reporter reporter { get; set; }
        public Customfield18424 customfield_18424 { get; set; }
        public Customfield10005 customfield_10005 { get; set; }
        public Customfield18435 customfield_18435 { get; set; }
        public Customfield18436 customfield_18436 { get; set; }
        public Customfield18432 customfield_18432 { get; set; }
        public Customfield18439 customfield_18439 { get; set; }
        public Customfield18440 customfield_18440 { get; set; }
        public Description description { get; set; }
        public Customfield18431 customfield_18431 { get; set; }
        public Customfield18429 customfield_18429 { get; set; }
        public Customfield18421 customfield_18421 { get; set; }
        public Customfield18443 customfield_18443 { get; set; }
        public Customfield18444 customfield_18444 { get; set; }
        public Customfield26024 customfield_26024 { get; set; }
        public Assignee assignee { get; set; }
        public Project project { get; set; }
        public Customfield18430 customfield_18430 { get; set; }
        public Customfield18433 customfield_18433 { get; set; }
        public Customfield18437 customfield_18437 { get; set; }
        public Customfield18438 customfield_18438 { get; set; }
        public Customfield10002 customfield_10002 { get; set; }
        public Customfield10007 customfield_10007 { get; set; }
        public Customfield10008 customfield_10008 { get; set; }
        public Parent parent { get; set; }
        public Customfield26018 customfield_26018 { get; set; }
        public Customfield26019 customfield_26019 { get; set; }
        public Customfield26020 customfield_26020 { get; set; }
        public Customfield26026 customfield_26026 { get; set; }
        public Customfield26016 customfield_26016 { get; set; }
        public Customfield26028 customfield_26028 { get; set; }
        public Customfield26012 customfield_26012 { get; set; }
        public Customfield26001 customfield_26001 { get; set; }
        public Customfield26002 customfield_26002 { get; set; }
        public Customfield26003 customfield_26003 { get; set; }
        public Customfield26004 customfield_26004 { get; set; }
        public Customfield26005 customfield_26005 { get; set; }
        public Customfield26008 customfield_26008 { get; set; }
        public Customfield26009 customfield_26009 { get; set; }
        public Customfield26010 customfield_26010 { get; set; }
        public Customfield26013 customfield_26013 { get; set; }
        public Customfield26022 customfield_26022 { get; set; }
        public Customfield26023 customfield_26023 { get; set; }
        public Customfield26025 customfield_26025 { get; set; }
        public Customfield26027 customfield_26027 { get; set; }
    }

    public class Issuetype
    {
        public string self { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string iconUrl { get; set; }
        public string name { get; set; }
        public bool subtask { get; set; }
        public string expand { get; set; }
        public Fields fields { get; set; }
    }

    public class Project
    {
        public string expand { get; set; }
        public string self { get; set; }
        public string id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public AvatarUrls avatarUrls { get; set; }
        public List<Issuetype> issuetypes { get; set; }
    }

    public class Root
    {
        public string expand { get; set; }
        public List<Project> projects { get; set; }
    }


}
