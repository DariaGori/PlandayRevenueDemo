using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Domain
{
    public class CreateEmployeeRequestDto
    {
        [JsonProperty("hiredFrom")]
        public String HiredFrom { get; set; }
        [JsonProperty("gender")]
        public String Gender { get; set; }
        [JsonProperty("primaryDepartmentId")]
        public int? PrimaryDepartmentId { get; set; }
        [JsonProperty("jobTitle")]
        public String JobTitle { get; set; }
        [JsonProperty("employeeTypeId")]
        public int? EmployeeTypeId { get; set; }
        [JsonProperty("bankAccount")]
        public BankAccount BankAccount { get; set; }
        [JsonProperty("salaryIdentifier")]
        public String SalaryIdentifier { get; set; }
        [JsonProperty("firstName")]
        public String FirstName { get; set; }
        [JsonProperty("lastName")]
        public String LastName { get; set; }
        [JsonProperty("userName")]
        public String UserName { get; set; }
        [JsonProperty("cellPhone")]
        public String CellPhone { get; set; }
        [JsonProperty("birthDate")]
        public String BirthDate { get; set; }
        [JsonProperty("ssn")]
        public String Ssn { get; set; }
        [JsonProperty("cellPhoneCountryCode")]
        public String CellPhoneCountryCode { get; set; }
        [JsonProperty("street1")]
        public String Address1 { get; set; }
        [JsonProperty("street2")]
        public String Address2 { get; set; }
        [JsonProperty("zip")]
        public String Zip { get; set; }
        [JsonProperty("phone")]
        public String Phone { get; set; }
        [JsonProperty("phoneCountryCode")]
        public String PhoneCountryCode { get; set; }
        [JsonProperty("city")]
        public String City { get; set; }
        [JsonProperty("email")]
        public String Email { get; set; }
        [JsonProperty("departments")]
        public int[] DepartmentIds { get; set; }
        [JsonProperty("employeeGroups")]
        public int[] EmployeeGroupIds { get; set; }
    }
}
