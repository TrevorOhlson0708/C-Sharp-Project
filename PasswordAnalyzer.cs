// Analyzes a password you put into it and gives it a score out of 100
// Also tells you what improvements you can make to make your password better
using System;
using System.Collections.Generic;
using System.Linq;

namespace PasswordStrengthAnalyzer
{
    class Program
    {
        private const int MaxPasswordDisplayLength = 20;
        private const int PasswordTruncateLength = 17;
        
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--help")
            {
                ShowHelp();
                return;
            }

            if (args.Length > 0)
            {
                AnalyzeAndDisplay(args[0]);
                return;
            }

            RunInteractiveMode();
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Password Strength Analyzer");
            Console.WriteLine("Usage:");
            Console.WriteLine("  PasswordStrengthAnalyzer [password]  - Analyze a password");
            Console.WriteLine("  PasswordStrengthAnalyzer            - Interactive mode");
            Console.WriteLine("  PasswordStrengthAnalyzer --help     - Show this help");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  PasswordStrengthAnalyzer MyP@ssw0rd!");
            Console.WriteLine("  PasswordStrengthAnalyzer");
        }

        private static void RunInteractiveMode()
        {
            Console.WriteLine("=== Password Strength Analyzer ===");
            Console.WriteLine("Enter a password to analyze (or 'q' to quit, 'help' for tips).");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Enter password: ");
                string? password = ReadPassword();

                if (string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("No password entered. Try again.");
                    Console.WriteLine();
                    continue;
                }

                if (password.ToLower() == "q" || password.ToLower() == "quit")
                    break;

                if (password.ToLower() == "help")
                {
                    ShowTips();
                    Console.WriteLine();
                    continue;
                }

                AnalyzeAndDisplay(password);
                Console.WriteLine();
            }

            Console.WriteLine("Goodbye!");
        }

        private static void ShowTips()
        {
            Console.WriteLine();
            Console.WriteLine("=== Password Tips ===");
            Console.WriteLine("✓ Use at least 12 characters");
            Console.WriteLine("✓ Mix uppercase and lowercase letters");
            Console.WriteLine("✓ Include numbers and special characters");
            Console.WriteLine("✓ Avoid common words and patterns");
            Console.WriteLine("✓ Use unique characters (avoid repetition)");
            Console.WriteLine("✓ Consider using a passphrase instead");
            Console.WriteLine();
        }

        private static void AnalyzeAndDisplay(string password)
        {
            var analysis = AnalyzePassword(password);

            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════");
            Console.WriteLine("           ANALYSIS RESULT");
            Console.WriteLine("═══════════════════════════════════");
            
            string displayPassword = password.Length <= MaxPasswordDisplayLength 
                ? password 
                : password.Substring(0, PasswordTruncateLength) + "...";
            
            Console.WriteLine($"Password:     {displayPassword}");
            Console.WriteLine($"Length:       {analysis.Length} characters");
            Console.WriteLine($"Lowercase:    {(analysis.HasLower ? "✓ Yes" : "✗ No")}");
            Console.WriteLine($"Uppercase:    {(analysis.HasUpper ? "✓ Yes" : "✗ No")}");
            Console.WriteLine($"Digits:       {(analysis.HasDigit ? "✓ Yes" : "✗ No")}");
            Console.WriteLine($"Symbols:      {(analysis.HasSymbol ? "✓ Yes" : "✗ No")}");
            Console.WriteLine($"Unique chars: {analysis.UniqueCharsCount}");
            Console.WriteLine($"Entropy:      {analysis.EstimatedEntropyBits:F1} bits");
            Console.WriteLine();
            
            int barLength = 30;
            int filledLength = (int)(barLength * analysis.Score / 100.0);
            string bar = new string('█', filledLength) + new string('░', barLength - filledLength);
            Console.WriteLine($"Score:        [{bar}] {analysis.Score}/100");
            Console.WriteLine($"Rating:       {GetRatingEmoji(analysis.Score)} {analysis.Rating}");
            Console.WriteLine();

            if (analysis.Feedback.Count > 0)
            {
                Console.WriteLine("💡 Suggestions:");
                foreach (var tip in analysis.Feedback)
                    Console.WriteLine($"   • {tip}");
            }
            else
            {
                Console.WriteLine("✅ Excellent! Your password is very strong.");
            }

            Console.WriteLine("═══════════════════════════════════");
        }

        private static string GetRatingEmoji(int score)
        {
            if (score < 25) return "🔴";
            if (score < 50) return "🟠";
            if (score < 70) return "🟡";
            if (score < 85) return "🟢";
            return "🟩";
        }

        private static string ReadPassword()
        {
            var password = new List<char>();
            ConsoleKeyInfo keyInfo;

            while (true)
            {
                keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (password.Count > 0)
                    {
                        password.RemoveAt(password.Count - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    while (password.Count > 0)
                    {
                        password.RemoveAt(password.Count - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    password.Add(keyInfo.KeyChar);
                    Console.Write("*");
                }
            }

            return new string(password.ToArray());
        }

        private static PasswordAnalysis AnalyzePassword(string password)
        {
            var analysis = new PasswordAnalysis
            {
                Length = password.Length,
                HasLower = password.Any(char.IsLower),
                HasUpper = password.Any(char.IsUpper),
                HasDigit = password.Any(char.IsDigit),
                HasSymbol = password.Any(ch => !char.IsLetterOrDigit(ch)),
                UniqueCharsCount = password.Distinct().Count()
            };

            analysis.EstimatedEntropyBits = EstimateEntropy(password, analysis);

            int score = 0;
            var feedback = new List<string>();

            if (analysis.Length <= 6)
            {
                score += 5;
                feedback.Add("Use a longer password (at least 10-12 characters recommended).");
            }
            else if (analysis.Length <= 9)
            {
                score += 15;
                feedback.Add("Consider increasing length to 12+ characters for better security.");
            }
            else if (analysis.Length <= 12)
            {
                score += 25;
            }
            else if (analysis.Length <= 16)
            {
                score += 35;
            }
            else
            {
                score += 40;
            }

            int varietyCount = 0;
            if (analysis.HasLower) varietyCount++;
            if (analysis.HasUpper) varietyCount++;
            if (analysis.HasDigit) varietyCount++;
            if (analysis.HasSymbol) varietyCount++;

            score += varietyCount * 10;

            if (!analysis.HasLower) feedback.Add("Add lowercase letters (a-z).");
            if (!analysis.HasUpper) feedback.Add("Add UPPERCASE letters (A-Z).");
            if (!analysis.HasDigit) feedback.Add("Add digits (0-9).");
            if (!analysis.HasSymbol) feedback.Add("Add special characters (!, @, #, $, %, etc.).");

            if (analysis.UniqueCharsCount >= 6 && analysis.UniqueCharsCount < 10)
                score += 5;
            else if (analysis.UniqueCharsCount >= 10)
                score += 10;

            string lower = password.ToLower();
            string[] commonPatterns =
            {
                "password", "1234", "12345", "123456", "qwerty",
                "letmein", "admin", "welcome", "monkey", "dragon",
                "master", "sunshine", "princess", "football", "shadow",
                "abc123", "password1", "iloveyou", "trustno1"
            };

            if (commonPatterns.Any(p => lower.Contains(p)))
            {
                score -= 25;
                feedback.Add("Avoid common words or patterns (e.g., 'password', '1234', 'qwerty').");
            }

            bool onlyDigits = password.All(char.IsDigit);
            bool onlyLetters = password.All(char.IsLetter);

            if (onlyDigits || onlyLetters)
            {
                score -= 15;
                feedback.Add("Mix different character types (letters, digits, symbols).");
            }

            if (HasManyRepeats(password))
            {
                score -= 10;
                feedback.Add("Avoid repeating the same character multiple times.");
            }

            if (HasSequentialPattern(password))
            {
                score -= 10;
                feedback.Add("Avoid sequential patterns (e.g., 'abc', '123').");
            }

            if (score < 0) score = 0;
            if (score > 100) score = 100;

            analysis.Score = score;
            analysis.Rating = GetRating(score);
            analysis.Feedback = feedback;

            return analysis;
        }

        private static string GetRating(int score)
        {
            if (score < 25) return "Very Weak";
            if (score < 50) return "Weak";
            if (score < 70) return "Fair";
            if (score < 85) return "Strong";
            return "Very Strong";
        }

        private static double EstimateEntropy(string password, PasswordAnalysis analysis)
        {
            int poolSize = 0;
            if (analysis.HasLower) poolSize += 26;
            if (analysis.HasUpper) poolSize += 26;
            if (analysis.HasDigit) poolSize += 10;
            if (analysis.HasSymbol) poolSize += 30;

            if (poolSize == 0 || password.Length == 0)
                return 0;

            return password.Length * Math.Log(poolSize, 2);
        }

        private static bool HasManyRepeats(string password)
        {
            if (password.Length < 4) return false;

            int maxRepeat = 1;
            int current = 1;

            for (int i = 1; i < password.Length; i++)
            {
                if (password[i] == password[i - 1])
                {
                    current++;
                    if (current > maxRepeat)
                        maxRepeat = current;
                }
                else
                {
                    current = 1;
                }
            }

            return maxRepeat >= 4;
        }

        private static bool HasSequentialPattern(string password)
        {
            if (password.Length < 3) return false;

            string lower = password.ToLower();
            
            for (int i = 0; i < lower.Length - 2; i++)
            {
                char c1 = lower[i];
                char c2 = lower[i + 1];
                char c3 = lower[i + 2];

                if (char.IsLetter(c1) && char.IsLetter(c2) && char.IsLetter(c3))
                {
                    if (c2 == c1 + 1 && c3 == c2 + 1)
                        return true;
                }

                if (char.IsDigit(c1) && char.IsDigit(c2) && char.IsDigit(c3))
                {
                    if (c2 == c1 + 1 && c3 == c2 + 1)
                        return true;
                }
            }

            return false;
        }
    }

    internal class PasswordAnalysis
    {
        public int Length { get; set; }
        public bool HasLower { get; set; }
        public bool HasUpper { get; set; }
        public bool HasDigit { get; set; }
        public bool HasSymbol { get; set; }
        public int UniqueCharsCount { get; set; }
        public double EstimatedEntropyBits { get; set; }
        public int Score { get; set; }
        public string Rating { get; set; } = string.Empty;
        public List<string> Feedback { get; set; } = new List<string>();
    }
}
