using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Script;
//using DigitalPlatform.CommonControl;	// Field856Dialog
using dp2Circulation;
//using dp2Catalog;

public class CALIS2NLC : MarcQueryHost
{
    static string strStartCode;// = "01344113";
    static string strStartEx;// = strStartCode.Substring(0, 2);
    static string strStartEnd = "";
    static int intAutoNo; //= int.Parse(strStartCode.Substring(2, strStartCode.Length - 2));

    public override void OnBegin(object sender, StatisEventArgs e)
    {
  
    
    }
    
    public override void OnRecord(object sender, StatisEventArgs e)
    {
        MarcNodeList nodes = null;
        string strContent = "";
        MarcNodeList nodes1 = null;
        MarcRecord record = this.MarcRecord;
        string strClc = "";
        string strNumber = "";
        nodes = record.select("field[@name='690']/subfield[@name='a']");
        foreach (MarcNode node in nodes)
        {
            strClc = node.Content;
            break;
        }

        nodes = record.select("field[@name='905']/subfield[@name='f']");
        if (nodes.count == 0)
        {
            string strError = "";

            string strAuthor = "";
            List<string> results = null;
            nodes1 = record.select("field[@name='701']/subfield[@name='a']");

            if (nodes1.count > 0)
            {
                goto FOUND;
            }
            nodes1 = record.select("field[@name='711']/subfield[@name='a']");

            if (nodes1.count > 0)
            {
                goto FOUND;
            }
            nodes1 = record.select("field[@name='200']/subfield[@name='a']");

            if (nodes1.count > 0)
            {
                goto FOUND;
            }
        FOUND:
            foreach (MarcNode node in nodes1)
            {
                strContent = node.Content;

                if (BiblioItemsHost.ContainHanzi(strContent))
                {
                    strAuthor = strContent;


                    

                    // 获得四角号码著者号
                    // return:
                    //      -1  error
                    //      0   canceled
                    //      1   succeed
                    int nRet = GetSjhmAuthorNumber(
                    strAuthor,
                   out strNumber,
                   out strError);
                    if (nRet != 1)
                    {


                    }
                }
                break;
            }
        }
        else 
        {
            return;  
        }




        nodes = record.select("field[@name='905']/subfield[@name='a']");

       // strStartEnd = String.Format("{0:000000}", intAutoNo);
       // intAutoNo = intAutoNo + 1;
        if (nodes.count > 0)
        {
            nodes[0].after(MarcQuery.SUBFLD + "d" + strClc +  "/" + strNumber);
        }
       
        nodes = record.select("field[@name='906']/subfield[@name='a']");
        if (nodes.count > 0)
        {
          foreach (MarcNode node in nodes)
          {
             node.after(MarcQuery.SUBFLD + "d" + strClc +  "/" + strNumber);
          }            
        }
        this.Changed = true;

    }



    /// <summary>
    /// 四角号码基础类
    /// </summary>
    public class QuickSjhm
    {
        XmlDocument dom = null;

        public QuickSjhm(string strFileName)
        {
            dom = new XmlDocument();
            dom.Load(strFileName);
        }

        // 获得四角号码
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetSjhm(string strHanzi,
            out string strSjhm,
            out string strError)
        {
            strSjhm = "";
            strError = "";

            if (dom == null)
            {
                strError = "尚未装载四角号码文件内容";
                return -1;
            }

            XmlNode node = dom.DocumentElement.SelectSingleNode("p[@h='" + strHanzi + "']");
            if (node == null)
                return 0;
            strSjhm = DomUtil.GetAttr(node, "s");
            return 1;
        }
    }
    // 对即将取四角号码的著者字符串进行预加工。例如去掉所有非汉字字符
    /// <summary>
    /// 对即将取四角号码的著者字符串进行预加工。例如去掉所有非汉字字符
    /// </summary>
    /// <param name="strAuthor">源字符串</param>
    /// <param name="strResult">返回结果字符串</param>
    /// <param name="strError">返回出错信息</param>
    /// <returns>-1: 出错; 0: 正常</returns>
    public static int PrepareSjhmAuthorString(string strAuthor,
        out string strResult,
        out string strError)
    {
        strResult = "";
        strError = "";

        // string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";

        for (int i = 0; i < strAuthor.Length; i++)
        {
            char ch = strAuthor[i];

            if (StringUtil.IsHanzi(ch) == false)
                continue;

            // 看看是否特殊符号
            if (StringUtil.SpecialChars.IndexOf(ch) != -1)
            {
                continue;
            }

            // 汉字
            strResult += ch;
        }

        return 0;
    }

    // 获得著者号 -- 四角号码
    // return:
    //      -1  error
    //      0   canceled
    //      1   succeed
    /// <summary>
    /// 获得字叫号码著者号。本函数可以被脚本重载
    /// </summary>
    /// <param name="strAuthor">著者字符串</param>
    /// <param name="strAuthorNumber">返回著者号</param>
    /// <param name="strError">返回出错信息</param>
    /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
    public int GetSjhmAuthorNumber(string strAuthor,
        out string strAuthorNumber,
        out string strError)
    {
        strError = "";
        strAuthorNumber = "";

        string strResult = "";
        int nRet = PrepareSjhmAuthorString(strAuthor,
        out strResult,
        out strError);
        if (nRet == -1)
            return -1;
        if (String.IsNullOrEmpty(strResult) == true)
        {
            strError = "著者字符串 '" + strAuthor + "' 里面没有包含有效的汉字字符";
            return -1;
        }

        List<string> sjhms = null;
        // 把字符串中的汉字转换为四角号码
        // parameters:
        //      bLocal  是否从本地获取四角号码
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        nRet = HanziTextToSjhm(
            true,
            strResult,
            out sjhms,
            out strError);
        if (nRet != 1)
            return nRet;

        if (strResult.Length != sjhms.Count)
        {
            strError = "著者字符串 '" + strResult + "' 里面的字符数(" + strResult.Length.ToString() + ")和取四角号码后的结果事项个数 " + sjhms.Count.ToString() + " 不符";
            return -1;
        }

        // 1，著者名称为一字者，取该字的四角号码。如：肖=9022
        if (strResult.Length == 1)
        {
            strAuthorNumber = sjhms[0].Substring(0, 1)+sjhms[0].Substring(2, 2);
            return 1;
        }
        // 2，著者名称为二字者，分别取两个字的左上角和右上角。如：刘翔=0287
        if (strResult.Length == 2)
        {
            strAuthorNumber = sjhms[0].Substring(0, 1) + sjhms[1].Substring(2, 2);
            return 1;
        }

        // 3，著者名称为三字者，依次取首字左上、右上两角和后两字的左上角。如：罗贯中=6075
        if (strResult.Length >= 3)
        {
            strAuthorNumber = sjhms[0].Substring(0, 1)
                + sjhms[1].Substring(2, 1)
                + sjhms[2].Substring(2, 1);
            return 1;
        }

        // 4，著者名称为四字者，依次取各字的左上角。如：中田英寿=5645
        // 5，五字及以上字数者，均以前四字取号，方法同上。如：奥斯特洛夫斯基=2423
        //if (strResult.Length >= 4)
        //{
        //    strAuthorNumber = sjhms[0].Substring(0, 1)
        //        + sjhms[1].Substring(0, 1)
        //        + sjhms[2].Substring(0, 1)
        //        + sjhms[3].Substring(0, 1);
        //    return 1;
        //}

        strError = "error end";
        return -1;
    }
    // 把字符串中的汉字转换为四角号码
    // parameters:
    //      bLocal  是否从本地获取四角号码
    // return:
    //      -1  出错
    //      0   用户希望中断
    //      1   正常
    public int HanziTextToSjhm(
        bool bLocal,
        string strText,
        out List<string> sjhms,
        out string strError)
    {
        strError = "";
        sjhms = new List<string>();

        // string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";


        for (int i = 0; i < strText.Length; i++)
        {
            char ch = strText[i];

            if (StringUtil.IsHanzi(ch) == false)
                continue;

            // 看看是否特殊符号
            if (StringUtil.SpecialChars.IndexOf(ch) != -1)
            {
                continue;
            }

            // 汉字
            string strHanzi = "";
            strHanzi += ch;


            string strResultSjhm = "";

            int nRet = 0;

            if (bLocal == true)
            {
                nRet = this.MainForm.LoadQuickSjhm(true, out strError);
                if (nRet == -1)
                    return -1;
                nRet = this.MainForm.QuickSjhm.GetSjhm(
                    strHanzi,
                    out strResultSjhm,
                    out strError);
            }
            else
            {
                throw new Exception("暂不支持从拼音库中获取四角号码");
                /*
                nRet = GetOnePinyin(strHanzi,
                     out strResultPinyin,
                     out strError);
                 * */
            }
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {	// canceled
                return 0;
            }

            Debug.Assert(strResultSjhm != "", "");

            strResultSjhm = strResultSjhm.Trim();
            sjhms.Add(strResultSjhm);
        }

        return 1;   // 正常结束
    }
    /// <summary>


}
/// clsInputBox 的摘要说明。
/// </summary>
public class InputBox : System.Windows.Forms.Form
{
    private System.Windows.Forms.TextBox txtData;
    private System.Windows.Forms.Label lblInfo;
    private System.ComponentModel.Container components = null;

    private InputBox()
    {
        InitializeComponent();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (components != null)
            {
                components.Dispose();
            }

        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {

        this.txtData = new System.Windows.Forms.TextBox();
        this.lblInfo = new System.Windows.Forms.Label();
        this.SuspendLayout();

        // 
        // txtData
        // 

        this.txtData.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular,
                                                    System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
        this.txtData.Location = new System.Drawing.Point(19, 8);
        this.txtData.Name = "txtData";
        this.txtData.Size = new System.Drawing.Size(317, 23);
        this.txtData.TabIndex = 0;
        this.txtData.Text = "";
        this.txtData.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtData_KeyDown);

        // 
        // lblInfo
        // 

        this.lblInfo.BackColor = System.Drawing.SystemColors.Info;
        this.lblInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.lblInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.lblInfo.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular,
                                                    System.Drawing.GraphicsUnit.Point, ((System.Byte)(134)));
        this.lblInfo.ForeColor = System.Drawing.Color.Gray;
        this.lblInfo.Location = new System.Drawing.Point(19, 32);
        this.lblInfo.Name = "lblInfo";
        this.lblInfo.Size = new System.Drawing.Size(317, 16);
        this.lblInfo.TabIndex = 1;
        this.lblInfo.Text = "[Enter]确认 | [Esc]取消";

        // 
        // InputBox
        // 

        this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
        this.ClientSize = new System.Drawing.Size(350, 48);
        this.ControlBox = false;
        this.Controls.Add(this.lblInfo);
        this.Controls.Add(this.txtData);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.Name = "InputBox";
        this.Text = "InputBox";
        this.ResumeLayout(false);
    }

    //对键盘进行响应
    private void txtData_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            this.Close();
        }

        else if (e.KeyCode == Keys.Escape)
        {
            txtData.Text = string.Empty;
            this.Close();
        }

    }

    //显示InputBox
    public static string ShowInputBox(string Title, string keyInfo)
    {
        InputBox inputbox = new InputBox();
        inputbox.Text = Title;
        if (keyInfo.Trim() != string.Empty)
            inputbox.lblInfo.Text = keyInfo;
        inputbox.ShowDialog();

        return inputbox.txtData.Text;
    }
}

