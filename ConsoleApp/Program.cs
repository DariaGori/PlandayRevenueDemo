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

namespace ConsoleApp
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        private const string AuthorizationUrl = "https://id.planday.com";
        private const string PlandayUrl = "https://openapi.planday.com";

        private const string CreateDepartmentUrl = "hr/v1.0/departments";
        private const string CreateRevenueUrl = "revenue/v1.0/revenue";
        private const string GetRevenueUnitsUrl = "revenue/v1.0/revenueunits";
        private const string CreateEmployeeUrl = "hr/v1.0/employees";
        private const string CreateEmployeeGroupUrl = "hr/v1.0/employeegroups";
        private const string RefreshTokenUrl = "connect/token";
        
        private const string AuthorizationGrantType = "refresh_token";
        private static string _accessToken;
        private static int _departmentId, _employeeGroupId;

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
            bool wasCancelled;

            int userInput;
                
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Please select an action: ");
            Console.WriteLine("1) Create a department");
            Console.WriteLine("2) Create an employee group");
            Console.WriteLine("3) Create an employee");
            Console.WriteLine("4) Create a revenue record");
            Console.WriteLine("X) Exit");
            Console.WriteLine("--------------------------------");

            (userInput, wasCancelled) = UserInputHelper.GetUserIntInput(
                "Please choose your action (or X to exit): ", 1, 4, "x");

            switch (userInput)
            {
                case 1:
                    _departmentId = (await CreateDepartment()).Id;
                    break;
                case 2:
                    _employeeGroupId = (await CreateEmployeeGroup()).Id;
                    break;
                case 3:
                    var employee = await CreateEmployee();
                    break;
                case 4:
                    var revenue = await CreateRevenue();
                    Console.WriteLine("Revenue item for revenue unit " + revenue.RevenueUnitId + " was successfully created");
                    break;
                default:
                    Console.WriteLine("Closing down...");
                    break;
            }
        }

        static async Task<Revenue> CreateRevenue()
        {
            Console.WriteLine("Please enter the date for the revenue to be created in the format 'YYYY-mm-dd'");
            var date = Console.ReadLine();
            var revenueDto = new CreateRevenueRequestDto()
            {
                Description = "POS",
                Date = date,
                Turnover = 2800.00
            };

            try
            {
                var revenueUnits = (await GetAsync<GetAllResponse<RevenueUnit>>(GetRevenueUnitsUrl)).DataUnits;
                // foreach (var unit in revenueUnits)
                // {
                //     Console.WriteLine(unit.Id + " - " + unit.Name);
                // }
                //
                // Console.WriteLine("Please enter the revenue unit ID: ");
                // var unitId = Console.ReadLine();
                // if (String.IsNullOrEmpty(unitId)) return null;
                //
                // revenueDto.RevenueUnitId = int.Parse(unitId);
                revenueDto.RevenueUnitId = revenueUnits[^1].Id;
                return (await PostJsonAsync<PostResponse<Revenue>>(CreateRevenueUrl, JsonConvert.SerializeObject(revenueDto))).Data;
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
                var authCredentials = await PostUrlEncodedAsync<AuthorizationResponse>(RefreshTokenUrl, content);
                _accessToken = authCredentials.AccessToken;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static async Task<Employee> CreateEmployee()
        {
            var username = "";
            bool wasCancelled = false;
            do
            {
                Console.WriteLine("Please enter your username");
                username = Console.ReadLine();
            } while (username?.Trim().Length == 0);
            
            if (_departmentId == 0)
                (_departmentId, wasCancelled) =
                    UserInputHelper.GetUserIntInput("Please enter department ID value (or X to exit)", 1,
                        int.MaxValue, "x");
            if (wasCancelled) return null;

            if (_employeeGroupId == 0)
                (_employeeGroupId, wasCancelled) =
                    UserInputHelper.GetUserIntInput("Please enter employee group ID value (or X to exit)", 1,
                        int.MaxValue, "x");
            if (wasCancelled) return null;

            var employeeDto = new CreateEmployeeRequestDto()
            {
                Gender = Gender.Male.ToString(),
                PrimaryDepartmentId = null,
                EmployeeTypeId = null,
                BirthDate = "2010-11-17",
                FirstName = "Tony",
                LastName = "Stark",
                UserName = username,
                DepartmentIds = new [] { _departmentId },
                EmployeeGroupIds = new [] { _employeeGroupId }
            };

            try
            {
                var employee = (await PostJsonAsync<PostResponse<Employee>>(CreateEmployeeUrl, 
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

        static async Task<Department> CreateDepartment()
        {
            var departmentDto = new CreateDepartmentRequestDto()
            {
                Name = "Test",
                Number = "123"
            };

            var department = (await PostJsonAsync<PostResponse<Department>>(CreateDepartmentUrl, 
                JsonConvert.SerializeObject(departmentDto))).Data;
            Console.WriteLine("Department has been successfully created with id " + department.Id);
            return department;
        }

        static async Task<EmployeeGroup> CreateEmployeeGroup()
        {
            var groupName = "";
            do
            {
                Console.WriteLine("Please enter a name for your employee group");
                groupName = Console.ReadLine();
            } while (groupName?.Trim().Length == 0);
            
            var groupDto = new CreateEmployeeGroupRequestDto()
            {
                Name = groupName
            };

            var group = (await PostJsonAsync<PostResponse<EmployeeGroup>>(CreateEmployeeGroupUrl, 
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

            return revealer?.Reveal();
        }
    }
}
