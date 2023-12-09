using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace PMT_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        public enum SortingAlgorithm
        {
            Option1 = 1,
            Option2 = 2
        }

        [HttpGet]
        public ActionResult<string> ProcessStringAction(string inputString, SortingAlgorithm choiceAlgorithm)
        {
            try
            {
                if (Regex.IsMatch(input: inputString, "^[a-z]*$"))
                {
                    string result = ProcessString(input: inputString);

                    var character = CountCharacters(input: result);

                    string substring = FindLongestSubstring(result);

                    char[] sortResult;

                    if (choiceAlgorithm.ToString() == "1")
                    {
                        var sortAlgorithm = new QuickSortAlgorithm();
                        sortResult = result.ToCharArray();
                        sortAlgorithm.Sort(sortResult);
                    }
                    else
                    {
                        var sortAlgorithm = new TreeSortAlgorithm();
                        sortResult = result.ToCharArray();
                        sortAlgorithm.Sort(sortResult);
                    }

                    int randomPosition = int.Parse(GetRandomNumber(result.Length).Result);

                    var responseObject = new
                    {
                        ProcessedString = result,
                        CountCharacter = character,
                        LongestSubstring = substring,
                        SortString = new string(sortResult),
                        RemoveString = result.Remove(randomPosition, 1)
                    };

                    return Ok(responseObject);
                }
                else
                {
                    return BadRequest("Были введены не подходящие символы: " + GetInvalidCharacters(input: inputString));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ErrorMessage = ex.Message });
            }

        }

        static async Task<string> GetRandomNumber(int lenMax)
        {
            lenMax -= 1;
            try
            {
                using (HttpClient client = new())
                {
                    string apiUrl = "https://www.random.org/integers/?num=1&min=0&max=" + lenMax.ToString() + "&col=1&base=10&format=plain&rnd=new";
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обращении к удаленному API: {ex.Message}");
            }

            Random random = new();
            return random.Next(0, lenMax).ToString();
        }

        static string FindLongestSubstring(string input)
        {
            string vowels = "aeiouy";
            string longestSubstring = "";
            int maxLength = longestSubstring.Length;

            for (int i = 0; i < input.Length; i++)
            {
                if (vowels.Contains(input[i]))
                {
                    for (int j = i + 1; j < input.Length; j++)
                    {
                        if (vowels.Contains(input[j]))
                        {
                            string substring = input.Substring(i, j - i + 1);
                            if (substring.Length > maxLength)
                            {
                                maxLength = substring.Length;
                                longestSubstring = substring;
                            }
                        }
                    }
                }
            }

            return longestSubstring;
        }

        static List<string> CountCharacters(string input)
        {
            List<string> characters = new List<string>();
            foreach (var baseCharacter in input.Distinct().ToArray())
            {
                var count = input.Count(character => character == baseCharacter);
                characters.Add(("Количество символов " + baseCharacter.ToString() + " в обработанной строке = " + count.ToString()));
            }
            return characters;
        }

        class InvalidStringException : Exception
        {
            public InvalidStringException(string characters) : base(characters) { }
        }

        static string GetInvalidCharacters(string input)
        {
            return new string(input.Where(c => !Regex.IsMatch(c.ToString(), "^[a-z]*$")).Distinct().ToArray());
        }

        static string ProcessString(string input)
        {
            if (input.Length % 2 == 0)
            {
                int halfLength = input.Length / 2;

                string firstHalf = input[..halfLength];
                string secondHalf = input[halfLength..];

                string reversFirstHalf = ReverseString(firstHalf);
                string reversSecondHalf = ReverseString(secondHalf);

                return reversFirstHalf + reversSecondHalf;
            }
            else
            {
                string reversInput = ReverseString(input);

                return reversInput + input;
            }
        }

        static string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }

    interface ISortAlgorithm
    {
        void Sort(char[] array);
    }

    class QuickSortAlgorithm : ISortAlgorithm
    {
        public void Sort(char[] inputArray)
        {
            QuickSort(inputArray, 0, inputArray.Length - 1);
        }

        private void QuickSort(char[] array, int startIndex, int endIndex)
        {
            if (startIndex < endIndex)
            {
                int supportIndex = Partition(array, startIndex, endIndex);

                QuickSort(array, startIndex, supportIndex - 1);
                QuickSort(array, supportIndex + 1, endIndex);
            }
        }

        private int Partition(char[] array, int lowIndex, int highIndex)
        {
            char support = array[highIndex];
            int i = lowIndex - 1;

            for (int j = lowIndex; j < highIndex; j++)
            {
                if (array[j] <= support)
                {
                    i++;
                    Swap(ref array[i], ref array[j]);
                }
            }

            Swap(ref array[i + 1], ref array[highIndex]);
            return i + 1;
        }

        private void Swap(ref char element1, ref char element2)
        {
            char temp = element1;
            element1 = element2;
            element2 = temp;
        }
    }

    public class TreeNode
    {
        public char Key;
        public TreeNode Left, Right;

        public TreeNode(char item)
        {
            Key = item;
            Left = Right = null;
        }
    }

    class TreeSortAlgorithm : ISortAlgorithm
    {
        private TreeNode root;

        public void Sort(char[] array)
        {
            root = null;

            foreach (var element in array)
            {
                Insert(element);
            }

            InOrderTraversal(root, array);
        }

        private void Insert(char key)
        {
            root = InsertRec(root, key);
        }

        private TreeNode InsertRec(TreeNode root, char key)
        {
            if (root == null)
            {
                root = new TreeNode(key);
                return root;
            }

            if (key < root.Key)
            {
                root.Left = InsertRec(root.Left, key);
            }
            else if (key >= root.Key)
            {
                root.Right = InsertRec(root.Right, key);
            }

            return root;
        }

        private void InOrderTraversal(TreeNode root, char[] result)
        {
            int index = 0;
            InOrderTraversal(root, result, ref index);
        }

        private void InOrderTraversal(TreeNode root, char[] result, ref int index)
        {
            if (root != null)
            {
                InOrderTraversal(root.Left, result, ref index);
                result[index++] = root.Key;
                InOrderTraversal(root.Right, result, ref index);
            }
        }
    }
}