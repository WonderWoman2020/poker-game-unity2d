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
        //przykladowe konta
        // mateLog passwd
        // mat pass
        // login1 pass1
        static void Main(string[] args)
        {
            //LoginUser("mateLog", "passwd");
            updateUserCoinsOrXP("mateLog", "coins", 10);
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
                    string toBeSearched = "xp\":";
                    var xpH = result.Substring(result.IndexOf(toBeSearched) + toBeSearched.Length);
                    var xp = Int32.Parse(Regex.Match(xpH, @"\d+").Value);

                    toBeSearched = "coins\":";
                    var coinsH = result.Substring(result.IndexOf(toBeSearched) + toBeSearched.Length);
                    var coins = Int32.Parse(Regex.Match(coinsH, @"\d+").Value);

                    toBeSearched = "login\":\"";
                    var loginH = result.Substring(result.IndexOf(toBeSearched) + toBeSearched.Length);
                    var data = loginH.Split('\"');
                    var login = data[0];

                    toBeSearched = "nick\":\"";
                    var nickH = result.Substring(result.IndexOf(toBeSearched) + toBeSearched.Length);
                    data = nickH.Split('\"');
                    var nick = data[0];
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
