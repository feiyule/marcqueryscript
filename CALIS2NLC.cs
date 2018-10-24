using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;


using dp2Circulation;

using DigitalPlatform.Marc;
using DigitalPlatform.Script;

public class CALIS2NLC : MarcQueryHost
{
    public override void OnRecord(object sender, StatisEventArgs e)
    {
        MarcNodeList nodes = null;
        string strContent = "";
        MarcNodeList nodes1 = null;
        MarcNode node2 = null;
        MarcRecord record = this.MarcRecord;

        if (record.Header[5, 4] == "nam ") 
        {
            record.Header[5, 4] = "oam2";
        }
        //有010的
        nodes = record.select("field[@name='010']/subfield[@name='b']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            if (strContent == "光盘")
            {
                node2 = node.Parent;
                if (node2.FirstChild.Name == "a")
                {
                    strContent = node2.FirstChild.Content;

                    //node2.Content = "{cr:CALIS}" + node2.Content;
                    node2.after("307  " + MarcQuery.SUBFLD + "a附光盘：ISBN " + strContent);
                    node2.detach();
                    this.Changed = true;
                }
            }

        }

        //有016的
        nodes = record.select("field[@name='016']/subfield[@name='b']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            if (strContent == "磁带")
            {
                node2 = node.Parent;
                if (node2.FirstChild.Name == "a")
                {
                    strContent = node2.FirstChild.Content;

                    //node2.Content = "{cr:CALIS}" + node2.Content;
                    node2.after("307  {cr:NLC}" + MarcQuery.SUBFLD + "a附磁带：" + strContent);
                    node2.detach();
                    this.Changed = true;
                }
            }

        }

        //有100的
        nodes = record.select("field[@name='100']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            if (strContent.Substring(28,4)=="0120")
            {
 
                node.Content = strContent.Substring(0, 28) + "0110" + strContent.Substring(32, strContent.Length - 32);
                this.Changed = true;

            }

        }

        //都存在的拼音
        nodes = record.select("field[@name='200' or @name='512' or @name='513' or @name='514' or @name='515' or @name='516' or @name='517' or @name='518' or @name='540' or @name='541' or @name='545' or @name='701' or @name='702' or @name='711' or @name='712' or @name='730']/subfield[@name='A']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            node.Name = "9";
            node.Content = node.Content.ToLower();
            this.Changed = true;

           

        }


        //国图不存在的拼音
        nodes = record.select("field[@name='225' or @name='600' or @name='601' or @name='604' or @name='605' or @name='606' or @name='607' or @name='610']/subfield[@name='A']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;


            node.detach();

            this.Changed = true;


        }

        //有200$d的
        nodes = record.select("field[@name='200']/subfield[@name='d']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            if (strContent.StartsWith("= "))
            {
                node.Content = strContent.Remove(0, 2);

                this.Changed = true;

            }

        }

        //有200$f$g的
        nodes = record.select("field[@name='200']/subfield[@name='f' or @name='g']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            if (BiblioItemsHost.ContainHanzi(strContent))
            {
                string strRight = strContent.Replace(",", "，");
                strRight = strRight.Replace(" ", "");
                node.Content = strRight.Replace("...", "");

                this.Changed = true;

            }

        }

        //有205的
        nodes = record.select("field[@name='205']/subfield[@name='a']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            if (strContent.IndexOf("第") >= 0)
            {

                string strRight = strContent.Replace("第", "");
                node.Content = strRight;

                this.Changed = true;
            }

        }

        //有215$a的
        nodes = record.select("field[@name='215']/subfield[@name='a']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            if (strContent.IndexOf(" ") >= 0)
            {

                string strRight = strContent.Replace(" ", "");
                node.Content = strRight;

                this.Changed = true;
            }

        }


        //有215$e的
        nodes = record.select("field[@name='215']/subfield[@name='e']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            if (strContent.IndexOf("光盘") >= 0)
            {

                string strRight = strContent.Replace("光盘", "");
                node.Content = strRight.Replace("片", "光盘");

                this.Changed = true;
            }

        }
        //有215$d的
        nodes = record.select("field[@name='215']/subfield[@name='d']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            if (strContent.IndexOf("x") >= 0)
            {

                string strRight = strContent.Replace("x", "×");
                node.Content =  strRight;

                this.Changed = true;
            }

        }

        //有410的
        nodes = record.select("field[@name='410']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            if (strContent.IndexOf(MarcQuery.SUBFLD + "i") >= 0)
            {
                node.Name = "462";
            }
            else
            {
                node.Name = "461";

            }
            this.Changed = true;
        }


        //有605$a的
        nodes = record.select("field[@name='605']/subfield[@name='a']");
        foreach (MarcNode node in nodes)
        {
            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            if (strContent.IndexOf("《") < 0)
            {

                node.Content = "《" + strContent + "》";

                this.Changed = true;
            }

        }
        
        
        //有600/701/702的
        nodes = record.select("field[@name='600' or @name='701' or @name='702']");
        foreach (MarcNode node in nodes)
        {

            strContent = node.Content;
            if (String.IsNullOrEmpty(strContent))
                continue;

            MarcNodeList subfields = node.select("subfield[@name='g' or @name='f']");
            if (subfields.count > 0)
            {
                foreach (MarcNode node3 in node.ChildNodes)
                {

                    if (node3.Name == "g")
                    {
                        node3.Name = "c";
                    }
                    if (node3.Name == "f")
                    {
                        node3.Content = "(" + node3.Content + ")";
                    }
                }

                for (int i = node.ChildNodes.count-1; i >=0; i--)
                {
                    MarcNode node3 = node.ChildNodes[i];
                    strContent = node3.Content;
                    bool prefix = false;
                    if (node3.Name=="f")
                    {
                        prefix = true;
                    }
                    else
                    {
                        if (prefix)
                        {
                            if (strContent.Substring(strContent.Length-1,1)==",")
                            {
                                node3.Content = strContent.Remove(strContent.Length - 1, 1);
                            }
                            
                        }
                        prefix = false;
                    }
                }

                for (int i = 0; i < node.ChildNodes.count; i++)
                {
                    MarcNode node3 = node.ChildNodes[i];
                    strContent = node3.Content;
                    bool prefix = false;
                    if (strContent.StartsWith("("))
                    {
                        if (prefix)
                        {
                            node3.Content = strContent.Remove(0, 1);
                            strContent = node.ChildNodes[i - 1].Content;
                            node.ChildNodes[i - 1].Content = strContent.Substring(0, strContent.Length - 1);
                        }
                        prefix = true;
                    }
                    else
                    {

                        prefix = false;
                    }
                }

                this.Changed = true;
            }


        }



    }
}
