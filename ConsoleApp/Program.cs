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
using DAL;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Nito.AsyncEx;

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
        private static List<Department>? _departments = new List<Department>();
        private static List<RevenueUnit>? _revenueUnits = new List<RevenueUnit>();

        private const string EmailRegexp = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@"
                                           + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";

        private const string DateRegexp = @"^\d{4}-((0[1-9])|(1[012]))-((0[1-9]|[12]\d)|3[01])$";
        private const string LiteralSpaceStringRegexp = @"^[0-9A-Za-z ]+$";

        private static readonly string _query = @"SELECT 
            SUM([Transaction].TotalNetSum) as NetSum,
            SUM([Transaction].TotalGrossSum) as GrossSum,
            [Theatre].ID as TheatreID,
            [Theatre].[Name] as TheatreName,
            [SalesPoint].AccountingCode as SalesPointAccountingCode

            FROM [Transaction] WITH (READUNCOMMITTED)
            INNER JOIN [SalesPoint] WITH (READUNCOMMITTED) ON [Transaction].SalesPointID = [SalesPoint].ID
            INNER JOIN [Theatre] WITH (READUNCOMMITTED) ON [SalesPoint].TheatreID = [Theatre].ID

            WHERE [Transaction].dtInvoice = @dtInvoice

            GROUP BY [Theatre].ID,
            [Theatre].[Name],
            [SalesPoint].AccountingCode;";

        static int Main()
        {
            GetConfig();
            int breakPoint;
            RefreshAccessToken();
            Console.WriteLine("You have successfully gained access to Planday! Press ENTER to continue");
            Console.ReadLine();

            do
            {
                try
                {
                    breakPoint = AsyncContext.Run(() => MainAsync());
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return -1;
                }
            } while (breakPoint != -1);

            return breakPoint;
        }

        static async Task<int> MainAsync()
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
            Console.WriteLine("8) Create revenue records from MCS data");
            Console.WriteLine("X) Exit");
            Console.WriteLine("--------------------------------");

            (userInput, _) = UserInputHelper.GetUserIntInput(
                "Please choose your action (or X to exit): ", 1, 8, "x");

            switch (userInput)
            {
                case 1:
                    var department = await CreateDepartment();
                    if (department != null) _departmentId = department.Id;
                    return 0;
                case 2:
                    _departments = await GetAvailableRecords<Department>(_departmentIds, DepartmentsUrl);
                    Console.WriteLine(_departments == null ? "Retrieving department records was cancelled or failed!" : "Success retrievng department records!");
                    return 0;
                case 3:
                    var employeeGroup = await CreateEmployeeGroup();
                    if (employeeGroup != null) _employeeGroupId = employeeGroup.Id;
                    else Console.WriteLine("Employee Group creation was cancelled!");
                    return 0;
                case 4:
                    await GetAvailableRecords<EmployeeGroup>(_employeeGroupIds, EmployeeGroupsUrl);
                    break;
                case 5:
                    var employee = await CreateEmployee();
                    if (employee == null) Console.WriteLine("Employee creation was cancelled!");
                    return 0;
                case 6:
                    await GetAvailableRecords<RevenueUnit>(_revenueUnitIds, RevenueUnitsUrl);
                    return 0;
                case 7:
                    var revenue = await CreateRevenueFromUserInput();
                    Console.WriteLine(revenue == null ? "Revenue creation was cancelled!" : 
                        $"Revenue item for revenue unit {revenue.RevenueUnitId} was successfully created");
                    return 0;
                case 8:
                    var revenueList = await CreateRevenueFromMCS();
                    Console.WriteLine(revenueList == null ? "Revenue creation was cancelled!" :
                        "Revenue records were successfully created!");
                    return 0;
                default:
                    Console.WriteLine("Closing down...");
                    return -1;
            }

            return -1;
        }

        static async Task<List<Revenue>?> CreateRevenueFromMCS()
        {
            var date = UserInputHelper.GetUserStringInput("Please enter the date for which the records should be retrieved in the YYYY-MM-DD format or '0' to exit",
                10, "0", DateRegexp);

            if (date == "0") return null;

            var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Configuration.GetSection("ConnectionStrings.MCS").Value)
                .Options;

            using var context = new AppDbContext(contextOptions);

            var revenueRecords = context.MCSRevenues
                .FromSqlRaw($"DECLARE @dtInvoice as DateTime = '{date}';" + _query)
                .ToList();

            if (revenueRecords == null)
            {
                Console.WriteLine("No revenue records found in the database for the date selected!");
                return null;
            } else
            {
                Console.WriteLine("MCS records successfully retrieved!");
                Console.WriteLine("Please wait for revenue creation...");
            }

            _departments = await GetAvailableRecords<Department>(_departmentIds, DepartmentsUrl);

            if (_departments == null)
            {
                Console.WriteLine("Retrieving available departments failed!");
                return null;
            }

            var departmentNames = _departments.Select(d => d.Name).ToList();
            var revenueList = new List<Revenue>();

            foreach (var record in revenueRecords)
            {
                if (departmentNames.Contains(record.TheatreName))
                {
                    _revenueUnits = await GetAvailableRecords<RevenueUnit>(_revenueUnitIds, RevenueUnitsUrl);
                    if (_revenueUnits == null)
                    {
                        Console.WriteLine("Retrieving available revenue units failed!");
                        return null;
                    }

                    var revenueUnit = _revenueUnits
                        .Where(r => r.DepartmentId == _departments.Where(d => d.Name == record.TheatreName).FirstOrDefault().Id)
                        .FirstOrDefault();

                    if (revenueUnit.Equals(default(RevenueUnit)))
                    {
                        Console.WriteLine($"No revenue unit was found for the department {record.TheatreName}");
                        continue;
                    }

                    var revenue = await CreateRevenue($"{record.TheatreID} - {date}", date, record.NetSum, revenueUnit.Id);
                    Console.WriteLine($"A revenue record for the revenue unit {revenue!.RevenueUnitId} successfully created");
                    revenueList.Add(revenue);
                } else
                {
                    Console.WriteLine(departmentNames);
                    Console.WriteLine($"No department with the name {record.TheatreName} exists in Planday!");
                }
            }

            return revenueList;
        }

        static async Task<List<T>?> GetAvailableRecords<T>(ICollection<int> idList, string url)
        where T : class, IResponseData
        {
            try
            {
                var records = (await GetAsync<GetAllModel<T>>(url));
                if (records == null)
                {
                    Console.WriteLine("No records found!");
                    return null;
                }

                idList = records.DataUnits.Select(r => r.Id).ToList();
                return records.DataUnits;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                throw;
            }
        }

        static async Task<Revenue?> CreateRevenueFromUserInput()
        {
            var date = UserInputHelper.GetUserStringInput("Please enter the date the Revenue was generated or '0' to exit", 
                10, "0", DateRegexp);
            if (date == "0") return null;
            
            var description = UserInputHelper.GetUserStringInput("Please enter a description for the Revenue or '0' to exit", 
                10, "0");
            if (description == "0") return null;

            bool wasCancelled;
            Decimal turnover;
            (turnover, wasCancelled) = UserInputHelper.GetUserDecimalInput(
                "Please enter the value for the turnover or 'X' to exit", 0, Decimal.MaxValue, "x");
            if (wasCancelled) return null;

            await GetAvailableRecords<RevenueUnit>(_revenueUnitIds, RevenueUnitsUrl);

            Console.WriteLine($"Available revenue unit IDs: {_revenueUnitIds}");

            int revenueUnitId;
            (revenueUnitId, wasCancelled) = UserInputHelper.GetUserIntInput(
                    "Please enter your Revenue Unit ID or 'X' to exit", 0, Int32.MaxValue, "x",
                    _revenueUnitIds);
            if (wasCancelled) return null;

            return await CreateRevenue(description, date, turnover, revenueUnitId);
        }

        static async Task<Revenue?> CreateRevenue(string description, string date, Decimal turnover, int revenueUnitId)
        {
            var revenueDto = new CreateRevenueModel()
            {
                Description = description,
                Date = date,
                Turnover = turnover,
                RevenueUnitId = revenueUnitId
            };

            try
            {
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
            var config = GetPlandayConfig();

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

        public static async Task<TResponse?> PostJsonAsync<TResponse>(string url, string content)
            where TResponse : class
        {
            using var client = new HttpClient {BaseAddress = new Uri(PlandayUrl)};
            var config = GetPlandayConfig();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
            client.DefaultRequestHeaders.Add("X-ClientId", config.XClientId);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var response = await client.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Request has failed with the status code {response.StatusCode}. Info: {response.Content}");
                    return null;
                } else
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TResponse>(responseData);
                }
            } catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                Console.WriteLine(e.Data);
                return null;
            }
        }
        
        public static async Task<TResponse?> PostUrlEncodedAsync<TResponse>(string url, FormUrlEncodedContent content)
            where TResponse : class
        {
            using var client = new HttpClient {BaseAddress = new Uri(AuthorizationUrl)};
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Request has failed with the status code {response.StatusCode}. Info: {response.Content}");
                    return null;
                } else
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TResponse>(responseData);
                }
            } catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                Console.WriteLine(e.Data);
                return null;
            }
        }

        public static async Task<TResponse?> GetAsync<TResponse>(string url)
            where TResponse : class
        {
            using var client = new HttpClient();
            var config = GetPlandayConfig();

            client.BaseAddress = new Uri(PlandayUrl);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
            client.DefaultRequestHeaders.Add("X-ClientId", config.XClientId);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Request has failed with the status code {response.StatusCode}. Info: {response.Content}");
                    return null;
                }
                else
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TResponse>(responseData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                Console.WriteLine(e.Data);
                return null;
            }
        }

        static void GetConfig()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.AddUserSecrets<Program>();
            Configuration = builder.Build();
        }

        static ApiConfiguration GetPlandayConfig()
        {
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
