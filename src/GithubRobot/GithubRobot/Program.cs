using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using Octokit;
using MySql.Data.MySqlClient;

namespace GithubRobot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("选择数据库：1.local 2.objnull");
            string constr = Console.ReadLine();
            if(constr == "2")
            {
                MySQLDB.Conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["ObjNullConnection"].ConnectionString);
            }
            else
            {
                MySQLDB.Conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString);
            }
            Console.WriteLine("命令：1.FollowBySQL 2.Start 3.Stop");
            switch (Console.ReadLine())
            {
                case "1":
                    FollowBySQL();
                    break;
            }
            Console.ReadLine();
        }

        public void temp()
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("MyAmazingApp"));
            github.Credentials = new Credentials("2fee142d6d760e6de72e0d81594607b54de62ae7");

            github.User.Get("robot0x01");

            github.User.Followers.Follow("631320085");

            //IReadOnlyList<Activity> acts = github.Activity.Events.GetAllUserReceived("631320085").Result;
            //foreach(Activity act in acts)
            //{
            //    Console.WriteLine(act.Actor.Login + " " + act.CreatedAt + " " + act.Repo.Name + " " + act.Type + " " + act.Payload.Sender);
            //}

            //ApiInfo apiInfo = github.GetLastApiInfo();
            //// If the ApiInfo isn't null, there will be a property called RateLimit
            //RateLimit rateLimit = apiInfo?.RateLimit;
            //int? howManyRequestsCanIMakePerHour = rateLimit?.Limit;
            //int? howManyRequestsDoIHaveLeft = rateLimit?.Remaining;
            //DateTimeOffset? whenDoesTheLimitReset = rateLimit?.Reset;
            //Console.WriteLine("Can:" + howManyRequestsCanIMakePerHour + " Left:" + howManyRequestsDoIHaveLeft + " Reset:" + whenDoesTheLimitReset);
        }

        public static void FollowBySQL()
        {
            Console.WriteLine("输入SQL：");
            string sql = Console.ReadLine();
            Console.WriteLine("输入Robot：");
            string robot = Console.ReadLine();
            Console.WriteLine("输入Token：");
            string token = Console.ReadLine();

            GitHubClient github = new GitHubClient(new ProductHeaderValue("objnulldotcom"));
            github.Credentials = new Credentials(token);
            github.User.Get(robot);

            Console.WriteLine("关注中……");
            foreach (DataRow row in MySQLDB.Query(sql).Rows)
            {
                github.User.Followers.Follow(row[0].ToString());
            }
            Console.WriteLine("关注完成。");
        }
    }
}
