using System.Xml;

namespace JmalHnd_Tsunami_Observe
{
    internal class Program
    {
        static void Main(string[] args)//気象庁防災情報XMLフォーマット技術情報 - 地震火山関連解説資料　https://dmdata.jp/docs/jma/manual/0101-0185.pdf#page=73
        {
            var debug = false;//ファイル読み込みデバッグ

            try//todo:保存、指定報数表示、マップ
            {
            restart:
                var uri = "https://www.data.jma.go.jp/developer/xml/feed/eqvol.xml";
                var xml = new XmlDocument();
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                var exist = true;
                Console.WriteLine($"処理を開始します。({DateTime.Now:HH:mm:ss.ff})");
                if (debug)
                    goto view;
                Console.Clear();
                Console.WriteLine($"取得中…({uri})");
                xml.Load(uri);
                nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                if (uri == "https://www.data.jma.go.jp/developer/xml/feed/eqvol.xml"/* || uri == "https://www.data.jma.go.jp/developer/xml/feed/eqvol_l.xml"*/)
                {
                    exist = false;
                    Console.WriteLine("feedから津波情報を検索中…");
                    foreach (XmlNode node in xml.SelectNodes("atom:feed/atom:entry", nsmgr)!)
                    {
                        if (node.SelectSingleNode("atom:title", nsmgr)?.InnerText == "津波情報a")
                        {
                            if (!node.SelectSingleNode("atom:content", nsmgr)!.InnerText.Contains("【津波観測に関する情報】"))
                                continue;
                            var uri2 = node.SelectSingleNode("atom:id", nsmgr)!.InnerText;
                            Console.WriteLine($"見つかりました。取得中…({uri2})");
                            xml.Load(uri2);
                            exist = true;
                            goto view;
                        }
                    }
                    Console.WriteLine("見つかりませんでした。");
                    Console.WriteLine($"処理が完了しました。({DateTime.Now:HH:mm:ss.ff})");
                    return;
                }
            //未発表再現用
            //Exist = false;
            view:
                if (exist)
                {//
                    if (debug)//""
                        //xml.Load("C:\\Users\\proje\\Downloads\\20240924020401_0_VTSE51_270000-edit.xml");
                    //xml.Load("C:\\Users\\proje\\Downloads\\20240102010301_0_VTSE51_010000.xml");
                    xml.Load("D:\\Ichihai1415\\data\\jma\\jmaxml_20240821_Samples\\32-39_12_12_191025_VTSE51.xml");

                    nsmgr.AddNamespace("jmx", "http://xml.kishou.go.jp/jmaxml1/");
                    nsmgr.AddNamespace("jmx_ib", "http://xml.kishou.go.jp/jmaxml1/informationBasis1/");
                    nsmgr.AddNamespace("jmx_se", "http://xml.kishou.go.jp/jmaxml1/body/seismology1/");
                    nsmgr.AddNamespace("jmx_eb", "http://xml.kishou.go.jp/jmaxml1/elementBasis1/");
                    //{xml.SelectSingleNode("", nsmgr).InnerText}
                    Console.WriteLine($"\n\n【津波観測に関する情報】");




                    Console.Write($"{DateTime.Parse(xml.SelectSingleNode("jmx:Report/jmx_ib:Head/jmx_ib:ReportDateTime", nsmgr)!.InnerText):MM/dd HH:mm}発表  ");
                    Console.Write(xml.SelectSingleNode("jmx:Report/jmx:Control/jmx:EditorialOffice", nsmgr)!.InnerText);
                    Console.Write($"  第{xml.SelectSingleNode("/jmx:Report/jmx_ib:Head/jmx_ib:Serial", nsmgr)?.InnerText}報 ");
                    Console.WriteLine(xml.SelectSingleNode("jmx:Report/jmx:Control/jmx:Status", nsmgr)!.InnerText);
                    Console.WriteLine($"{xml.SelectSingleNode("/jmx:Report/jmx_ib:Head/jmx_ib:Headline/jmx_ib:Text", nsmgr)?.InnerText}");
                    if (xml.SelectSingleNode("jmx:Report/jmx:Control/jmx:Status", nsmgr)!.InnerText == "取消")
                    {
                        Console.WriteLine("取り消し報です。処理を終了します。");
                        return;
                    }

                    //<Revise
                    Console.WriteLine("\n[追加/更新]");
                    
                    foreach (XmlNode infos in xml.SelectNodes("/jmx:Report/jmx_se:Body/jmx_se:Tsunami/jmx_se:Observation/jmx_se:Item", nsmgr)!)
                    {
                        var first = true;
                        foreach (XmlNode info in infos.SelectNodes("jmx_se:Station", nsmgr)!)
                        {
                            //[すべて]と異なるところ
                            if (info.SelectSingleNode("jmx_se:MinHeight/jmx_se:Revise", nsmgr) == null && info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:Revise", nsmgr) == null)
                                continue;
                            //[すべて]と異なるところ終了

                            if (first)
                            {
                                string area = infos.SelectSingleNode("jmx_se:Area/jmx_se:Name", nsmgr)?.InnerText ?? "<エリア名取得失敗>";
                                Console.Write("<" + area + ">");
                                first = false;
                            }

                            Console.SetCursorPosition(26, Console.CursorTop);//最大名は北海道太平洋沿岸東部
                            Console.Write("  ");
                            //観測点名
                            Console.Write(info.SelectSingleNode("jmx_se:Name", nsmgr)?.InnerText);
                            Console.SetCursorPosition(52, Console.CursorTop);//最大名は青森東方沖１００ｋｍＡ?

                            var maxTimeSt = info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:DateTime", nsmgr)?.InnerText;
                            if (maxTimeSt != null)
                                Console.Write(DateTime.Parse(maxTimeSt).ToString("MM/dd HH:mm"));
                            else
                                Console.Write("--/-- --:--");

                            var height = double.Parse(info.SelectSingleNode("jmx_se:MaxHeight/jmx_eb:TsunamiHeight", nsmgr)?.InnerText ?? "-1");
                            if (height < 10)
                                Console.SetCursorPosition(66, Console.CursorTop);
                            else
                                Console.SetCursorPosition(65, Console.CursorTop);

                            if (height == -1)
                                Console.Write("-.-");
                            else
                                Console.Write(height.ToString("#0.0"));
                            //以上(測定範囲超過)の判定用
                            var maxHeightDescription = info.SelectSingleNode("jmx_se:MaxHeight/jmx_eb:TsunamiHeight", nsmgr)?.Attributes?["description"]?.Value ?? "";
                            //上昇中
                            var maxHeightCondition = info.SelectSingleNode("jmx_se:MaxHeight/jmx_eb:TsunamiHeight", nsmgr)?.Attributes?["condition"]?.Value ?? "";

                            if (maxHeightDescription.Contains("以上"))
                                Console.Write("m以上 ");
                            else
                                Console.Write("m　　 ");
                            //小さいとき:[警報以上:観測中　注意報:微弱]  大津波警報の基準を超え、追加あるいは更新;重要　複数の場合全角空白
                            var maxCondition = info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:Condition", nsmgr)?.InnerText.Replace("重要", "").Replace("　", "");
                            Console.Write(maxCondition);
                            Console.Write(maxHeightCondition);

                            var maxRevise = info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:Revise", nsmgr)?.InnerText;
                            Console.SetCursorPosition(82, Console.CursorTop);
                            if (maxRevise == null)
                                Console.Write("");
                            else
                                Console.Write("(" + maxRevise + ")");
                            Console.SetCursorPosition(88, Console.CursorTop);

                            Console.Write("  |  ");

                            var firstTimeSt = info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:ArrivalTime", nsmgr)?.InnerText;
                            var firstCondition = info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:Condition", nsmgr)?.InnerText;
                            if (firstTimeSt != null)
                                Console.Write(DateTime.Parse(firstTimeSt).ToString("MM/dd HH:mm"));
                            else if (firstCondition != null)
                                if (firstCondition == "第１波識別不能")
                                    Console.Write("   第１波識別不能");
                                else
                                    Console.Write("--/-- --:--");
                            else
                                Console.Write("--/-- --:--");

                            Console.Write("  ");

                            Console.Write(info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:Initial", nsmgr)?.InnerText);

                            var minRevise = info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:Revise", nsmgr)?.InnerText;
                            Console.SetCursorPosition(111, Console.CursorTop);
                            if (minRevise == null)
                                Console.Write("");
                            else
                                Console.Write("(" + minRevise + ")");

                            Console.WriteLine();
                        }
                    }

                    Console.WriteLine("\n[すべて]");

                    foreach (XmlNode infos in xml.SelectNodes("/jmx:Report/jmx_se:Body/jmx_se:Tsunami/jmx_se:Observation/jmx_se:Item", nsmgr)!)
                    {
                        var first = true;
                        foreach (XmlNode info in infos.SelectNodes("jmx_se:Station", nsmgr)!)
                        {
                            if(first)
                            {
                                string area = infos.SelectSingleNode("jmx_se:Area/jmx_se:Name", nsmgr)?.InnerText ?? "<エリア名取得失敗>";
                                Console.Write("<" + area + ">");
                                first = false;
                            }

                            Console.SetCursorPosition(26, Console.CursorTop);//最大名は北海道太平洋沿岸東部
                            Console.Write("  ");
                            //観測点名
                            Console.Write(info.SelectSingleNode("jmx_se:Name", nsmgr)?.InnerText);
                            Console.SetCursorPosition(52, Console.CursorTop);//最大名は青森東方沖１００ｋｍＡ?

                            var maxTimeSt = info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:DateTime", nsmgr)?.InnerText;
                            if (maxTimeSt != null)
                                Console.Write(DateTime.Parse(maxTimeSt).ToString("MM/dd HH:mm"));
                            else
                                Console.Write("--/-- --:--");

                            var height = double.Parse(info.SelectSingleNode("jmx_se:MaxHeight/jmx_eb:TsunamiHeight", nsmgr)?.InnerText ?? "-1");
                            if (height < 10)
                                Console.SetCursorPosition(66, Console.CursorTop);
                            else
                                Console.SetCursorPosition(65, Console.CursorTop);

                            if (height == -1)
                                Console.Write("-.-");
                            else
                                Console.Write(height.ToString("#0.0"));
                            //以上(測定範囲超過)の判定用
                            var maxHeightDescription = info.SelectSingleNode("jmx_se:MaxHeight/jmx_eb:TsunamiHeight", nsmgr)?.Attributes?["description"]?.Value ?? "";
                            //上昇中
                            var maxHeightCondition = info.SelectSingleNode("jmx_se:MaxHeight/jmx_eb:TsunamiHeight", nsmgr)?.Attributes?["condition"]?.Value ?? "";

                            if (maxHeightDescription.Contains("以上"))
                                Console.Write("m以上 ");
                            else
                                Console.Write("m　　 ");
                            //小さいとき:[警報以上:観測中　注意報:微弱]  大津波警報の基準を超え、追加あるいは更新;重要　複数の場合全角空白
                            var maxCondition = info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:Condition", nsmgr)?.InnerText.Replace("重要", "").Replace("　", "");
                            Console.Write(maxCondition);
                            Console.Write(maxHeightCondition);

                            var maxRevise = info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:Revise", nsmgr)?.InnerText;
                            Console.SetCursorPosition(82, Console.CursorTop);
                            if (maxRevise == null)
                                Console.Write("");
                            else
                                Console.Write("(" + maxRevise + ")");
                            Console.SetCursorPosition(88, Console.CursorTop);

                            Console.Write("  |  ");

                            var firstTimeSt = info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:ArrivalTime", nsmgr)?.InnerText;
                            var firstCondition = info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:Condition", nsmgr)?.InnerText;
                            if (firstTimeSt != null)
                                Console.Write(DateTime.Parse(firstTimeSt).ToString("MM/dd HH:mm"));
                            else if (firstCondition != null)
                                if (firstCondition == "第１波識別不能")
                                    Console.Write("   第１波識別不能");
                                else
                                    Console.Write("--/-- --:--");
                            else
                                Console.Write("--/-- --:--");

                            Console.Write("  ");

                            Console.Write(info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:Initial", nsmgr)?.InnerText);

                            var minRevise = info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:Revise", nsmgr)?.InnerText;
                            Console.SetCursorPosition(111, Console.CursorTop);
                            if (minRevise == null)
                                Console.Write("");
                            else
                                Console.Write("(" + minRevise + ")");

                            Console.WriteLine();
                        }
                    }

                }
                //throw new Exception("デバック用");
                Console.WriteLine($"\n\n処理が完了しました。({DateTime.Now:HH:mm:ss.ff}) 何かキーを押すと再取得します。");
                Console.ReadKey();
                goto restart;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\n//////////////////////////////////////////////////\nエラーが発生しました。わからない場合開発者に報告してください。このスクリーンショットがあると助かります。" +
                    $"\n<エラーの例(英語になることがあります)>\nオブジェクト参照がオブジェクト インスタンスに設定されていません。/値をnullにすることはできません:処理ミスです。" +
                    $"\n//////////////////////////////////////////////////\n\n内容:{ex}\n\n何かキーを押すと終了します。");
                Console.ReadKey();
            }

        }
    }
}