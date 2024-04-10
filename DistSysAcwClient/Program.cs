using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Client
{
    class Program
    {
        static HttpClient client = new HttpClient();
        static string name;
        static string apiKey;

        static async Task Main()
        {
            client.BaseAddress = new Uri("http://localhost:53415/api/");

            Console.WriteLine("Hello. What would you like to do?");
            string inputString = Console.ReadLine();
            int counter = 0;

            while (inputString != "Exit")
            {
                if(counter != 0)
                {
                    Console.WriteLine("What would you like to do next?");
                    inputString = Console.ReadLine();
                }

                string[] input = inputString.Split(" ");
                //input[0] == CONTROLLER
                //input[1] == COMMAND
                //input[2,3,4 . . .] extra fields depending on the needed ones

                switch (input[0])//controller switch
                {

                    case "TalkBack":
                        await TalkBack(input[1], input);
                        break;

                    case "User":
                        await User(input[1], input);
                        break;

                    case "Protected":
                        await Protected(input);
                        break;
                }

                counter++;
            }
            
        }

        static async Task TalkBack(string command, string[] pContent)
        {
            string response = "";
            switch (command)
            {
                case "Hello":
                    Task<string> talkbackHello = Get("talkback/hello");
                    response = await talkbackHello;
                    Console.WriteLine(response);
                    break;

                case "Sort":
                    string content = pContent[2].Replace("[", "").Replace("]", "");
                    string[] contents = content.Split(",");
                    List<int> numbers = new List<int>();
                    foreach (string numberString in contents)
                    {
                        int number = int.Parse(numberString);
                        numbers.Add(number);
                    }

                    string query = "";
                    foreach (int num in numbers)
                    {
                        query += "integers=" + num + "&";
                    }

                    Task<string> talkbackSort = Get("talkback/sort?" + query);
                    response = await talkbackSort;
                    Console.WriteLine(response);
                    break;
            }
        }

        static async Task User(string command, string[] pContent)
        {
            string response = "";
            switch (command)
            {
                case "Get":
                    Task<string> userGet = Get("user/new?username=" + pContent[2]);
                    response = await userGet;
                    Console.WriteLine(response);
                    break;

                case "Post":
                    Task<HttpResponseMessage> userPost = Post("user/new", pContent[2]);
                    HttpResponseMessage message = await userPost;
                    if(message.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("Got API Key");
                        string apiContent = await message.Content.ReadAsStringAsync();
                        apiKey = apiContent;
                    }
                    Console.WriteLine(message);
                    break;

                case "Set":
                    name = pContent[2];
                    try
                    {
                        apiKey = pContent[3];
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Please eneter ApiKey Field by User Set");
                        break;
                    }

                    Console.WriteLine("Stored.");
                    break;

                case "Delete":
                    SetApiKey();
                    Task<string> userDelete = Delete("user/removeuser?username=" + pContent[3]);
                    response = await userDelete;
                    Console.WriteLine(response);
                    break;

                case "Role":
                    if (apiKey == null)
                    {
                        Console.WriteLine("You need to do a User Post or User Set first.");
                    }

                    SetApiKey();
                    JsonObject job = new JsonObject();
                    job["username"] = pContent[2];
                    job["role"] = pContent[3].ToLower();
                    Task<string> userChangeRole = Put("user/changerole", job);
                    response = await userChangeRole;
                    Console.WriteLine(response);
                    break;           
            }
        }

        static async Task Protected(string[] pContent)
        {
            string command = pContent[1].ToLower();
            string response = "";
            if (apiKey == null)
            {
                Console.WriteLine("You need to do a User Post or User Set first.");
                return;
            }

            switch (command)
            {
                case "hello":
                    SetApiKey();
                    Task<string> protectedHello = Get("protected/hello");
                    response = await protectedHello;
                    Console.WriteLine(response);
                    break;

                case "sha1":
                    SetApiKey();
                    string sha1Message = pContent[2];
                    Task<string> protectedSha1 = Get("protected/sha1?message=" + sha1Message);
                    response = await protectedSha1;
                    Console.WriteLine(response);
                    break;

                case "sha256":
                    SetApiKey();
                    string sha256Message = pContent[2];
                    Task<string> protectedSha256 = Get("protected/sha256?message=" + sha256Message);
                    response = await protectedSha256;
                    Console.WriteLine(response);
                    break;
            }
        }

        static void SetApiKey()
        {
            client.DefaultRequestHeaders.Remove("ApiKey");
            client.DefaultRequestHeaders.Add("ApiKey", apiKey);
        }

        static async Task<string> Get(string path)
        {
            string responseString = "";
            HttpResponseMessage response = await client.GetAsync(path);
            responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        static async Task<string> Delete(string path)
        {
            string responseString = "";
            HttpResponseMessage response = await client.DeleteAsync(path);
            responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        static async Task<HttpResponseMessage> Post(string path, string jsonBody)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(path, jsonBody);
            return response;
        }

        static async Task<string> Put(string path, JsonObject jsonObject)
        {
            string responseString = "";
            HttpResponseMessage response = await client.PutAsJsonAsync(path, jsonObject);
            responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
    }
}
#region Task 10 and beyond

#endregion
