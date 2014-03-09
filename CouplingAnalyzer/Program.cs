using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http;
using System.Web;
using System.Net;
using System.IO;

namespace TNKTLib.CouplingAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            //var characters = new[] { "のぞ", "にこ", "えり", "こと", "ほの", "うみ", "まき", "りん", "ぱな" };
            Console.WriteLine("カップリング画像数調査ツール");
            Console.WriteLine("バージョン 1.0");
            Console.WriteLine("このソフトはpixivから指定したカップリングの画像数を調べるツールです (注 http://www.pixiv.net/に負荷をかける可能性があります。ご利用は自己責任で。\n");
            Console.WriteLine("調べたいカップリングのキャラクターの名前を一行ずつ入力し、最後にendと入力！");
            Console.WriteLine("例:\nこと\nほの\nうみ\nend");
            Console.WriteLine("キャラクターの名前を入力してください...\n");

            var characters = new List<string>();
            var input = "";
            do
            {
                input = Console.ReadLine();
                if (input != "end") characters.Add(input);
            } while (input != "end");

            var couplingCount = new Dictionary<string, int>();
            for (int i = 0; i < characters.Count; i++)
            {
                for (int j = i + 1; j < characters.Count; j++)
                {
                    var coupleName = characters[i] + characters[j];
                    couplingCount[coupleName] = GetCouplingCount(characters[i], characters[j]);
                    couplingCount[coupleName] += GetCouplingCount(characters[j], characters[i]);
                }
            }

            couplingCount.OrderByDescending(pair => pair.Value)
                .ToList()
                .ForEach(couple => Console.WriteLine(couple.Key + ": " + couple.Value + " 件"));
            Console.WriteLine("\n調査終了");
            Console.ReadKey();
        }

        static HttpClient client = new HttpClient();
        static int GetCouplingCount(string chara1, string chara2)
        {
            var uri = new Uri("http://www.pixiv.net/tags.php?tag=" + chara1 + chara2);
            var html = client.GetStringAsync(uri).Result;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            int count;
            var nodes = doc.DocumentNode.SelectNodes("//*[@id=\"item-container\"]/section[1]/section/a");
            var text = nodes.First().InnerText.Substring(10);
            var isParseable = Int32.TryParse(text.Substring(0, text.Length - 2), out count);
            if (isParseable)
                return count;

            nodes = doc.DocumentNode.SelectNodes("//*[@id=\"item-container\"]/section[2]/section/a");
            text = nodes.First().InnerText.Substring(10);
            return Int32.Parse(text.Substring(0, text.Length - 2));
        }
    }
}
