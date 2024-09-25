using System.Xml;

namespace JmalHnd_Tsunami_Observe
{
    internal class Program
    {
        public readonly static ConsoleColor defaultColor = Console.ForegroundColor;
        public readonly static bool debug = false;//ファイル読み込みデバッグ

        static void Main(string[] args)//気象庁防災情報XMLフォーマット技術情報 - 地震火山関連解説資料　https://dmdata.jp/docs/jma/manual/0101-0185.pdf#page=73
        {
            Console.WriteLine(new string('/', 120));
            Console.WriteLine("JmalHnd-Tsunami-Observe v1.0.6    Ichihai1415\n" +
                "このソフトはWindows11の標準のコマンドプロンプトでの表示用に作られています。\n" +
                "それ以外の場合や上下の/が改行されている場合は表示が正しくされない場合があります。\n" +
                "またこのソフトは臨時のものです。README.mdを確認してください。");
            Console.WriteLine(new string('/', 120));

            Console.WriteLine("\n何かキーを押すと続行します。");
            Console.ReadKey();
            try//todo:保存、指定報数表示、マップ
            {
            restart:
                var uri = args.Length == 1 ? args[0].Replace("\"", "") : "https://www.data.jma.go.jp/developer/xml/feed/eqvol.xml";
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
                if (uri == "https://www.data.jma.go.jp/developer/xml/feed/eqvol.xml" || uri == "https://www.data.jma.go.jp/developer/xml/feed/eqvol_l.xml")
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
                            xml = new XmlDocument();
                            xml.Load(uri2);
                            exist = true;
                            goto view;
                        }
                    }
                    Console.WriteLine("見つかりませんでした。");
                    Console.WriteLine($"処理が完了しました。({DateTime.Now:HH:mm:ss.ff})\n\n何かキーを押すと再取得します。");
                    Console.ReadKey();
                    goto restart;
                }
            //未発表再現用
            //exist = false;
            view:
                if (exist)
                {
                    if (xml == null)
                        throw new Exception("xmlの取得に失敗しています。データがありません。");

                    if (debug)//""
                              //xml.Load("C:\\Users\\proje\\Downloads\\20240924020401_0_VTSE51_270000-edit.xml");
                              //xml.Load("C:\\Users\\proje\\Downloads\\20240102010301_0_VTSE51_010000.xml");
                              //xml.Load("D:\\Ichihai1415\\data\\jma\\jmaxml_20240821_Samples\\32-39_12_12_191025_VTSE51.xml");
                              //xml.Load("D:\\Ichihai1415\\data\\jma\\jmaxml_20240821_Samples\\32-39_12_14_191025_VTSE51.xml");
                              //xml.Load("D:\\Ichihai1415\\data\\jma\\jmaxml_20240821_Samples\\32-39_11_08_120615_VTSE51.xml");//新規/更新なし
                              //xml.Load("D:\\Ichihai1415\\data\\jma\\jmaxml_20240821_Samples\\38-39_03_03_210805_VTSE51.xml");//取り消し(ヘッダは通常)
                        Console.Write("");//if (debug)で↓のを入れないように

                    nsmgr = new XmlNamespaceManager(xml.NameTable);
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
                        Console.WriteLine("取り消し報です。処理を終了します。\n\n何かキーを押すと再取得します。");
                        Console.ReadKey();
                        goto restart;
                    }

                    for (int view = 1; view <= 2; view++)
                    {
                        if (view == 1)
                            ConsoleWrite("\n[追加/更新]", ConsoleColor.White, true);
                        else
                            ConsoleWrite("\n[すべて]", ConsoleColor.White, true);


                        ConsoleWrite("エリア名　　　　　　　　　観測点名                更新時刻      高さ     付加事項       |  第一波の時刻 初動 付加事項", ConsoleColor.White, true);
                        ConsoleWrite(new string('-', 120), ConsoleColor.White, true);

                        foreach (XmlNode infos in xml.SelectNodes("/jmx:Report/jmx_se:Body/jmx_se:Tsunami/jmx_se:Observation/jmx_se:Item", nsmgr)!)
                        {
                            var first = true;
                            foreach (XmlNode info in infos.SelectNodes("jmx_se:Station", nsmgr)!)
                            {
                                if (view == 1)
                                    if (info.SelectSingleNode("jmx_se:MinHeight/jmx_se:Revise", nsmgr) == null && info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:Revise", nsmgr) == null)
                                        continue;

                                if (first)
                                {
                                    string area = infos.SelectSingleNode("jmx_se:Area/jmx_se:Name", nsmgr)?.InnerText ?? "";
                                    ConsoleWrite(area);
                                    first = false;
                                }

                                Console.SetCursorPosition(24, Console.CursorTop);//最大名は北海道太平洋沿岸東部
                                Console.Write("  ");
                                //観測点名
                                ConsoleWrite(info.SelectSingleNode("jmx_se:Name", nsmgr)?.InnerText);
                                Console.SetCursorPosition(50, Console.CursorTop);//最大名は青森東方沖１００ｋｍＡ?

                                var maxTimeSt = info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:DateTime", nsmgr)?.InnerText;
                                if (maxTimeSt != null)
                                    ConsoleWrite(DateTime.Parse(maxTimeSt).ToString("MM/dd HH:mm"));
                                else
                                    ConsoleWrite("--/-- --:--", ConsoleColor.DarkGray);

                                var height = double.Parse(info.SelectSingleNode("jmx_se:MaxHeight/jmx_eb:TsunamiHeight", nsmgr)?.InnerText ?? "-1");
                                if (height < 10)
                                    Console.SetCursorPosition(64, Console.CursorTop);
                                else
                                    Console.SetCursorPosition(63, Console.CursorTop);

                                //小さいとき:[警報以上:観測中　注意報:微弱]  大津波警報の基準を超え、追加あるいは更新;重要　複数の場合全角空白
                                var maxCondition = info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:Condition", nsmgr)?.InnerText.Replace("重要", "").Replace("　", "");

                                ConsoleColor heightColor;
                                if (height >= 10)
                                    heightColor = ConsoleColor.DarkMagenta;
                                else if (height >= 3)
                                    heightColor = ConsoleColor.Magenta;
                                else if (height >= 1)
                                    heightColor = ConsoleColor.DarkRed;
                                else if (height >= 0.2)
                                    heightColor = ConsoleColor.DarkYellow;
                                else if (height >= 0)
                                    heightColor = ConsoleColor.DarkBlue;
                                else if (maxCondition == "観測中")
                                    heightColor = ConsoleColor.DarkYellow;
                                else if (maxCondition == "微弱")
                                    heightColor = ConsoleColor.DarkBlue;
                                else//欠測
                                    heightColor = ConsoleColor.DarkGray;


                                if (height == -1)
                                    ConsoleWrite("-.-m", heightColor);
                                else
                                    ConsoleWrite(height.ToString("#0.0m"), heightColor);
                                //以上(測定範囲超過)の判定用
                                var maxHeightDescription = info.SelectSingleNode("jmx_se:MaxHeight/jmx_eb:TsunamiHeight", nsmgr)?.Attributes?["description"]?.Value ?? "";
                                //上昇中
                                var maxHeightCondition = info.SelectSingleNode("jmx_se:MaxHeight/jmx_eb:TsunamiHeight", nsmgr)?.Attributes?["condition"]?.Value ?? "";

                                if (maxHeightDescription.Contains("以上"))
                                    ConsoleWrite("以上 ", heightColor);
                                else
                                    Console.Write("　　 ");

                                if (maxCondition == "微弱")
                                    ConsoleWrite("微弱", ConsoleColor.DarkBlue);
                                else if (maxCondition == "観測中")
                                    ConsoleWrite("観測中", ConsoleColor.DarkYellow);
                                else if (maxCondition == "欠測")
                                    ConsoleWrite("欠測", ConsoleColor.DarkGray);
                                else
                                    ConsoleWrite(maxCondition);

                                if (maxHeightCondition == "上昇中")
                                    ConsoleWrite("上昇中", ConsoleColor.Yellow);
                                else
                                    ConsoleWrite(maxHeightCondition);

                                var maxRevise = info.SelectSingleNode("jmx_se:MaxHeight/jmx_se:Revise", nsmgr)?.InnerText;
                                Console.SetCursorPosition(80, Console.CursorTop);
                                if (maxRevise != null)
                                    if (maxRevise == "追加")
                                        ConsoleWrite("(追加)", ConsoleColor.Red);
                                    else if (maxRevise == "更新")
                                        ConsoleWrite("(更新)", ConsoleColor.Blue);
                                Console.SetCursorPosition(88, Console.CursorTop);

                                ConsoleWrite("|  ", ConsoleColor.White);

                                var firstTimeSt = info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:ArrivalTime", nsmgr)?.InnerText;
                                var firstCondition = info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:Condition", nsmgr)?.InnerText;
                                //到達時間　第１波識別不能
                                if (firstTimeSt != null)
                                    ConsoleWrite(DateTime.Parse(firstTimeSt).ToString("MM/dd HH:mm"));
                                else if (firstCondition != null)
                                    if (firstCondition == "第１波識別不能")
                                        ConsoleWrite("第１波識別不能", ConsoleColor.DarkGray);
                                    else
                                        ConsoleWrite("--/-- --:--", ConsoleColor.DarkGray);
                                else
                                    ConsoleWrite("--/-- --:--", ConsoleColor.DarkGray);

                                Console.Write("  ");

                                //押し引き
                                var firstInitial = info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:Initial", nsmgr)?.InnerText;
                                if (firstInitial != null)
                                    if (firstInitial == "押し")
                                        ConsoleWrite("押し", ConsoleColor.DarkCyan);
                                    else if (firstInitial == "引き")
                                        ConsoleWrite("引き", ConsoleColor.DarkGreen);


                                var firstRevise = info.SelectSingleNode("jmx_se:FirstHeight/jmx_se:Revise", nsmgr)?.InnerText;
                                Console.SetCursorPosition(109, Console.CursorTop);
                                if (firstRevise != null)
                                    if (firstRevise == "追加")
                                        ConsoleWrite("(追加)", ConsoleColor.Red);
                                    else if (firstRevise == "更新")
                                        ConsoleWrite("(更新)", ConsoleColor.Blue);
                                if (firstCondition == "欠測")
                                    ConsoleWrite(" 欠測", ConsoleColor.DarkGray);

                                ConsoleWrite("", true);
                            }
                        }
                    }
                }
                //throw new Exception("デバック用");
                ConsoleWrite($"\n\n処理が完了しました。({DateTime.Now:HH:mm:ss.ff})\n\n何かキーを押すと再取得します。", true);
                Console.ReadKey();
                goto restart;
            }
            catch (Exception ex)
            {
                //throw;
                ConsoleWrite($"\n\n//////////////////////////////////////////////////\nエラーが発生しました。わからない場合開発者に報告してください。このスクリーンショットがあると助かります。" +
                    $"\n<エラーの例(英語になることがあります)>\nオブジェクト参照がオブジェクト インスタンスに設定されていません。/値をnullにすることはできません:処理ミスです。" +
                    $"\n//////////////////////////////////////////////////\n\n内容:{ex}\n\n何かキーを押すと終了します。", ConsoleColor.Red, true);
                Console.ReadKey();
            }

        }

        public static void ConsoleWrite(string? text, bool line = false)
        {
            ConsoleWrite(text, defaultColor, line);
        }


        public static void ConsoleWrite(string? text, ConsoleColor color, bool line = false)
        {
            Console.ForegroundColor = color;
            if (line)
                Console.WriteLine(text);
            else
                Console.Write(text);
        }
    }
}