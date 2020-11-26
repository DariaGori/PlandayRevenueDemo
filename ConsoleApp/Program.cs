using Domain;
using Domain.Configuration;
using Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Domain.Contract;
using Domain.RequestModel;
using Domain.ResponseModel;

namespace ConsoleApp
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; } = default!;

        private const string AuthorizationUrl = "https://id.planday.com";
        private const string PlandayUrl = "https://openapi.planday.com";

        private const string DepartmentsUrl = "hr/v1.0/departments";
        private const string CreateRevenueUrl = "revenue/v1.0/revenue";
        private const string RevenueUnitsUrl = "revenue/v1.0/revenueunits";
        private const string CreateEmployeeUrl = "hr/v1.0/employees";
        private const string EmployeeGroupsUrl = "hr/v1.0/employeegroups";
        private const string RefreshTokenUrl = "connect/token";

        private const string AuthorizationGrantType = "refresh_token";
        private static string _accessToken = "";
        
        private static int _departmentId, _employeeGroupId;
        private static List<int> _departmentIds = new List<int>();
        private static List<int> _employeeGroupIds = new List<int>();
        private static List<int> _revenueUnitIds = new List<int>();
        
        private const string EmailRegexp = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@"
                                           + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";

        private const string DateRegexp = @"^\d{4}-((0[1-9])|(1[012]))-((0[1-9]|[12]\d)|3[01])$";
        private const string LiteralSpaceStringRegexp = @"^[0-9A-Za-z ]+$";

        static void Main()
        {
            var input = "";
            RefreshAccessToken();
            Console.WriteLine("You have successfully gained access to Planday! Press ENTER to continue");
            Console.ReadLine();
            
            do
            {
                MainAsync();
                Console.WriteLine("---------------------------------------");
                Console.WriteLine("Press ENTER to continue or 'X' to exit");
                input = Console.ReadLine();
            } while (input != "X");
            
            Console.ReadLine();
        }

        static async void MainAsync()
        {
            int userInput;
                
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Please select an action: ");
            Console.WriteLine("1) Create a department");
            Console.WriteLine("2) Get available departments");
            Console.WriteLine("3) Create an employee group");
            Console.WriteLine("4) Get available employee groups");
            Console.WriteLine("5) Create an employee");
            Console.WriteLine("6) Check out available revenue units");
            Console.WriteLine("7) Create a revenue record");
            Console.WriteLine("X) Exit");
            Console.WriteLine("--------------------------------");

            (userInput, _) = UserInputHelper.GetUserIntInput(
                "Please choose your action (or X to exit): ", 1, 7, "x");

            switch (userInput)
            {
                case 1:
                    var department = await CreateDepartment();
                    if (department != null) _departmentId = department.Id;
                    break;
                case 2:
                    GetAvailableRecords<Department>(_departmentIds, DepartmentsUrl);
                    break;
                case 3:
                    var employeeGroup = await CreateEmployeeGroup();
                    if (employeeGroup != null) _employeeGroupId = employeeGroup.Id;
                    else Console.WriteLine("Employee Group creation was cancelled!");
                    break;
                case 4:
                    GetAvailableRecords<EmployeeGroup>(_employeeGroupIds, EmployeeGroupsUrl);
                    break;
                case 5:
                    var employee = await CreateEmployee();
                    if (employee == null) Console.WriteLine("Employee creation was cancelled!");
                    break;
                case 6:
                    GetAvailableRecords<RevenueUnit>(_revenueUnitIds, RevenueUnitsUrl);
                    break;
                case 7:
                    var revenue = await CreateRevenue();
                    Console.WriteLine(revenue == null ? "Revenue unit creation was cancelled!" : 
                        "Revenue item for revenue unit " + revenue.RevenueUnitId + " was successfully created");
                    break;
                default:
                    Console.WriteLine("Closing down...");
                    break;
            }
        }

        static async void GetAvailableRecords<T>(ICollection<int> idList, string url)
        where T : class, IResponseData
        {
            try
            {
                var records = (await GetAsync<GetAllModel<T>>(url)).DataUnits;
                if (records == null)
                {
                    Console.WriteLine("No records found!");
                    return;
                }
                
                foreach (var record in records)
                {
                    Console.WriteLine($"{record.Id} - {record.Name}");
                    idList.Add(record.Id);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        static async Task<Revenue?> CreateRevenue()
        {
            var date = UserInputHelper.GetUserStringInput("Please enter the date the Revenue was generated or '0' to exit", 
                10, "0", DateRegexp);
            if (date == "0") return null;
            
            var description = UserInputHelper.GetUserStringInput("Please enter a description for the Revenue or '0' to exit", 
                10, "0");
            if (description == "0") return null;

            bool wasCancelled;
            double turnover;
            (turnover, wasCancelled) = UserInputHelper.GetUserDoubleInput(
                "Please enter the value for the turnover or 'X' to exit", 0, Double.MaxValue, "x");
            if (wasCancelled) return null;
            
            var revenueDto = new CreateRevenueModel()
            {
                Description = description,
                Date = date,
                Turnover = turnover
            };

            try
            {
                (revenueDto.RevenueUnitId, wasCancelled) = UserInputHelper.GetUserIntInput(
                    "Please enter your Revenue Unit ID or 'X' to exit", 0, Int32.MaxValue, "x", 
                    _revenueUnitIds);
                if (wasCancelled) return null;
                
                return (await PostJsonAsync<PostModel<Revenue>>(CreateRevenueUrl, 
                    JsonConvert.SerializeObject(revenueDto))).Data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        static async void RefreshAccessToken()
        {
            var config = GetConfig();

            FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["client_id"] = config.XClientId,
                ["grant_type"] = AuthorizationGrantType,
                ["refresh_token"] = config.RefreshToken
            });

            try
            {
                var authCredentials = await PostUrlEncodedAsync<AuthorizationModel>(RefreshTokenUrl, content);
                _accessToken = authCredentials.AccessToken;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        
        static async Task<Employee?> CreateEmployee()
        {
            var wasCancelled = false;

            var username = UserInputHelper.GetUserStringInput("Please enter your username or '0' to exit", 
                50, "0", EmailRegexp);
            if (username == "0") return null;
            
            var firstName = UserInputHelper.GetUserStringInput("Please enter your first name or '0' to exit", 
                50, "0", LiteralSpaceStringRegexp);
            if (firstName == "0") return null;
            
            var lastName = UserInputHelper.GetUserStringInput("Please enter your last name or '0' to exit", 
                50, "0", LiteralSpaceStringRegexp);
            if (lastName == "0") return null;

            if (_departmentId == 0)
                (_departmentId, wasCancelled) =
                    UserInputHelper.GetUserIntInput("Please enter department ID value (or X to exit)", 1,
                        int.MaxValue, "x", _departmentIds);
            if (wasCancelled) return null;

            if (_employeeGroupId == 0)
                (_employeeGroupId, wasCancelled) =
                    UserInputHelper.GetUserIntInput("Please enter employee group ID value (or X to exit)", 1,
                        int.MaxValue, "x", _employeeGroupIds);
            if (wasCancelled) return null;

            var employeeDto = new CreateEmployeeModel()
            {
                Gender = Gender.Male.ToString(),
                PrimaryDepartmentId = _departmentId,
                EmployeeTypeId = null,
                FirstName = firstName,
                LastName = lastName,
                UserName = username,
                DepartmentIds = new [] { _departmentId },
                EmployeeGroupIds = new [] { _employeeGroupId }
            };

            try
            {
                var employee = (await PostJsonAsync<PostModel<Employee>>(CreateEmployeeUrl, 
                    JsonConvert.SerializeObject(employeeDto))).Data;
                Console.WriteLine("Employee has been successfully created with id " + employee.Id);
                return employee;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        static async Task<Department?> CreateDepartment()
        {
            var departmentName = UserInputHelper.GetUserStringInput("Please enter your Department name or '0' to exit",
                50, "0", LiteralSpaceStringRegexp);
            if (departmentName == "0") return null;

            int? departmentNumber;
            bool wasCancelled;

            (departmentNumber, wasCancelled) = UserInputHelper.GetUserIntInput(
                "Please enter a number for your department or 'X' to exit", 0, int.MaxValue, "X");
            if (wasCancelled) departmentNumber = null;

            var departmentDto = new CreateDepartmentModel()
            {
                Name = departmentName,
                Number = departmentNumber == null ? null : departmentNumber.ToString()
            };

            var department = (await PostJsonAsync<PostModel<Department>>(DepartmentsUrl, 
                JsonConvert.SerializeObject(departmentDto))).Data;
            Console.WriteLine("Department has been successfully created with id " + department.Id);
            return department;
        }

        static async Task<EmployeeGroup?> CreateEmployeeGroup()
        {
            var groupName = UserInputHelper.GetUserStringInput(
                "Please enter the name for the Employee Group or '0' to exit", 50, "0", 
                LiteralSpaceStringRegexp);
            if (groupName == "0") return null;
            
            var groupDto = new CreateEmployeeGroupModel()
            {
                Name = groupName
            };

            var group = (await PostJsonAsync<PostModel<EmployeeGroup>>(EmployeeGroupsUrl, 
                JsonConvert.SerializeObject(groupDto))).Data;
            Console.WriteLine("Employee group has been successfully created with id " + group.Id);
            return group;
        }

        public static async Task<TResponse> PostJsonAsync<TResponse>(string url, string content)
        {
            using var client = new HttpClient {BaseAddress = new Uri(PlandayUrl)};
            var config = GetConfig();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
            client.DefaultRequestHeaders.Add("X-ClientId", config.XClientId);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var response = await client.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<TResponse>(responseData);
            } catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                throw;
            }
        }
        
        public static async Task<TResponse> PostUrlEncodedAsync<TResponse>(string url, FormUrlEncodedContent content)
        {
            using var client = new HttpClient {BaseAddress = new Uri(AuthorizationUrl)};
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<TResponse>(responseData);
            } catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                throw;
            }
        }

        public static async Task<TResponse> GetAsync<TResponse>(string url)
        {
            using var client = new HttpClient();
            var config = GetConfig();

            client.BaseAddress = new Uri(PlandayUrl);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
            client.DefaultRequestHeaders.Add("X-ClientId", config.XClientId);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TResponse>(responseData);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                throw;
            }
        }

        static ApiConfiguration GetConfig()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.AddUserSecrets<Program>();
            Configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();
            services
                .Configure<ApiConfiguration>(Configuration.GetSection(nameof(ApiConfiguration)))
                .AddOptions()
                .AddSingleton<ISecretRevealer, SecretRevealer>()
                .BuildServiceProvider();

            var serviceProvider = services.BuildServiceProvider();
            var revealer = serviceProvider.GetService<ISecretRevealer>();

            return revealer!.Reveal();
        }
    }
}
