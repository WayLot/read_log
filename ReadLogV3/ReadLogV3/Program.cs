using MySql.Data.MySqlClient;
using Dapper;
using System.Collections.Concurrent;
using ReadLogV3;
using Newtonsoft.Json;
using System.Data;
using static ReadLogV3.TrackingLog;
using System.Diagnostics;

class Program
{
    private static MySqlConnection connection;
    private static List<TrackingLog.Root> obj;
    private static List<Organization> organization;
    private static List<Course> courses;
    private static List<Section> sections;
    private static List<Video> videos;

    //private static DataTable dt;
    private static DataTable organization_DataTable;
    private static DataTable course_DataTable;
    private static DataTable section_DataTable;
    private static DataTable video_DataTable;

    static readonly BlockingCollection<List<TrackingLog.Root>> DataQueue = new BlockingCollection<List<TrackingLog.Root>>();
    static void Main()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        string ConnectionString = @"Server=127.0.0.2;Port=3306;Database=uni_web_app;Uid=root;Pwd=admin;";
        connection = new MySqlConnection(ConnectionString);
       

        Task taskRead = Task.Run(() =>
        {
            Stopwatch stopwatch1 = Stopwatch.StartNew();
            long memoryUsed = GC.GetTotalMemory(false);
            Console.WriteLine("Memory used after reading log: " + memoryUsed + " bytes");
            ReadLogFile();
            stopwatch1.Stop();
            Console.WriteLine(stopwatch1.Elapsed.ToString());
            ;
        });

        //Console.WriteLine("Thời gian đã qua xử lý: " + elapsedTime);


        organization = connection.Query<Organization>("Select * from Organization").ToList();
        organization_DataTable = new DataTable();
        organization_DataTable.Columns.Add("Id", typeof(int));
        organization_DataTable.Columns.Add("Name", typeof(string));
        foreach (var org in organization)
        {
            organization_DataTable.Rows.Add(org.org_id,org.org_name);
        }
        
        courses = connection.Query<Course>("Select * from Course").ToList();
        course_DataTable = new DataTable();
        course_DataTable.Columns.Add("Id", typeof(int));
        course_DataTable.Columns.Add("Name", typeof(string));
        foreach(var course in courses)
        {
            course_DataTable.Rows.Add(course.cou_id,course.cou_name);
        }

        sections = connection.Query<Section>("Select * from Section").ToList();
        section_DataTable = new DataTable();
        section_DataTable.Columns.Add("Id", typeof(int));
        section_DataTable.Columns.Add("Name", typeof(string));
        foreach(var section in sections)
        {
            section_DataTable.Rows.Add(section.sec_id,section.sec_name);
        }
        videos = connection.Query<Video>("Select * from video").ToList();
        video_DataTable = new DataTable();
        video_DataTable.Columns.Add("Id", typeof(int));
        video_DataTable.Columns.Add("Name", typeof(string));
        video_DataTable.Columns.Add("Video_Duration", typeof(int));
        foreach(var video in videos)
        {
            video_DataTable.Rows.Add(video.video_id, video.video_name, video.video_duration);
        }
        
        connection.Open();

        Task task1 = Task.Run(() =>
        {

            Stopwatch stopwatch1 = Stopwatch.StartNew();
            long memoryUsed2 = GC.GetTotalMemory(false);
            Console.WriteLine("Memory used before insert: " + memoryUsed2 + " bytes");
            InsertDataToMySQL();
            long memoryUsed1 = GC.GetTotalMemory(false);
            Console.WriteLine("Memory used: " + memoryUsed1 + " bytes");
            stopwatch1.Stop();
            Console.WriteLine(stopwatch1.Elapsed.ToString());
        });
        
        Console.WriteLine("Press any key to stop reading the log file...");
        
        Task.WaitAll(taskRead, task1);
        //Task.WhenAll(taskRead, task1).Wait();
        stopwatch.Stop();
    
        Console.WriteLine("Thời gian đã qua xử lý: " + stopwatch.Elapsed);
        Console.ReadKey();


    }
    
    static void ReadLogFile()
    {
        string filePath = @"D:\Source\LogApp\ReadTrackingLog\tracking.log";
        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line.Contains("play_video") || line.Contains("load_video"))
                    {
                        obj = JsonConvert.DeserializeObject<List<TrackingLog.Root>>("[" + line + "]");
                        //dt.Rows.Add(obj[0].name, obj[0].context.org_id, obj[0].context.course_id, obj[0].session, obj[0].Event, obj[0].context.user_id, obj[0].time);
                        DataQueue.Add(obj);
                        Thread.Sleep(10);
                    }
                }
            }
            DataQueue.CompleteAdding();

            Console.WriteLine("Finished reading the log file.");
        }
        catch (ThreadAbortException)
        {
            Console.WriteLine("Log reading thread aborted.");
         }
     }

    public static void InsertDataToMySQL()
    {
        try
        {
            while (!DataQueue.IsCompleted)
            {
                if (DataQueue.TryTake(out var line))
                {

                    InsertToOrganization(line[0].context.org_id);
                    InsertToCourse(line[0].context.course_id);
                    InsertToSection(line[0].session);
                    InsertToVideo(line[0].Event);
                    InsertToData(line);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            
        }
        catch (ThreadAbortException)
        {
            throw;
        }
    }
    public static void InsertToOrganization(string organization_Name)
    {
        var fount = organization_DataTable.Select("Name = '" + organization_Name + "'");
        if (fount.Length != 0)
        {
            return;
        }
        var result = connection.Execute("INSERT INTO `uni_web_app`.`organization`(`org_name`) VALUES ( @org_name)"
            , new Organization { org_name = organization_Name });
        organization_DataTable.Rows.Add(organization_DataTable.Rows.Count + 1, organization_Name);
    }

    public static void InsertToCourse(string course_Name)
    {
        var fount = course_DataTable.Select("Name = '" + course_Name + "'");
        if (fount.Length != 0)
        {
            return;
        }
        var result = connection.Execute(@"INSERT INTO `uni_web_app`.`course` ( `cou_name`) VALUES  (@cou_name)"
            , new Course { cou_name = course_Name });
        course_DataTable.Rows.Add(course_DataTable.Rows.Count + 1, course_Name);
    }

    public static void InsertToSection(string section_Name)
    {
        var fount = section_DataTable.Select("Name = '" + section_Name + "'");
        if (fount.Length != 0)
        {
            return;
        }
        var result = connection.Execute("INSERT INTO `uni_web_app`.`section` (`sec_name`) VALUES ( @sec_name)"
            , new Section { sec_name = section_Name });
        section_DataTable.Rows.Add(section_DataTable.Rows.Count + 1, section_Name);
    }

    public static void InsertToVideo(object event_name)
    {
        var even_Data = JsonConvert.DeserializeObject<List<Event_Data>>("[" + event_name +"]");
        var fount = video_DataTable.Select("Name = '" + even_Data[0].id + "'");
        if (fount.Length != 0)
        {
            return;
        }
        var result = connection.Execute("INSERT INTO `uni_web_app`.`video`(`video_name`,`video_duration`) VALUES ( @video_name, @video_duration)"
                , new Video { video_name = even_Data[0].id, video_duration = even_Data[0].duration });
        video_DataTable.Rows.Add(video_DataTable.Rows.Count +1, even_Data[0].id, even_Data[0].duration);
    }

    public static void InsertToData(List<Root> data)
    {
        List<Video_Infor> dataWithVideoInfor = new List<Video_Infor>();
        var even_Data = JsonConvert.DeserializeObject<List<Event_Data>>("[" + data[0].Event + "]");
        dataWithVideoInfor.Add(new Video_Infor()
        {
            Org_Id = data[0].context.org_id,
            Cou_Id = data[0].context.course_id,
            Sec_Id = data[0].session,
            User_Id = data[0].context.user_id,
            id = even_Data[0].id,
            code = even_Data[0].code,
            duration = even_Data[0].duration,
            currentTime = even_Data[0].currentTime,
            time_create = data[0].time.ToString(),
        });

        var getDataWithID = (from a in dataWithVideoInfor
                             join b in organization_DataTable.AsEnumerable() on a.Org_Id equals b["Name"]
                             join c in course_DataTable.AsEnumerable() on a.Cou_Id equals (string)c["Name"]
                             join d in section_DataTable.AsEnumerable() on a.Sec_Id equals (string)d["Name"]
                             join e in video_DataTable.AsEnumerable() on a.id equals (string)e["Name"]
                             select new
                             {

                                 Organization_Id = (int)b["Id"],
                                 Couse_Id = (int)c["Id"],
                                 Section_Id = (int)d["Id"],
                                 Video_id = (int)e["Id"],
                                 User_Id = a.User_Id,
                                 Duration = a.duration,
                                 Current = a.currentTime,
                                 Create_Time = a.time_create
                             }).ToList();
        var dur = Convert.ToInt32(getDataWithID[0].Duration);
        var cru = Convert.ToDecimal(getDataWithID[0].Current);
        var watchF = (cru / dur) * 100 > 95 ? 1 : 0;
        var block = (cru / 20);
        string query = @"INSERT INTO `uni_web_app`.`data`(`org_id`,`cou_id`,`sec_id`,`sub_id`,`unit_id`,`video_id`,`user_id`,`watched_time`,`first_time_watched`,`fully_watched`,`block`) 
                        VALUES (@org_id, @cou_id, @sec_id, @sub_id,@unit_id, @video_id, @user_id, @watched_time,@first_time_watched, @fully_watched,@block)";
        var result = connection.Execute(query,
            new DataInDataTable
            {
                org_id = getDataWithID[0].Organization_Id,
                cou_id = getDataWithID[0].Couse_Id,
                sec_id = getDataWithID[0].Section_Id,
                sub_id = 1,
                unit_id = 1,
                video_id = getDataWithID[0].Video_id,
                user_id = Convert.ToInt32(getDataWithID[0].User_Id),
                watched_time = getDataWithID[0].Current == null ? 0 : cru,
                first_time_watched = getDataWithID[0].Current == null ? 1 : 0,
                fully_watched = watchF,
                block = block
            });
    }
}

