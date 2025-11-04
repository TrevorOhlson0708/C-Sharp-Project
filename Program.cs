using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.CommandLine;
using Aspose.Email.Storage.Pst;
using Aspose.Email.Mapi;

namespace PSTGenerator;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var folderCountOption = new Option<int>(
            aliases: new[] { "--folders", "-f" },
            description: "Number of folders to create in the PST file")
        {
            IsRequired = true
        };

        var emailCountOption = new Option<int>(
            aliases: new[] { "--emails", "-e" },
            description: "Total number of emails to create across all folders")
        {
            IsRequired = true
        };

        var targetSizeOption = new Option<long>(
            aliases: new[] { "--size", "-s" },
            description: "Target size in MB (e.g., 1024 for 1GB, 10240 for 10GB)")
        {
            IsRequired = false
        };
        targetSizeOption.SetDefaultValue(0L);

        var outputPathOption = new Option<string>(
            aliases: new[] { "--output", "-o" },
            description: "Output path for the PST file")
        {
            IsRequired = false
        };
        outputPathOption.SetDefaultValue("TestPST.pst");

        var rootCommand = new RootCommand("PST File Generator - Creates test PST files with fake emails")
        {
            folderCountOption,
            emailCountOption,
            targetSizeOption,
            outputPathOption
        };

        rootCommand.SetHandler(async (int folders, int emails, long targetSize, string outputPath) =>
        {
            await GeneratePSTFile(folders, emails, targetSize, outputPath);
        }, folderCountOption, emailCountOption, targetSizeOption, outputPathOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task GeneratePSTFile(int folderCount, int emailCount, long targetSizeMB, string outputPath)
    {
        // Convert MB to bytes
        long targetSize = targetSizeMB * 1024 * 1024;

        Console.WriteLine($"PST Generator Starting...");
        Console.WriteLine($"Folders: {folderCount}");
        Console.WriteLine($"Emails: {emailCount}");
        Console.WriteLine($"Target Size: {targetSizeMB / 1024.0:F2} GB ({targetSizeMB:N0} MB)");
        Console.WriteLine($"Output: {outputPath}");
        Console.WriteLine();

        // Ensure output directory exists
        string outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Console.WriteLine($"Creating output directory: {outputDir}");
            Directory.CreateDirectory(outputDir);
        }

        // Delete existing PST file if it exists
        if (File.Exists(outputPath))
        {
            Console.WriteLine($"Deleting existing PST file: {outputPath}");
            File.Delete(outputPath);
        }

        try
        {
            // Create PST file
            Console.WriteLine($"Creating PST file: {outputPath}");
            using (PersonalStorage pst = PersonalStorage.Create(outputPath, FileFormatVersion.Unicode))
            {
                Console.WriteLine("PST file created successfully!");
                Console.WriteLine();

                // Get the root folder
                FolderInfo rootFolder = pst.RootFolder;

                // Calculate emails per folder
                int emailsPerFolder = emailCount / folderCount;
                int remainingEmails = emailCount % folderCount;

                // Calculate size per email if target size is specified
                long sizePerEmail = 0;
                if (targetSize > 0)
                {
                    sizePerEmail = targetSize / emailCount;
                }

                // Create folders and emails
                var random = new Random();
                var folderNames = new HashSet<string>();

                for (int folderIndex = 0; folderIndex < folderCount; folderIndex++)
                {
                    // Generate unique folder name
                    string folderName;
                    int attempts = 0;
                    do
                    {
                        folderName = $"Folder_{folderIndex + 1}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                        attempts++;
                    } while (folderNames.Contains(folderName) && attempts < 100);
                    folderNames.Add(folderName);

                    Console.WriteLine($"Creating folder {folderIndex + 1}/{folderCount}: {folderName}");

                    // Create folder in PST
                    FolderInfo folder = rootFolder.AddSubFolder(folderName);

                    // Calculate emails for this folder
                    int emailsInFolder = emailsPerFolder + (folderIndex < remainingEmails ? 1 : 0);

                    Console.WriteLine($"  Adding {emailsInFolder} emails...");

                    for (int emailIndex = 0; emailIndex < emailsInFolder; emailIndex++)
                    {
                        if (emailIndex > 0 && emailIndex % 100 == 0)
                        {
                            Console.WriteLine($"  Progress: {emailIndex}/{emailsInFolder} emails");
                        }

                        // Generate email content
                        string subject = GenerateRandomSubject(random);
                        string body = GenerateRandomEmailBody(random, sizePerEmail);
                        string sender = GenerateRandomEmail(random, "sender");
                        string recipient = GenerateRandomEmail(random, "recipient");
                        DateTime sentDate = DateTime.Now.AddDays(-random.Next(0, 365));

                        // Create MAPI message
                        MapiMessage message = new MapiMessage(sender, recipient, subject, body);
                        message.ClientSubmitTime = sentDate;
                        message.DeliveryTime = sentDate;

                        // Add attachments if we need to meet size requirements
                        if (sizePerEmail > 0 && sizePerEmail > 10000) // Only if we need significant size
                        {
                            long remainingSize = sizePerEmail - body.Length;
                            if (remainingSize > 1000)
                            {
                                AddRandomAttachment(message, random, remainingSize);
                            }
                        }
                        else if (targetSize == 0 && random.NextDouble() < 0.1) // 10% chance to add attachments
                        {
                            AddRandomAttachment(message, random, random.Next(1024, 1024 * 1024)); // 1KB to 1MB
                        }

                        // Add message to folder
                        folder.AddMessage(message);
                    }

                    Console.WriteLine($"  Completed folder {folderIndex + 1}/{folderCount}");
                    Console.WriteLine();
                }

                Console.WriteLine("PST file generation completed!");
            }

            // Get final file size
            if (File.Exists(outputPath))
            {
                FileInfo fileInfo = new FileInfo(outputPath);
                Console.WriteLine($"Final file size: {fileInfo.Length / (1024.0 * 1024.0 * 1024.0):F2} GB ({fileInfo.Length:N0} bytes)");
                Console.WriteLine($"PST file location: {Path.GetFullPath(outputPath)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }

        await Task.CompletedTask;
    }

    static string GenerateRandomSubject(Random random)
    {
        string[] subjects = {
            "Meeting Follow-up",
            "Project Update",
            "Quarterly Report",
            "Budget Review",
            "Status Report",
            "Action Items",
            "Weekly Summary",
            "Urgent: Review Required",
            "Team Meeting Notes",
            "Customer Feedback",
            "Product Launch",
            "Marketing Campaign",
            "Sales Forecast",
            "Technical Documentation",
            "Code Review Request"
        };

        return $"{subjects[random.Next(subjects.Length)]} - {Guid.NewGuid().ToString().Substring(0, 8)}";
    }

    static string GenerateRandomEmailBody(Random random, long targetSize)
    {
        if (targetSize > 0 && targetSize > 5000)
        {
            // Generate body to meet size requirement
            int targetLength = (int)Math.Min(targetSize - 2000, 1000000); // Cap at 1MB for body
            var body = new System.Text.StringBuilder(targetLength);

            string[] paragraphs = {
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. ",
                "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. ",
                "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris. ",
                "Duis aute irure dolor in reprehenderit in voluptate velit esse. ",
                "Excepteur sint occaecat cupidatat non proident, sunt in culpa. "
            };

            while (body.Length < targetLength)
            {
                body.Append(paragraphs[random.Next(paragraphs.Length)]);
                if (random.Next(100) < 5)
                {
                    body.Append("\n\n");
                }
            }

            return body.ToString();
        }
        else
        {
            // Generate random body of reasonable size
            int paragraphs = random.Next(3, 10);
            var body = new System.Text.StringBuilder();

            string[] sentenceTemplates = {
                "This is an important message regarding the project status. ",
                "We need to schedule a meeting to discuss the upcoming deliverables. ",
                "Please review the attached documents and provide your feedback. ",
                "The team has made significant progress on the assigned tasks. ",
                "We are facing some challenges that require immediate attention. ",
                "The deadline for submission is approaching, please ensure all work is completed. ",
                "Thank you for your continued support and dedication to this project. ",
                "I look forward to hearing your thoughts on this matter. "
            };

            for (int i = 0; i < paragraphs; i++)
            {
                int sentences = random.Next(2, 6);
                for (int j = 0; j < sentences; j++)
                {
                    body.Append(sentenceTemplates[random.Next(sentenceTemplates.Length)]);
                }
                body.Append("\n\n");
            }

            return body.ToString();
        }
    }

    static string GenerateRandomEmail(Random random, string prefix)
    {
        string[] domains = { "example.com", "test.com", "company.com", "email.com", "mail.com" };
        return $"{prefix}_{random.Next(1000, 9999)}@{domains[random.Next(domains.Length)]}";
    }

    static void AddRandomAttachment(MapiMessage message, Random random, long targetSize)
    {
        try
        {
            // Create a temporary file with random data
            string tempFile = Path.GetTempFileName();

            try
            {
                using (FileStream fs = new FileStream(tempFile, FileMode.Create))
                {
                    byte[] buffer = new byte[8192];
                    long remaining = targetSize;

                    while (remaining > 0)
                    {
                        random.NextBytes(buffer);
                        int toWrite = (int)Math.Min(remaining, buffer.Length);
                        fs.Write(buffer, 0, toWrite);
                        remaining -= toWrite;
                    }
                }

                // Add attachment to message
                string attachmentName = $"Attachment_{Guid.NewGuid().ToString().Substring(0, 8)}.dat";
                message.Attachments.Add(attachmentName, File.ReadAllBytes(tempFile));
            }
            finally
            {
                // Delete temp file after adding
                try
                {
                    File.Delete(tempFile);
                }
                catch { }
            }
        }
        catch
        {
            // Ignore attachment errors
        }
    }
}

//cd TestPstFile
//dotnet run -- --folders 5 --emails 100 --output "SmallTest.pst"
//dotnet run -- --folders 10 --emails 500 --output "MediumTest.pst"
//dotnet run -- --folders 10 --emails 500 --size 1024 --output "1GB.pst"
//dotnet run -- --folders 20 --emails 2000 --size 10240 --output "10GB.pst"