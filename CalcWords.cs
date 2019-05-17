using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CalculateWords
{
    public class CalcWords
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Не задан исходный файл для подсчета слов");
                Console.WriteLine("Сортировка указывается 2-ым параметром: 1- по количеству повторов (нарастающим итогом), 2- по количеству повторов (убывающим итогом), иначе по алфавиту");
                return;
            }

            var fileName = args[0];
            Console.WriteLine($"Чтение файла {args[0]}");

            if (!fileName.ToLower().Contains(".txt"))
            {
                Console.WriteLine("некорректный формат файла - файл должен быть в формате '*.txt'");
                return;
            }

            Dictionary<string, int> listWords;

            try
            {
                listWords = GetWordsFromFile(fileName);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("не найден файл по указанному пути");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"неизвестная ошибка чтения {ex.Message}"  );
                return;
            }

            if (listWords == null || listWords.Count == 0)
            {
                Console.WriteLine("Файл пуст");
                return;
            }

            WriteSortedFile(SortWords.GetSortedWords(args.Length > 1 ? args[1] : string.Empty, listWords));
        }

        
        private static Dictionary<string, int> GetWordsFromFile(string name)
        {
            var listWords = new Dictionary<string, int>();
            string[] separators = { " ", ",", ".", "!", "?" };

            var fileStream = new FileStream(name, FileMode.Open);
            using (var reader = new StreamReader(fileStream, Encoding.UTF8))
            {
                while (reader.Peek() >= 0)
                {
                    string str = reader.ReadLine();
                    string[] words = str.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string nameWord in words)
                    {
                        if (!listWords.ContainsKey(nameWord))
                        {
                            listWords.Add(nameWord, 1);
                        }
                        else
                        {
                            listWords[nameWord]++;
                        }
                    }
                }
            }

            return listWords;
        }
        

        private static void WriteSortedFile(IOrderedEnumerable<KeyValuePair<string, int>> sortWords)
        {
            const string NAME = "ResultWords.txt";
            
            try
            {
                if (File.Exists(NAME) == true)
                {
                    File.Delete(NAME);
                }

                Encoding utf8WithoutBom = new UTF8Encoding(false);
                using (var sw = new StreamWriter(new FileStream(NAME, FileMode.Create, FileAccess.Write), utf8WithoutBom))
                {
                    foreach (var pair in sortWords)
                    {
                        sw.WriteLine($"{pair.Key} {pair.Value}");
                    }

                    sw.Flush();
                    Console.WriteLine($"сформирован файл {NAME} c отсортированными словами");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"неизвестная ошибка записи {ex.Message}");
                return;
            }
        }
    }

    internal class SortWords
    {
        internal static IOrderedEnumerable<KeyValuePair<string, int>> GetSortedWords(string str, Dictionary<string, int> listWords)
        {
            DefaultSort sort;

            switch (str)
            {
                case "1":
                    sort = new ValueAccSort();
                    break;
                case "2":
                    sort = new ValueDecSort();
                    break;
                default:
                    sort = new DefaultSort();
                    break;
            }

            return sort.SortedWords(listWords);
        }
    }

    class DefaultSort
    {
        public virtual IOrderedEnumerable<KeyValuePair<string, int>> SortedWords(Dictionary<string, int> dict)
        {
            Console.WriteLine("Выбрана сортировка по алфавиту");
            return from entry in dict orderby entry.Key ascending select entry;
        }
    }

    class ValueAccSort : DefaultSort
    {
        public override IOrderedEnumerable<KeyValuePair<string, int>> SortedWords(Dictionary<string, int> dict)
        {
            Console.WriteLine("Выбрана сортировка по количеству повторов (нарастающим итогом)");
            return from entry in dict orderby entry.Value ascending, entry.Key ascending select entry;
        }
    }

    class ValueDecSort : DefaultSort
    {
        public override IOrderedEnumerable<KeyValuePair<string, int>> SortedWords(Dictionary<string, int> dict)
        {
            Console.WriteLine("Выбрана сортировка по количеству повторов (убывающим итогом)");
            return from entry in dict orderby entry.Value descending, entry.Key ascending select entry;
        }
    }
}
