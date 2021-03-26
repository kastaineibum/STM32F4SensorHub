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
    public class ZAlexFunctions
    {
        public static string serveraddr = "http://127.0.0.1/wp/";
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
        /// 获取自定义短时间戳  13位 更兼容未来（2019年起）
        /// </summary>  
        /// <returns></returns>  
        public static long GetLong3mnStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(2019, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
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
        /// 将时间戳转换为日期类型，并格式化
        /// </summary>
        /// <param name="longDateTime"></param>
        /// <returns></returns>
        private static string Long3mnStampToDateTimeString(string longDateTime)
        {
            //用来格式化long类型时间的,声明的变量
            long unixDate;
            DateTime start;
            DateTime date;
            //ENd

            unixDate = long.Parse(longDateTime);
            start = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);
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

        public static void setBaro1(int baro)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set pressure="+baro+" where mid=" + AlexData.mid;
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
        public static void setHumidity1(int humidity)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set humidity=" + humidity + " where mid=" + AlexData.mid;
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

        public static void setTemperature1(int temperature)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set temperature=" + temperature + " where mid=" + AlexData.mid;
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

        public static void setElevation1(int elevation)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set elevation=" + elevation + " where mid=" + AlexData.mid;
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

        public static void setLiquidlevel1(int liquidlevel)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set liquidlevel=" + liquidlevel + " where mid=" + AlexData.mid;
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

        public static void setBaro2(int baro)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set pressure2=" + baro + " where mid=" + AlexData.mid;
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
        public static void setHumidity2(int humidity)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set humidity2=" + humidity + " where mid=" + AlexData.mid;
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

        public static void setTemperature2(int temperature)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set temperature2=" + temperature + " where mid=" + AlexData.mid;
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

        public static void setElevation2(int elevation)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set elevation2=" + elevation + " where mid=" + AlexData.mid;
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

        public static void setLiquidlevel2(int liquidlevel)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set liquidlevel2=" + liquidlevel + " where mid=" + AlexData.mid;
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

        public static void setPowderwater(int powder,int water)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "insert into yog_make(powder,water,mid) values(" + powder + "," + water + "," + AlexData.mid + ")";
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

        public static int getLastbid()
        {
            int rst = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select top 1 bid from yog_make where mid=" + AlexData.mid + " order by bid desc";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst = reader.GetInt32("bid");
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

        public static void setTpd(int bid,int t, int p,int d)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_make set temperature=" + t + ",pressure=" + p + ",duration=" + d + " where bid=" + AlexData.bid;
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

        public static void setNormaltemp(int normaltemp1,int normaltemp2)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set normaltemp1=" + normaltemp1 + ",normaltemp2=" + normaltemp2 + " where mid=" + AlexData.mid;
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

        public static int getNormaltemp1()
        {
            int rst = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select normaltemp1 from yog_machine where mid=" + AlexData.mid;
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst = reader.GetInt32("normaltemp1");
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

        public static int getNormaltemp2()
        {
            int rst = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select normaltemp2 from yog_machine where mid=" + AlexData.mid;
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst = reader.GetInt32("normaltemp2");
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

        public static DateTime getBrewstartstamp()
        {
            DateTime rst = new DateTime();
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select startstamp from yog_make where bid=" + AlexData.bid;
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst = reader.GetDateTime("startstamp");
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

        public static DateTime getBrewendstamp()
        {
            DateTime rst = new DateTime();
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select endstamp from yog_make where bid=" + AlexData.bid;
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst = reader.GetDateTime("endstamp");
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

        public static void setBrewstartstamp(string stp)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_make set startstamp='" + stp + "' where bid=" + AlexData.bid;
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

        public static void setBrewendstamp(string stp)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_make set endstamp='" + stp + "' where bid=" + AlexData.bid;
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

        public static void setCupcount(int ct)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set cupcount=cupcount+" + ct + " where mid=" + AlexData.mid;
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

        public static void setCupcountdec(int ct)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_machine set cupcount=cupcount-" + ct + " where mid=" + AlexData.mid;
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

        public static int getCupcount()
        {
            int rst = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select cupcount from yog_machine where mid=" + AlexData.mid;
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst = reader.GetInt32("cupcount");
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

        public static void setCleanstartstamp(string stp)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_clean set startstamp='" + stp + "' where cid=" + AlexData.cid;
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

        public static void setCleanendstamp(string stp)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "update yog_clean set endstamp='" + stp + "' where cid=" + AlexData.cid;
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

        public static int getLastcid()
        {
            int rst = 0;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select top 1 cid from yog_clean where mid=" + AlexData.mid + " order by cid desc";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst = reader.GetInt32("cid");
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

        public static void setClean()
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "insert into yog_clean(mid) values(" + AlexData.mid + ")";
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

        public static DateTime getCleanstartstamp()
        {
            DateTime rst = new DateTime();
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select startstamp from yog_clean where cid=" + AlexData.cid;
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst = reader.GetDateTime("startstamp");
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

        public static DateTime getCleanendstamp()
        {
            DateTime rst = new DateTime();
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "select endstamp from yog_clean where cid=" + AlexData.cid;
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rst = reader.GetDateTime("endstamp");
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



    }
}
