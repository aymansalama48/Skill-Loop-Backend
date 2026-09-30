using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TranslateFluentValidation
{
    class Program
    {
        static void Main(string[] args)
        {
            string srcDir = @"..\..\src";
            var messagePattern = new Regex(@"\.WithMessage\(\s*""([^""]+[\u0600-\u06FF]+[^""]*)""\s*\)");

            string Translate(string arabic)
            {
                if (arabic.Contains("مطلوب"))
                    return "This field is required.";
                if (arabic.Contains("أكبر من الصفر") || arabic.Contains("أكبر من 0"))
                    return "Value must be greater than 0.";
                if (arabic.Contains("أكبر من أو يساوي"))
                    return "Value must be greater than or equal to the minimum.";
                if (arabic.Contains("بين") && arabic.Contains("و"))
                    return "Value is out of range.";
                if (arabic.Contains("يتجاوز") || arabic.Contains("أطول من"))
                    return "Length exceeds the maximum allowed.";
                if (arabic.Contains("يقل عن") || arabic.Contains("أقصر من"))
                    return "Length is less than the minimum required.";
                if (arabic.Contains("غير صالح"))
                    return "Invalid value.";
                if (arabic.Contains("صيغة البريد الإلكتروني"))
                    return "Invalid email address format.";
                if (arabic.Contains("رابط"))
                    return "Invalid URL format.";
                if (arabic.Contains("يجب أن يكون"))
                    return "Invalid value.";

                // Fallback
                return "Invalid value.";
            }

            void ProcessFile(string filepath)
            {
                var content = File.ReadAllText(filepath);
                var matches = messagePattern.Matches(content);
                if (matches.Count == 0) return;

                var newContent = content;
                foreach (Match match in matches)
                {
                    var fullMatch = match.Groups[0].Value;
                    var arabicText = match.Groups[1].Value;
                    
                    var englishText = Translate(arabicText);
                    newContent = newContent.Replace(fullMatch, $@".WithMessage(""{englishText}"")");
                }

                if (content != newContent)
                {
                    File.WriteAllText(filepath, newContent);
                }
            }

            foreach (var file in Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories))
            {
                ProcessFile(file);
            }

            Console.WriteLine("Translation complete.");
        }
    }
}
