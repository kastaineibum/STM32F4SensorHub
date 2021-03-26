using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO.Ports;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Data;

namespace STMCounterFormServer
{
    public class ZMoridaFunctions
    {
        public static string serveraddr = "http://192.168.1.2/wp/";
        public static string connStr = "server=127.0.0.1;port=3306;user=root;password=ll123456!;database=wp503;";

        /// <summary>  
        /// 获取时间戳  13位
        /// </summary>  
        /// <returns></returns>  
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds * 1000);
        }

        /// <summary>  
        /// 获取自定义短时间戳  10位 更兼容未来（2019年起）
        /// </summary>  
        /// <returns></returns>  
        public static long GetShort3mnStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(2019, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        /// <summary>
        /// 将时间戳转换为日期类型，并格式化
        /// </summary>
        /// <param name="longDateTime"></param>
        /// <returns></returns>
        private static string LongDateTimeToDateTimeString(string longDateTime)
        {
            //用来格式化long类型时间的,声明的变量
            long unixDate;
            DateTime start;
            DateTime date;
            //ENd

            unixDate = long.Parse(longDateTime);
            start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            date = start.AddMilliseconds(unixDate).ToLocalTime();

            return date.ToString("yyyy-MM-dd HH:mm:ss");

        }

        /// <summary> 
        /// 获取时间戳 10位
        /// </summary> 
        /// <returns></returns> 
        public static long GetTimeStampTen()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public static string GetPostContent(string pageUrl, string postData, string tmethod = "POST")
        {
            if (serveraddr.Contains("https://")) return HttpsGetPostContent(pageUrl, postData);
            string strMsg = string.Empty;
            try
            {
                byte[] requestBuffer = System.Text.Encoding.GetEncoding("gb2312").GetBytes(postData);

                WebRequest request = WebRequest.Create(serveraddr + pageUrl);
                request.Method = tmethod;
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = requestBuffer.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(requestBuffer, 0, requestBuffer.Length);
                    requestStream.Close();
                }

                WebResponse response = request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("gb2312")))
                {
                    strMsg = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch
            { }
            return strMsg;
        }
        delegate bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors);
        public static string HttpsGetPostContent(string pageUrl, string postData)
        {
            CheckValidationResult tf = delegate { return true; };
            string strMsg = string.Empty;
            HttpWebRequest request = null;
            //HTTPSQ请求  
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(tf);
                request = WebRequest.Create(serveraddr + pageUrl) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                //如果需要POST数据    
                byte[] data = System.Text.Encoding.GetEncoding("gb2312").GetBytes(postData);
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (Stream stream = response.GetResponseStream())
                {//获取响应的字符串流  
                    StreamReader sr = new StreamReader(stream, Encoding.GetEncoding("gb2312")); //创建一个stream读取流  
                    strMsg = sr.ReadToEnd();
                }
            }
            catch
            {
            }
            return strMsg;
        }

        public static string PostJson(string Url, string jsonParas)
        {
            string strURL = Url;

            //创建一个HTTP请求 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
            //Post请求方式 
            request.Method = "POST";
            //内容类型
            request.ContentType = "application/x-www-form-urlencoded";

            //设置参数，并进行URL编码 

            string paraUrlCoded = jsonParas;//System.Web.HttpUtility.UrlEncode(jsonParas);   

            byte[] payload;
            //将Json字符串转化为字节 
            payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
            //设置请求的ContentLength  
            request.ContentLength = payload.Length;
            //发送请求，获得请求流 

            Stream writer;
            try
            {
                writer = request.GetRequestStream();//获取用于写入请求数据的Stream对象
            }
            catch (Exception)
            {
                writer = null;
                //Console.Write("连接服务器失败!");
            }
            //将请求参数写入流
            writer.Write(payload, 0, payload.Length);
            writer.Close();//关闭请求流

            String strValue = "";//strValue为http响应所返回的字符流
            HttpWebResponse response;
            try
            {
                //获得响应流
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;
            }

            Stream s = response.GetResponseStream();
            //Stream postData = Request.InputStream;
            StreamReader sRead = new StreamReader(s);
            string postContent = sRead.ReadToEnd();
            sRead.Close();

            return postContent;
        }

        public static string TestReadWpusers()
        {
            string rst = "";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select * from wp_users";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst += (reader.GetInt32("ID") + "|" + reader.GetString("user_login") + "|" + reader.GetString("user_email") + "\r\n");
                }

            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return rst;
        }

        public static string TestReadTemplates()
        {
            string rst = "";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select distinct templateid,templatename from med_template";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst += (reader.GetInt32("templateid") + "^" + reader.GetString("templatename") + "|");
                }

            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return rst;
        }

        public static void TestReadMysql()
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select * from wp_users";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    //Console.WriteLine(reader[0].ToString() + reader[1].ToString() + reader[2].ToString());
                    //Console.WriteLine(reader.GetInt32(0)+reader.GetString(1)+reader.GetString(2));
                    Debug.WriteLine(reader.GetInt32("id") + reader.GetString("arg0") + reader.GetString("arg1"));//"id"是数据库对应的列名
                }

            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        public static void TestWriteMysql()
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "insert into dta_args(arg0,arg1) values('abc','123')";
                //string sql = "delete from user where userid='9'";
                //string sql = "update user set username='啊哈',password='123' where userid='8'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int result = cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        public static void TestTransactMysql()
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            MySqlTransaction transaction = conn.BeginTransaction();
            try
            {
                string sql1 = "insert into dta_args(arg1) values('')";
                MySqlCommand cmd1 = new MySqlCommand(sql1, conn);
                cmd1.ExecuteNonQuery();

                string sql2 = "insert into dta_args(arg1) values('')";
                MySqlCommand cmd2 = new MySqlCommand(sql2, conn);
                cmd2.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
                transaction.Rollback();
                conn.Close();
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                {
                    transaction.Commit();
                    conn.Close();
                }
            }
        }

        public static int ExecuteNonQuery(string sql)
        {
            int result = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static string getTemplateNames()
        {
            string rst = "";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select distinct templatename from med_template";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst += (reader.GetString("templatename") + "|");
                }

            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return rst;
        }

        public static string getTemplateData(string tempname)
        {
            string rst = "";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select timepoint,ele from med_template where templatename='"+tempname+"' order by timepoint asc";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst += (reader.GetInt32("timepoint") + "^"+ reader.GetInt32("ele") + "|");
                }

            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return rst;
        }

        public static int getWantedElevation(string tempname,int timepoint)
        {
            int rst = -1;

            int tp1 = 0;
            int tp2 = 0;
            int el1 = 0;
            int el2 = 0;

            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select timepoint,ele from med_template where templatename='" + tempname + "' and timepoint<="+ timepoint + " order by timepoint desc";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tp1 = reader.GetInt32("timepoint");
                    el1 = reader.GetInt32("ele");

                    break;
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }

            try
            {
                conn.Open();
                string sql2 = "select timepoint,ele from med_template where templatename='" + tempname + "' and timepoint>" + timepoint + " order by timepoint asc";
                MySqlCommand cmd2 = new MySqlCommand(sql2, conn);
                MySqlDataReader reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    tp2 = reader2.GetInt32("timepoint");
                    el2 = reader2.GetInt32("ele");

                    break;
                }

            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }

            if (tp2 == 0)
            {
                rst = -2;
            }
            else if (tp2 - tp1 != 0)
            {
                rst = (timepoint - tp1) * (el2 - el1) / (tp2 - tp1) + el1;
            }
            else
            {
                rst = -1;
            }

            return rst;
        }

        public static string getUserFullname(string usertel)
        {
            string rst = "";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select fullname from med_userext where tel='" + usertel + "'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst += (reader.GetString("fullname"));
                    break;
                }

            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return rst;
        }

        public static long getEndtempstamp(string tempname,long starttempstamp)
        {
            long rst = -1;
            int finaltp = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select timepoint,ele from med_template where templatename='" + tempname + "' order by timepoint desc";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    finaltp = reader.GetInt32("timepoint");
                    break;
                }

            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            rst = starttempstamp + finaltp;
            return rst;
        }

        public static int getTemplateid(string tempname)
        {
            int rst = -1;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select templateid from med_template where templatename='" + tempname + "'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst = reader.GetInt32("templateid");
                    break;
                }

            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return rst;
        }

        public static int getLatestOptlogid(string deviceid)
        {
            int rst = -1;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select optlogid from med_optlog where deviceid='" + deviceid + "' order by optlogid desc";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst = reader.GetInt32("optlogid");
                    break;
                }

            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return rst;
        }

        public static int getHighestTemplateid()
        {
            int rst = -1;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select templateid from med_template order by templateid desc";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst = reader.GetInt32("templateid");
                    break;
                }

            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return rst;
        }

        public static int addOptlog(int time,string templatename,string deviceid,string usertel,int ele=0)
        {
            int result = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "insert into med_optlog(time,templateid,templatename,ele,deviceid,tel,fullname) values( "+time.ToString()+","+ 
                    getTemplateid(templatename) + " ,'"+templatename+"',"+ele+" ,'"+deviceid+"','"+usertel+"','"+ getUserFullname(usertel) + "')";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static int addDevicelog(int ele,int baro,int temp,int humi,string usertel,string fullname,int optlogid)
        {
            int result = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "insert into med_devicelog(ele,baro,temp,humi,tel,fullname,optlogid) values( "+ele+","+baro+" ,"+temp+" ,"+humi+" ,'"+usertel+"','"+fullname+"', "+optlogid+")";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static int addSimpleTemp(int time,int ele)
        {
            int result = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            int curtempid = getHighestTemplateid() + 1;
            try
            {
                conn.Open();
                string sql = "insert into med_template(templateid,timepoint,ele,templatename) values(" + curtempid + ",60," + ele + ",'" +
                    "--" + time/60 + "min" + ele + "meter" + "')";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result += cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            int finaltp2 = time - 60 - Convert.ToInt32(ele * 0.12);
            try
            {
                conn.Open();
                string sql = "insert into med_template(templateid,timepoint,ele,templatename) values(" + curtempid + ","+ finaltp2 + "," + ele + ",'" +
                    "--" + time/60 + "min" + ele + "meter" + "')";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result += cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            try
            {
                conn.Open();
                string sql = "insert into med_template(templateid,timepoint,ele,templatename) values(" + curtempid + "," + time + ",0,'" +
                    "--" + time / 60 + "min" + ele + "meter" + "')";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result += cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static int updateOptlogtime(int optlogid, int time)
        {
            int result = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update med_optlog set time="+time.ToString()+" where optlogid="+optlogid;
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static int deleteSimpleTemp(string tempname)
        {
            int result = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "delete from med_template where templatename='"+tempname+"'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static int deleteSimpleTemp()
        {
            int result = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "delete from med_template where templatename like '--%'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

    }
}
