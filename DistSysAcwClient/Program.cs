﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Client
{
    class Program
    {
        static HttpClient client = new HttpClient();
        static string name;
        static string apiKey;
        static string pubKey;
        static byte[] AESkey;
        static byte[] IV;

        static async Task Main()
        {
            client.BaseAddress = new Uri("http://150.237.94.9/3553366/api/");

            Console.WriteLine("Hello. What would you like to do?");
            string inputString = Console.ReadLine();
            int counter = 0;

            while (inputString != "Exit")
            {
                if(counter != 0)
                {
                    Console.WriteLine("What would you like to do next?");
                    inputString = Console.ReadLine();
                    Console.Clear();
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
                        Console.WriteLine("Please enter ApiKey Field by User Set");
                        break;
                    }

                    Console.WriteLine("Stored.");
                    break;

                case "Delete":
                    SetApiKey();
                    Task<string> userDelete = Delete("user/removeuser?username=" + name);
                    response = await userDelete;
                    if(response == "true")
                    {
                        Console.WriteLine("True");
                        break;
                    }

                    Console.WriteLine("False");
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
                    Task<string> userChangeRole = PostJoB("user/changerole", job);
                    response = await userChangeRole;
                    Console.WriteLine(response);
                    break;   
                    
            }
        }

        static async Task Protected(string[] pContent)
        {
            string command = pContent[1].ToLower();
            string command2 = "";
            if(pContent.Length > 2)
            {
                command2 = pContent[2].ToLower();
            }
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

                case "get":
                    if (command2 != "publickey")
                    {
                        Console.WriteLine("Couldn’t Get the Public Key");
                        break;
                    }

                    HttpResponseMessage httpResponse = await GetPublicKey();
                    pubKey = await httpResponse.Content.ReadAsStringAsync();

                    if(httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("Got Public Key");
                        break;
                    }

                    break;

                case "sign":
                    if(apiKey == null)
                    {
                        Console.WriteLine("You need to do a User Post or User Set first");
                        break;
                    }

                    else if(pubKey == null)
                    {
                        Console.WriteLine("Client doesn’t yet have the public key");
                        break;
                    }

                    string message = pContent[2];
                    Task<string> signResponse = Get("protected/sign?message=" + message);
                    response = await signResponse;

                   if(!PubkeyVerify(message, response))
                   {
                       Console.WriteLine("Message was not successfully signed");
                       break;
                   }

                   if(PubkeyVerify(message, response))
                   {
                       Console.WriteLine("Message was successfully signed");
                   }
                   break;

                case "mashify":
                    if(apiKey == null)
                    {
                        Console.WriteLine("You need to do a User Post or User Set first");
                        break;
                    }

                    if(pubKey == null || pubKey.Length <= 1)
                    {
                        Console.WriteLine("Client doesn't yet have the public key");
                    }

                    message = pContent[2];

                    using (Aes aes = Aes.Create())
                    {
                        aes.GenerateKey();
                        aes.GenerateIV();
                        AESkey = aes.Key;
                        IV = aes.IV;
                    }

                    using (RSA rsa = RSA.Create())
                    {
                        rsa.FromXmlString(pubKey);

                        byte[] encrypyedBytes = EncryptWithRSA(message, rsa);
                        byte[] encryptedKeyBytes = EncryptWithRSA(Convert.ToBase64String(AESkey), rsa);
                        byte[] encryptedIVBytes = EncryptWithRSA(Convert.ToBase64String(IV), rsa);

                        string encryptedString = Uri.EscapeDataString(BitConverter.ToString(encrypyedBytes));
                        string encryptedKey = Uri.EscapeDataString(BitConverter.ToString(encryptedKeyBytes));
                        string encryptedIV = Uri.EscapeDataString(BitConverter.ToString(encryptedIVBytes));

                        SetApiKey();
                        Console.Write("protected/mashify?encryptedString=" + encryptedString + "&encryptedSymKey=" + encryptedKey + "&encryptedIV=" + encryptedIV);
                        Task <HttpResponseMessage> mashifyResponse = GetHttp("protected/mashify?encryptedString="+encryptedString+"&encryptedSymKey="+encryptedKey+"&encryptedIV="+encryptedIV);
                        //HttpResponseMessage responseHttp = await mashifyResponse;
                        HttpResponseMessage responseMessage = await mashifyResponse;

                        Console.WriteLine(responseMessage);
                        break;
                    }          
            }
        }

        static byte[] EncryptWithRSA(string dataToEncrypt, RSA rsa)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(dataToEncrypt);
            return rsa.Encrypt(bytes, RSAEncryptionPadding.OaepSHA1);
        }

        static bool PubkeyVerify(string ogMessage, string signedMessage)
        {
            using RSA rsa = RSA.Create();
            rsa.FromXmlString(pubKey);

            signedMessage = signedMessage.Replace("-", "");
            byte[] hexbytes = HexToByteArray(signedMessage);
            byte[] ogMessageBytes = Encoding.UTF8.GetBytes(ogMessage);

            return rsa.VerifyData(ogMessageBytes, hexbytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        }

        static byte[] HexToByteArray(string hex)
        {
            int numofchars = hex.Length;
            byte[] bytes = new byte[numofchars / 2];
            for(int i = 0; i < numofchars; i += 2)
            {
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        static async Task<HttpResponseMessage> GetPublicKey()
        {
            SetApiKey();
            Task<HttpResponseMessage> getpublickey = HttpGet("protected/getpublickey");
            HttpResponseMessage httpResponse = await getpublickey;

            return httpResponse;
        }

        static void SetApiKey()
        {
            client.DefaultRequestHeaders.Remove("ApiKey");
            client.DefaultRequestHeaders.Add("ApiKey", apiKey);
        }

        static async Task<HttpResponseMessage> GetHttp(string path)
        {
            PleaseWait();
            HttpResponseMessage response = await client.GetAsync(path);
            return response;
        }

        static async Task<string> Get(string path)
        {
            PleaseWait();
            string responseString = "";
            HttpResponseMessage response = await client.GetAsync(path);
            responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        static async Task<HttpResponseMessage> HttpGet(string path)
        {
            PleaseWait();
            HttpResponseMessage response = await client.GetAsync(path);
            return response;
        }

        static async Task<string> Delete(string path)
        {
            PleaseWait();
            string responseString = "";
            HttpResponseMessage response = await client.DeleteAsync(path);
            responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        static async Task<HttpResponseMessage> Post(string path, string jsonBody)
        {
            PleaseWait();
            HttpResponseMessage response = await client.PostAsJsonAsync(path, jsonBody);
            return response;
        }

        static async Task<string> PostJoB(string path, JsonObject job)
        {
            PleaseWait();
            string responseString = "";
            HttpResponseMessage response = await client.PostAsJsonAsync(path, job);
            responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        static async Task<string> Put(string path, JsonObject jsonObject)
        {
            PleaseWait();
            string responseString = "";
            HttpResponseMessage response = await client.PutAsJsonAsync(path, jsonObject);
            responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        static async Task PleaseWait()
        {
            Console.WriteLine("...Please Wait...");
        }
    }
}
#region Task 10 and beyond

#endregion
