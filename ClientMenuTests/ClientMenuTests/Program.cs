using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ClientMenuTests
{
    class Program
    {
        public class Acc
        {
            public string token;
            public string login;
            public string tokens;
            public NetworkStream ns;
            public Acc(string login, string password)
            {
                TcpClient server = new TcpClient();
                server.Connect("127.0.0.1", 6937);
                ns = server.GetStream();
                var rsaClient = new RSACryptoServiceProvider(1024);
                rsaClient.PersistKeyInCsp = false;
                byte[] keybuffer = new byte[1024];
                StringBuilder keyStringBuilder = new StringBuilder();
                int nobytes = ns.Read(keybuffer, 0, keybuffer.Length);
                keyStringBuilder.AppendFormat("{0}", Encoding.ASCII.GetString(keybuffer, 0, nobytes));
                rsaClient.FromXmlString(keyStringBuilder.ToString());
                byte[] message = System.Text.Encoding.ASCII.GetBytes(login + ' ' + password);
                var encryptedData = rsaClient.Encrypt(message, false);
                ns.Write(encryptedData, 0, encryptedData.Length);
                byte[] myReadBuffer = new byte[2048];
                int numberOfBytesRead = 0;
                StringBuilder myCompleteMessage = new StringBuilder();
                numberOfBytesRead = ns.Read(myReadBuffer, 0, myReadBuffer.Length);
                myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                string[] request = myCompleteMessage.ToString().Split(new char[] { ' ' });
                token = request[0];
                ns.Flush();
                bool error = false;
                if (token == "##&&@@0000")
                {
                    error = true;
                    Console.WriteLine("Server Error");
                }
                else if (token == "##&&@@0001")
                {
                    error = true;
                    Console.WriteLine("bad login");
                }
                else if (token == "##&&@@0002")
                {
                    error = true;
                    Console.WriteLine("bad password");
                }
                else if (token == "##&&@@0003")
                {
                    error = true;
                    Console.WriteLine("already logged");
                }
                if (!error)
                {
                    TcpClient serverGame = new TcpClient();
                    serverGame.Connect("127.0.0.1", 6938);
                    NetworkStream gameStream = serverGame.GetStream();
                    //Console.WriteLine(login + " connected");
                    this.login = request[3];
                    tokens = request[2];
                }
            }
        }
        public static Acc MenuTest;
        public static Acc MenuTester;
        public static Acc MenuTesterek;
        public static int currTest = 1;
        static void Main(string[] args)
        {

            //MenuTest MenuTests tests
            // MenuTester Menutester tests
            //MenuTesterek MenuTesterek tests
            MenuTest = new Acc("MenuTests", "tests");
            MenuTester = new Acc("MenuTester", "tests");
            MenuTesterek = new Acc("MenuTesterek", "tests");

            //Clear
            GetTablesTests();
            CreateTableTests();
            JoinTableTests();
            ChangeTableSettingsTests();
            StartGameTests();
            GetTokensTests();
            ChangePasswordTests();
            DeleteAccoutTests();


            Thread.Sleep(5000);

        }
        public static void GetTablesTests()
        {
            TestWithoutReturn("           Get tables, no tables | ", "2 ", "answer 2 1 ", MenuTester);
            TestWithoutReturn("                    Create table | ", "0 ABC 1 0 0 4500 4500 ", "answer 0 0 ", MenuTester);
            TestWithoutReturn("    Get tables when at the table | ", "2 ", "answer 2 2 ", MenuTester);
            TestWithoutReturn("                      Get tables | ", "2 ", "answer 2 0 :T:ABC MenuTester 1 0 0 4500 4500", MenuTest);
        }
        public static void CreateTableTests()
        {
            Console.WriteLine();
            TestWithoutReturn("         Create table bad packet | ", "0 1 0 0 7500 7500 ", "answer 0 A ", MenuTest);
            TestWithoutReturn("  Create table when at the table | ", "0 ABC 1 0 0 10 10 ", "answer 0 1 ", MenuTester);
            TestWithoutReturn(" Create table with existing name | ", "0 ABC 1 0 0 10 10 ", "answer 0 2 ", MenuTest);
            TestWithoutReturn("   Create table #5 bad parameter | ", "0 ABC 1 F 0 10 10 ", "answer 0 9 ", MenuTest);
            TestWithoutReturn("   Create table #6 bad parameter | ", "0 ABC 1 0 F 10 10 ", "answer 0 9 ", MenuTest);
            TestWithoutReturn("   Create table #7 bad parameter | ", "0 ABC 1 0 0 FF 10 ", "answer 0 9 ", MenuTest);
            TestWithoutReturn("   Create table #8 bad parameter | ", "0 ABC 1 0 0 10 FF ", "answer 0 9 ", MenuTest);
            TestWithoutReturn("       Create table to long name | ", "0 ABCDEFGHIJabcdefghij0123456789123 1 0 0 10 10 ", "answer 0 9 ", MenuTester);
            Console.Write("\n");
        }
        public static void JoinTableTests()
        {
            TestWithoutReturn("   Join table that doesn't exist | ", "1 XYZ ", "answer 1 2 ", MenuTest);
            TestWithoutReturn("    Join table when at the table | ", "1 ABC ", "answer 1 1 ", MenuTester);
            TestWithoutReturn("                      Join table | ", "1 ABC ", "answer 1 0 ", MenuTesterek);
            TestWithoutReturn("Join table with not enough token | ", "1 ABC ", "answer 1 3 ", MenuTest);
            TestWithoutReturn("         Join table without name | ", "1 ", "answer 1 A ", MenuTest);
            TestWithoutReturn("                         Log Out | ", "3 ", "answer 3 0 ", MenuTest);
            MenuTest = new Acc("MenuTests", "tests");
            Console.Write("\n");
        }
        public static void ChangeTableSettingsTests()
        {
            TestWithoutReturn("                      Bad packet | ", "5 0 0 7500 7500 ", "answer 5 A ", MenuTest);
            TestWithoutReturn(" Table settings, away from table | ", "5 1 0 0 7500 7500 ", "answer 5 1 ", MenuTest);
            //TestWithoutReturn(" Gra która wystartowała | ", "5 1 0 0 7500 7500 ", "answer 5 2 ", MenuTest);
            TestWithoutReturn(" Table settings #4 bad parameter | ", "5 1 F 0 7500 7500 ", "answer 5 9 ", MenuTester);
            TestWithoutReturn(" Table settings #5 bad parameter | ", "5 1 0 F 7500 7500 ", "answer 5 9 ", MenuTester);
            TestWithoutReturn(" Table settings #6 bad parameter | ", "5 1 0 0 FF 7500 ", "answer 5 9 ", MenuTester);
            TestWithoutReturn(" Table settings #7 bad parameter | ", "5 1 0 0 7500 FF ", "answer 5 9 ", MenuTester);
            TestWithoutReturn("           Change table settings | ", "5 1 0 1 9000 6000 ", "answer 5 0 ", MenuTester);
            TestWithoutReturn("         Get tables after change | ", "2 ", "answer 2 0 :T:ABC MenuTester 2 0 1 9000 6000", MenuTest);
            Console.Write("\n");
        }
        public static void GetTokensTests()
        {
            TestWithoutReturn(" Get Tokens already at the table | ", "7 ", "answer 7 2 ", MenuTester);
            TestWithoutReturn("                      Get Tokens | ", "7 ", "answer 7 0 1 ", MenuTest);
            DateTime now = DateTime.Now;
            int expect;
            if (now.Hour >= 12)
                expect = 24 - now.Hour;
            else
                expect = 12 - now.Hour;
            TestWithoutReturn("    Get Tokens "+expect.ToString()+" hours remaining | ", "7 ", "answer 7 1 " + expect.ToString() + " ", MenuTest);
            Console.WriteLine();
        }
        public static void StartGameTests()
        {
            TestWithoutReturn("          Start game, away table | ", "6 ", "answer 6 1 ", MenuTest);
            //TestWithoutReturn("                      Start game | ", "6 ", "answer 6 0 ", MenuTester);
            //TestWithoutReturn("  Start game, running game error | ", "6 ", "answer 6 1 ", MenuTester);
            Console.WriteLine();
        }
        public static void ChangePasswordTests()
        {
            TestWithoutReturn("      Change Password bad packet | ", "9 tests test ", "answer 9 A ", MenuTest);
            TestWithoutReturn("       New password not the same | ", "9 tests test testowiron ", "answer 9 1 ", MenuTest);
            TestWithoutReturn("                    Bad password | ", "9 tescik test test ", "answer 9 2 ", MenuTest);
            TestWithoutReturn("                Password changed | ", "9 tests tests tests ", "answer 9 0 ", MenuTest);
            Console.WriteLine();
        }
        public static void DeleteAccoutTests()
        {
            TestWithoutReturn("       Delete account bad packet | ", "10 ", "answer A A ", MenuTest);
            TestWithoutReturn(" Player can't delete account now | ", "10 tests ", "answer A 3 ", MenuTester);
            TestWithoutReturn("      Delete accout bad password | ", "10 tescik ", "answer A 1 ", MenuTest);
            //TestWithoutReturn("               Account deleted | ", "10 tests ", "answer A 0 ", MenuTest);
            Console.WriteLine();
            TestWithoutReturn("              other request test | ", "15 cos tam ", "answer 100 1 ", MenuTest);
        }
        public static void TestWithoutReturn(string name, string send, string expect, Acc who)
        {
            byte[] tosend = System.Text.Encoding.ASCII.GetBytes(who.token + ' ' + send);
            who.ns.Write(tosend, 0, tosend.Length);
            byte[] readBuf = new byte[256];
            StringBuilder menuRequestStr = new StringBuilder();
            int nrbyt = who.ns.Read(readBuf, 0, readBuf.Length);
            menuRequestStr.AppendFormat("{0}", Encoding.ASCII.GetString(readBuf, 0, nrbyt));
            string returned = menuRequestStr.ToString();
            Console.Write(currTest.ToString("00") + ". ");
            currTest++;
            Console.Write(name);
            
            if (returned == expect)
            {
                Console.Write("OK\n");
            }
            else
            {
                Console.Write("Failed, was: "+ returned + " expected: " + expect + "\n");
            }
        }
    }
}
