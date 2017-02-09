using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
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
            if (constr == "2")
            {
                MySQLDB.Conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["ObjNullConnection"].ConnectionString);
            }
            else
            {
                MySQLDB.Conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString);
            }

            while (true)
            {
                Console.WriteLine("执行命令：1.FollowBySQL 2.StartReadEvents 3.CheckAPIStatus");
                switch (Console.ReadLine())
                {
                    case "1":
                        FollowBySQL();
                        break;
                    case "2":
                        StartReadEvents();
                        break;
                    case "3":
                        CheckAPIStatus();
                        break;
                }
            }
        }
        
        //用指定机器人关注指定查询的用户
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

        //抓取最新事件
        public static void StartReadEvents()
        {
            Console.WriteLine("输入更新间隔分钟数：");
            int waitmi = int.Parse(Console.ReadLine());
            Console.WriteLine("更新程序已启动，输入任意字符停止。");
            bool stop = false;
            Task.Run(() =>
            {
                while (true)
                {
                    if(stop)
                    {
                        break;
                    }
                    Console.WriteLine("更新中……");
                    int count = 0;
                    //获取每个机器账号的新事件
                    string sql = "select GitHubLogin, GitHubAccessToken from nulluser where Email like '%objnull.com' and GitHubLogin like 'robot%'";
                    foreach (DataRow row in MySQLDB.Query(sql).Rows)
                    {
                        string robot = row[0].ToString();

                        //当前机器人最后更新时间
                        string ledSql = "select CreateDate from githubevent where Robot='" + robot + "' order by CreateDate desc limit 1";
                        DataTable ledDT = MySQLDB.Query(ledSql);
                        DateTime LastEventDate;
                        if (ledDT.Rows.Count == 0)
                        {
                            LastEventDate = DateTime.Now.AddYears(-1);
                        }
                        else
                        {
                            LastEventDate = DateTime.Parse(ledDT.Rows[0][0].ToString());
                        }

                        GitHubClient github = new GitHubClient(new ProductHeaderValue("objnulldotcom"));
                        github.Credentials = new Credentials(row[1].ToString());
                        IReadOnlyList<Activity> acts = github.Activity.Events.GetAllUserReceived(robot).Result;
                        //插入最新事件
                        foreach (Activity act in acts.Where(a => a.CreatedAt.LocalDateTime > LastEventDate))
                        {
                            string id = act.Id;
                            string githubLogin = act.Actor.Login;
                            string avatarUrl = act.Actor.AvatarUrl;
                            DateTime createDate = act.CreatedAt.LocalDateTime;
                            string eventType = act.Type;
                            string repoName = act.Repo.Name;
                            string addSql = "insert into githubevent values('" + id + "','" + githubLogin + "','" + avatarUrl +
                                "',DATE_FORMAT('" + createDate.ToString("yyyy-MM-dd HH:mm:ss") + "','%Y-%m-%d %H:%i:%s'),'" + eventType + "','" + repoName + "','" + robot + "')";
                            MySQLDB.Execute(addSql);
                            count++;
                        }
                    }
                    Console.WriteLine("更新完成，插入" + count + "条数据。下一次更新在" + DateTime.Now.AddMinutes(waitmi).ToString() + "。输入任意字符停止。");
                    Thread.Sleep(new TimeSpan(0, waitmi, 0));
                }
                Console.WriteLine("更新停止");
            });
            Console.ReadLine();
            stop = true;
        }

        //查看API状态
        public static void CheckAPIStatus()
        {
            string sql = "select GitHubLogin, GitHubAccessToken from nulluser where Email like '%objnull.com' and GitHubLogin like 'robot%'";
            foreach (DataRow row in MySQLDB.Query(sql).Rows)
            {
                string robot = row[0].ToString();

                GitHubClient github = new GitHubClient(new ProductHeaderValue("objnulldotcom"));
                github.Credentials = new Credentials(row[1].ToString());
                Console.WriteLine(github.User.Get(robot).Result.Login);
                ApiInfo apiInfo = github.GetLastApiInfo();
                // If the ApiInfo isn't null, there will be a property called RateLimit
                RateLimit rateLimit = apiInfo?.RateLimit;
                int? howManyRequestsCanIMakePerHour = rateLimit?.Limit;
                int? howManyRequestsDoIHaveLeft = rateLimit?.Remaining;
                DateTime? whenDoesTheLimitReset = rateLimit?.Reset.LocalDateTime;
                Console.WriteLine("Can:" + howManyRequestsCanIMakePerHour + " Left:" + howManyRequestsDoIHaveLeft + " Reset:" + whenDoesTheLimitReset);
            }
        }
    }
}
