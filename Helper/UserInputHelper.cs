using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Helper
{
    public class UserInputHelper
    {
        public static (int input, bool wasCancelled) GetUserIntInput(string prompt, int minValue, int maxValue, 
            string? exitValue = null, List<int>? allowedValues = null)
        {
            do
            {
                Console.WriteLine(prompt);

                Console.Write(">");

                var inputLine = Console.ReadLine()?.ToLower().Trim();

                if (inputLine != null && inputLine == exitValue) return (0, true);

                if (int.TryParse(inputLine, out var userInt))
                {
                    if (userInt > maxValue || userInt < minValue)
                    {
                        Console.WriteLine($"{inputLine} is not a valid parameter! Please try again");
                        continue;
                    }

                    if (allowedValues != null && !allowedValues.Contains(userInt))
                    {
                        Console.WriteLine($"ID {userInt} doesn't exist! Please try again");
                    }
                    else
                    {
                        return (userInt, false);
                    }
                }
                else
                {
                    Console.WriteLine($"'{inputLine}' cannot be converted into a number! Please try again");
                }
            } while (true);
        }

        public static string GetUserStringInput(string prompt, int maxLength, string? exitValue = null, string? regex = null)
        {
            do
            {
                Console.WriteLine(prompt);

                Console.Write(">");

                var inputLine = Console.ReadLine();

                if (exitValue != null && inputLine?.ToLower().Trim() == exitValue) return inputLine.ToLower().Trim();

                if (String.IsNullOrWhiteSpace(inputLine))
                {
                    Console.WriteLine("Empty strings are not allowed. Please try again");
                    continue;
                }
                
                if (regex != null)
                {
                    Regex rgx = new Regex(regex);
                    if (!rgx.IsMatch(inputLine!))
                    {
                        Console.WriteLine("Your input doesn't match the expected pattern, please try again!");
                        continue;
                    }
                }
                
                if (inputLine!.Length > maxLength)
                {
                    Console.WriteLine("The text is too long. Please try again with smaller blocks");
                }
                else
                {
                    inputLine = maxLength == 1 ? inputLine.ToLower().Trim() : inputLine;
                    return inputLine;
                }
            } while (true);
        }

        public static (Double input, bool wasCancelled) GetUserDoubleInput(string prompt, Double minValue, Double maxValue, string? exitValue)
        {
            do
            {
                Console.WriteLine(prompt);

                Console.Write(">");

                var inputLine = Console.ReadLine()?.ToLower().Trim();

                if (inputLine != null && inputLine == exitValue) return (0, true);

                if (Double.TryParse(inputLine, out var userDouble))
                {
                    if (userDouble > maxValue || userDouble < minValue)
                    {
                        Console.WriteLine($"{inputLine} is not a valid parameter! Please try again");
                    }
                    else
                    {
                        return (userDouble, false);
                    }
                }
                else
                {
                    Console.WriteLine($"'{inputLine}' cannot be converted into a floating point value! Please try again");
                }
            } while (true);
        }
    }
}
