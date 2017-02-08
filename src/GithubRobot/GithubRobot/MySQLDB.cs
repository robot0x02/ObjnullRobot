using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;

namespace GithubRobot
{
    static class MySQLDB
    {
        public static MySqlConnection Conn { get; set; }

        public static DataTable Query(string sql)
        {
            try
            {
                Conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, Conn);
                DataSet dataset = new DataSet();
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                adapter.Fill(dataset);
                cmd.Dispose();
                return dataset.Tables[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine("异常：" + ex.Message);
                return null;
            }
            finally
            {
                Conn.Dispose();
            }
        }

        public static void Execute(string sql)
        {
            try
            {
                Conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, Conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("异常：" + ex.Message);
            }
            finally
            {
                Conn.Dispose();
            }
        }
    }
}
