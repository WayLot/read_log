using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadLogV3
{
    public class TrackingLog
    {
        public class Context
        {
            public string? user_id { get; set; }
            public object? course_user_tags { get; set; }
            public string? path { get; set; }
            public string? course_id { get; set; }
            public string? org_id { get; set; }

        }

        public class Root
        {
            public string? name { get; set; }
            public Context context { get; set; }
            public string? username { get; set; }
            public string? session { get; set; }
            public string? ip { get; set; }
            public string? agent { get; set; }
            public string? host { get; set; }
            public string? referer { get; set; }
            public string? accept_language { get; set; }
            public object? Event { get; set; }
            public DateTime time { get; set; }
            public string? event_type { get; set; }
            public string? event_source { get; set; }
            public object page { get; set; }
        }

        public class Video_Infor
        {
            public string? Org_Id { get; set; }
            public string? Cou_Id { get; set; }
            public string? Sec_Id { get; set; }
            public string? User_Id { get; set; }
            public string? id { get; set; }
            public string? code { get; set; }
            public string? duration { get; set; }
            public string? currentTime { get; set; }
            public string? time_create { get; set; }
        }

        public class Event_Data
        {
            public string? id { get; set; }
            public string? code { get; set; }
            public string? duration { get; set; }
            public string? currentTime { get; set; }
        }

        public class Organization
        {
            public int? org_id { get; set; }
            public string? org_name { get; set; }
        }

        public class Course
        {
            public int? cou_id { get; set; }
            public string? cou_name { get; set; }
        }

        public class Section
        {
            public int? sec_id { get; set; }
            public string? sec_name { get; set; }
        }

        public class Video
        {
            public int? video_id { get; set; }
            public string? video_name { get; set; }
            public string? video_duration { get; set; }
        }

        public class Subsection
        {
            public int? sub_id { get; set; }
            public string? sub_name { get; set; }
        }

        public class Unit
        {
            public int? unit_id { get; set; }
            public string? unit_name { get; set; }
        }

        public class DataInDataTable
        {
            public int? org_id { get; set; }
            public int? cou_id { get; set; }
            public int? sec_id { get; set; }
            public int? sub_id { get; set; }
            public int? unit_id { get; set; }
            public int? video_id { get; set; }
            public int? user_id { get; set; }
            public decimal? watched_time { get; set; }
            public int? first_time_watched { get; set; }
            public int? fully_watched { get; set; }
            public decimal? block { get; set; }
        }
    }
}
