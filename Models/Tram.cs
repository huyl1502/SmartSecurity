using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSecurity.Models
{
    public class Tram
    {
        public int Id { get; set; }
        public int Area_Id { get; set; }
        public int Group_Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longtitude { get; set; }
        public int? TimeZone { get; set; }
        public DateTime? CreateDay { get; set; }
        public DateTime? UpdateDay { get; set; }
        public int? CreateBy { get; set; }
        public int? UpdateBy { get; set; }
        public bool IsActive { get; set; }
        public int DeviceId { get; set; }

    }
}
