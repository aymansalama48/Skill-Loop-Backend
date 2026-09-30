using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ErrorRefactor
{
    class Program
    {
        static void Main(string[] args)
        {
            string srcDir = @"..\..\src";
            var errorPattern = new Regex(@"new\s+Error\(\s*""([^""]+)""\s*,\s*""([^""]+)""\s*(?:,\s*(ErrorType\.[a-zA-Z]+))?\s*\)");

            // Structure: Project -> ErrorClass -> ErrorName -> (Code, Desc, ErrType)
            var errorClasses = new Dictionary<string, Dictionary<string, Dictionary<string, (string Code, string Desc, string ErrType)>>>();

            string GetErrorClassName(string code)
            {
                var parts = code.Split('.');
                if (parts.Length > 1) return parts[0] + "Errors";
                return "CommonErrors";
            }

            string TranslateArabicToEnglish(string desc, string code)
            {
                var parts = code.Split('.');
                var feature = parts[0];
                var errorName = parts.Length > 1 ? parts[1] : parts[0];
                var descLower = desc.ToLower();

                if (descLower.Contains("غير موجود") || descLower.Contains("not found"))
                    return $"{feature} was not found.";
                if (descLower.Contains("صلاحية") || descLower.Contains("permission") || descLower.Contains("unauthorized"))
                    return $"You do not have permission to modify this {feature.ToLower()}.";
                if (descLower.Contains("بالفعل") || descLower.Contains("already"))
                    return $"This {feature.ToLower()} is already {errorName.ToLower()}.";
                if (descLower.Contains("مطلوب") || descLower.Contains("required"))
                    return $"{feature} is required.";

                var s1 = Regex.Replace(errorName, "([A-Z])", " $1").Trim();
                return $"{feature} {s1.ToLower()}.";
            }

            void ProcessFile(string filepath)
            {
                var content = File.ReadAllText(filepath);
                var matches = errorPattern.Matches(content);
                if (matches.Count == 0) return;

                // Determine project name based on path
                string project = "Skill-Loop.Application";
                if (filepath.Contains("Skill-Loop.Domain")) project = "Skill-Loop.Domain";
                else if (filepath.Contains("Skill-Loop.Infrastructure")) project = "Skill-Loop.Infrastructure";

                if (!errorClasses.ContainsKey(project))
                    errorClasses[project] = new Dictionary<string, Dictionary<string, (string, string, string)>>();

                var newContent = content;
                foreach (Match match in matches)
                {
                    var code = match.Groups[1].Value;
                    var desc = match.Groups[2].Value;
                    var errType = match.Groups[3].Success ? match.Groups[3].Value : "ErrorType.Failure";

                    var errClass = GetErrorClassName(code);
                    var errName = code.Split('.')[^1];
                    var fullName = $"{errClass}.{errName}";

                    if (!errorClasses[project].ContainsKey(errClass))
                        errorClasses[project][errClass] = new Dictionary<string, (string, string, string)>();
                    
                    errorClasses[project][errClass][errName] = (code, TranslateArabicToEnglish(desc, code), errType);

                    var oldTextPattern = @"new\s+Error\(\s*""" + Regex.Escape(code) + @"\s*""\s*,\s*""" + Regex.Escape(desc) + @"\s*""(?:\s*,\s*" + Regex.Escape(errType) + @")?\s*\)";
                    newContent = Regex.Replace(newContent, oldTextPattern, fullName);

                    var usingNamespace = $"{project.Replace("-", "_")}.Common.Errors.{errClass.Replace("Errors", "")}";
                    if (!newContent.Contains($"using {usingNamespace};"))
                    {
                        newContent = $"using {usingNamespace};\n" + newContent;
                    }
                }

                if (content != newContent)
                {
                    File.WriteAllText(filepath, newContent);
                }
            }

            foreach (var file in Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories))
            {
                // Skip the existing Application Error files so we don't mess them up accidentally if they have `new Error`
                if (file.Contains(@"\Common\Errors\")) continue;
                ProcessFile(file);
            }

            foreach (var projKvp in errorClasses)
            {
                var project = projKvp.Key;
                foreach (var errKvp in projKvp.Value)
                {
                    var errClass = errKvp.Key;
                    var errors = errKvp.Value;

                    var folderName = errClass.Replace("Errors", "");
                    var folderPath = Path.Combine(srcDir, project, "Common", "Errors", folderName);
                    Directory.CreateDirectory(folderPath);

                    var filePath = Path.Combine(folderPath, $"{errClass}.cs");

                    List<string> lines;
                    if (File.Exists(filePath))
                    {
                        lines = new List<string>(File.ReadAllLines(filePath));
                        while (lines.Count > 0 && lines[^1].Trim() != "}")
                            lines.RemoveAt(lines.Count - 1);
                        if (lines.Count > 0 && lines[^1].Trim() == "}")
                            lines.RemoveAt(lines.Count - 1);
                    }
                    else
                    {
                        var nsProject = project.Replace("-", "_");
                        lines = new List<string>
                        {
                            "using Skill_Loop.Domain.Common.Results;",
                            "",
                            $"namespace {nsProject}.Common.Errors.{folderName};",
                            "",
                            $"public static class {errClass}",
                            "{"
                        };
                    }

                    var fullText = string.Join("\n", lines);
                    foreach (var err in errors)
                    {
                        var name = err.Key;
                        var (code, desc, errType) = err.Value;
                        if (!fullText.Contains($"public static readonly Error {name} ="))
                        {
                            lines.Add($"    public static readonly Error {name} = new Error(");
                            lines.Add($"        \"{code}\",");
                            lines.Add($"        \"{desc}\",");
                            lines.Add($"        {errType});");
                            lines.Add("");
                        }
                    }

                    lines.Add("}");
                    File.WriteAllLines(filePath, lines);
                }
            }

            Console.WriteLine("Refactoring complete.");
        }
    }
}
