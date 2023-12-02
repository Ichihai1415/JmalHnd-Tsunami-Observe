using System.Xml;

namespace JmalHnd_Tsunami_Observe
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string Uri = "https://www.data.jma.go.jp/developer/xml/feed/eqvol.xml";
                Console.Clear();
                bool Exist = true;
                Console.WriteLine($"処理を開始します。({DateTime.Now:HH:mm:ss.ff})");
                XmlDocument xml = new XmlDocument();
                Console.WriteLine($"取得中…({Uri})");
                xml.Load(Uri);
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                if (Uri == "https://www.data.jma.go.jp/developer/xml/feed/eqvol.xml" || Uri == "https://www.data.jma.go.jp/developer/xml/feed/eqvol_l.xml")
                {
                    Exist = false;
                    Console.WriteLine("feedから津波情報を検索中…");
                    foreach (XmlNode node in xml.SelectNodes("atom:feed/atom:entry", nsmgr))
                    {
                        if (node.SelectSingleNode("atom:title", nsmgr).InnerText == "津波情報a")
                        {
                            if (!node.SelectSingleNode("atom:content", nsmgr).InnerText.Contains("【津波観測に関する情報】"))
                                continue;
                            string URL2 = node.SelectSingleNode("atom:id", nsmgr).InnerText;
                            Console.WriteLine($"見つかりました。取得中…({URL2})");
                            xml.Load(URL2);
                            Exist = true;
                            goto view;
                        }
                    }
                    Console.WriteLine("見つかりませんでした。描画はしません。");
                    Console.WriteLine($"処理が完了しました。({DateTime.Now:HH:mm:ss.ff})");
                    return;
                }
            //未発表再現用
            //Exist = false;
            view:
                if (Exist)
                {//
                    nsmgr.AddNamespace("jmx", "http://xml.kishou.go.jp/jmaxml1/");
                    nsmgr.AddNamespace("jmx_ib", "http://xml.kishou.go.jp/jmaxml1/informationBasis1/");
                    nsmgr.AddNamespace("jmx_se", "http://xml.kishou.go.jp/jmaxml1/body/seismology1/");
                    nsmgr.AddNamespace("jmx_eb", "http://xml.kishou.go.jp/jmaxml1/elementBasis1/");
                    //{xml.SelectSingleNode("", nsmgr).InnerText}
                    Console.WriteLine($"\n\n/////津波観測に関する情報/////");


                    Console.Write($"{DateTime.Parse(xml.SelectSingleNode("jmx:Report/jmx_ib:Head/jmx_ib:ReportDateTime", nsmgr).InnerText):HH:mm}発表");
                    Console.WriteLine($"  第{xml.SelectSingleNode("/jmx:Report/jmx_ib:Head/jmx_ib:Serial", nsmgr).InnerText}報");
                    Console.WriteLine($"{xml.SelectSingleNode("/jmx:Report/jmx_ib:Head/jmx_ib:Headline/jmx_ib:Text", nsmgr).InnerText}");

                    foreach (XmlNode infos in xml.SelectNodes("/jmx:Report/jmx_se:Body/jmx_se:Tsunami/jmx_se:Observation/jmx_se:Item", nsmgr))
                    {
                        string area = infos.SelectSingleNode("jmx_se:Area/jmx_se:Name", nsmgr).InnerText;
                        foreach (XmlNode info in infos.SelectNodes("jmx_se:Station", nsmgr))
                        {
                            Console.Write($"{area}");
                            Console.Write($"  {info.SelectSingleNode("jmx_se:Name", nsmgr).InnerText}");
                            Console.Write($"    {info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:Condition", nsmgr).InnerText}");
                            Console.Write($"    {DateTime.Parse(info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:DateTime", nsmgr).InnerText):HH:mm}");
                            if (info.SelectSingleNode("jmx_se:MaxHeight/jmx_eb:TsunamiHeight", nsmgr) == null)
                                Console.WriteLine($"  {info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:Condition", nsmgr).InnerText}");//微弱? 観測中はどうなる？
                            else
                                Console.WriteLine($"  {info.SelectSingleNode("jmx_se:MaxHeight/jmx_eb:TsunamiHeight", nsmgr).InnerText}m");
                        }
                    }
                }
                //throw new Exception("デバック用");
                Console.WriteLine($"\n\n処理が完了しました。({DateTime.Now:HH:mm:ss.ff})");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n//////////////////////////////////////////////////\nエラーが発生しました。わからない場合開発者に報告してください。このスクリーンショットがあると助かります。" +
                    $"\n<エラーの例>\nオブジェクト参照がオブジェクト インスタンスに設定されていません。/値をnullにすることはできません:処理ミスです。" +
                    $"\n//////////////////////////////////////////////////\n内容:{ex}");
                Console.ReadKey();
            }

        }
    }
}