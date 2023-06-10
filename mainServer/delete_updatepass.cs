using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace amazonWebServices
{
    class Program
    {
        static void Main(string[] args)
        {
            //LoginUser("mat", "pass");
            //updateUserCoinsOrXP("mateLog", "coins", 10);
            //updatePasswd("test1", "value1", "pass1");
            //deleteAccount("test3", "pass3");
            updateUserCoinsOrXPbyNick("mate", "xp", 12);
        }
        public static void updateUserCoinsOrXPbyNick(string playerNick, string item, int value)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/updatevaluebynick");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"nick\":\"" + playerNick + "\"," +
                                "\"toUpdate\":\"" + item + "\"," +
                                "\"updatedValue\":\"" + value + "\"}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                var dataFromDatabase = result.Split("\"");
            }
        }
        public static void deleteAccount(string login, string password)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/deleteaccount");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"login\":\"" + login + "\"," +
                              "\"password\":\"" + password + "\"}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                var dataFromDatabase = result.Split("\"");
                var responseCode = Regex.Match(dataFromDatabase[2], @"\d+").Value;
                if (responseCode == "200")
                {
                    // usunieto
                    Console.WriteLine("konto usuniete");
                }
                else if (responseCode == "401")
                {
                    // zle haslo
                    Console.WriteLine("zle haslo");
                }
                else
                {
                    //wyslij info uzytkownikowi o bledzie innym niz wymienione (np server error)
                    Console.WriteLine("server error");
                }
            }
        }
        public static void updatePasswd(string login, string actualPasswd, string newPasswd)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/updatepasswd");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"login\":\"" + login + "\"," +
                              "\"currentPassword\":\"" + actualPasswd + "\"," +
                              "\"newPassword\":\"" + newPasswd + "\"}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                var dataFromDatabase = result.Split("\"");
                var responseCode = Regex.Match(dataFromDatabase[2], @"\d+").Value;
                if (responseCode == "200")
                {
                    //poprawnie zmienione
                    Console.WriteLine("haslo zmienione");
                }
                else if (responseCode == "401")
                {
                    //wyslij info uzytkownikowi o zlym hasle
                    Console.WriteLine("zle haslo");
                }
                else
                {
                    //wyslij info uzytkownikowi o bledzie innym niz wymienione (np server error)
                    Console.WriteLine("server error");
                }
            }
        }
        //funkcja w bazie danych to actualValue += value, podajemy login gracza, "coins" albo "xp" i wartosc ze znakiem o ile dodac/odjac
        public static void updateUserCoinsOrXP(string playerLogin, string item, int value)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/updatevalues");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"login\":\"" + playerLogin + "\"," +
                                "\"toUpdate\":\"" + item + "\"," +
                                "\"updatedValue\":\"" + value + "\"}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                var dataFromDatabase = result.Split("\"");
            }
        }

        public static void LoginUser(string playerLogin, string playerPassword)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/loginuser");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"login\":\"" + playerLogin + "\",\"password\":\"" + playerPassword + "\"}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                var dataFromDatabase = result.Split("\"");
                var responseCode = Regex.Match(dataFromDatabase[2], @"\d+").Value;
                if (responseCode == "412")
                {
                    //wyslij info o tym ze taki login nie istnieje
                }
                else if (responseCode == "416")
                {
                    //wyslij info uzytkownikowi o zlym hasle
                }
                else if (responseCode == "210") // ok
                {
                    string text = result;
                    string searchTerm = "xp";
                    string xp3 = string.Empty;
                    int pos = text.IndexOf(searchTerm);
                    if (pos >= 0)
                    {
                        string temp = text.Substring(pos + searchTerm.Length).Trim();
                        string[] parts = temp.Split('\"');
                        string value = parts[0];
                    }
                    var xp2 = Int32.Parse(Regex.Match(result, @".* (xp)(\D*)( ).*").Value); 
                    var xp = Int32.Parse(Regex.Match(result, @"xp\d+").Value);
                    var coins = Int32.Parse(Regex.Match(dataFromDatabase[8], @"\d+").Value);
                    var login = dataFromDatabase[11];
                    var nick = dataFromDatabase[15];
                    //wyslij uzytkownikowi te informacje
                }
                else //failed
                {
                    //wyslij info uzytkownikowi o bledzie innym niz wymienione (np server error)
                }
            }
        }
    }
}
