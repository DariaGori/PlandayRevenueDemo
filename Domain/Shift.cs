using Newtonsoft.Json;
using System;

namespace Domain
{
    public class Shift
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("departmentId")]
        public int DepartmentId { get; set; }
        [JsonProperty("employeeId")]
        public int EmployeeId { get; set; }
        [JsonProperty("employeeGroupId")]
        public int EmployeeGroupId { get; set; }
        [JsonProperty("positionId")]
        public int PositionId { get; set; }
        [JsonProperty("shiftTypeId")]
        public int ShiftTypeId { get; set; }
        [JsonProperty("punchClockShiftId")]
        public int PunchClockShiftId { get; set; }
        [JsonProperty("date")]
        public String Date { get; set; }
        [JsonProperty("startDateTime")]
        public String StartDateTime { get; set; }
        [JsonProperty("endDateTime")]
        public String EndDateTime { get; set; }
        [JsonProperty("comment")]
        public String Comment { get; set; }
        [JsonProperty("status")]
        public String Status { get; set; }
        [JsonProperty("dateTimeCreated")]
        public String DateTimeCreated { get; set; }
        [JsonProperty("dateTimeModified")]
        public String DateTimeModified { get; set; }
    }
}
