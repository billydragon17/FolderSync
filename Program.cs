using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

class FolderSync
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: FolderSync.exe <sourceFolderPath> <replicaFolderPath> <syncIntervalInSeconds>");
            return;
        }

        string sourceFolderPath = args[0];
        string replicaFolderPath = args[1];
        int syncIntervalInSeconds = int.Parse(args[2]);

        if (!Directory.Exists(sourceFolderPath))        // Source doesn't exist
        {
            Console.WriteLine("Source folder does not exist.");
            return;
        }

        if (!Directory.Exists(replicaFolderPath))       // Replica doesn't exist
        {
            Console.WriteLine("Replica folder does not exist. Creating...");
            Directory.CreateDirectory(replicaFolderPath);
        }

        string logFilePath = Path.Combine(replicaFolderPath, "sync_log.txt");

        while (true)
        {
            SynchronizeFolders(sourceFolderPath, replicaFolderPath, logFilePath);
            Thread.Sleep(syncIntervalInSeconds * 1000); // Pause sync for the specified interval
        }
    }

    static void SynchronizeFolders(string sourcePath, string replicaPath, string logFilePath)
    {
        Console.WriteLine($"Synchronizing {sourcePath} to {replicaPath}...");

        string[] sourceFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

        foreach (string sourceFile in sourceFiles)
        {
            string relativePath = sourceFile.Substring(sourcePath.Length + 1); // Get relative path
            string replicaFile = Path.Combine(replicaPath, relativePath);

            if (!File.Exists(replicaFile) || !IsFileContentEqual(sourceFile, replicaFile))
            {
                Console.WriteLine($"Copying {relativePath}...");
                Directory.CreateDirectory(Path.GetDirectoryName(replicaFile));
                File.Copy(sourceFile, replicaFile, true);
            }
        }

        foreach (string replicaFile in Directory.GetFiles(replicaPath, "*", SearchOption.AllDirectories))
        {
            string relativePath = replicaFile.Substring(replicaPath.Length + 1); // Get relative path
            string sourceFile = Path.Combine(sourcePath, relativePath);     

            if (!File.Exists(sourceFile))
            {
                Console.WriteLine($"Removing {relativePath}...");
                File.Delete(replicaFile);
            }
        }

        File.AppendAllText(logFilePath, $"{DateTime.Now}: Synchronization complete.\n");
        Console.WriteLine("Synchronization complete.");
    }

    static bool IsFileContentEqual(string file1, string file2)
    {
        using (var md5 = MD5.Create())
        {
            byte[] hash1 = md5.ComputeHash(File.ReadAllBytes(file1));
            byte[] hash2 = md5.ComputeHash(File.ReadAllBytes(file2));

            return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
        }
    }
}
