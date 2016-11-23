using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ClearNuget
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var homePath = (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            var nugetPackagesPath = Path.Combine(homePath, ".nuget", "packages");
            if (args.Length > 0)
            {
                nugetPackagesPath = args[0];
            }

            var versionComparer = CreateComparer();
            foreach (var packagesDir in Directory.GetDirectories(nugetPackagesPath))
            {
                if (Path.GetFileName(packagesDir).StartsWith(".")) continue;
                Console.WriteLine($"Clear {packagesDir} ...");
                var versionsDirs = Directory.GetDirectories(packagesDir);
                var willRemoveDirs = versionsDirs.OrderBy(Path.GetFileName, versionComparer).ToList();
                willRemoveDirs.RemoveAt(willRemoveDirs.Count - 1);
                foreach (var willRemoveDir in willRemoveDirs)
                {
                    Directory.Delete(willRemoveDir, true);
                }
            }

            Console.WriteLine("OK");
            Console.ReadKey();
        }

        private static Comparer<string> CreateComparer()
        {
            return Comparer<string>.Create((x, y) =>
            {
                var numbersForX = x.Split('.');
                var numbersForY = y.Split('.');
                for (int i = 0; i < numbersForX.Length; i++)
                {
                    if (numbersForY.Length <= i) return 1;
                    int numberX, numberY;
                    var tryParseForX = int.TryParse(numbersForX[i], out numberX);
                    var tryParseForY = int.TryParse(numbersForY[i], out numberY);
                    if (tryParseForX && tryParseForY)
                    {
                        if (numberX == numberY) continue;
                        return numberX.CompareTo(numberY);
                    }
                    if (!tryParseForX && !tryParseForY)
                    {
                        if (numbersForX[i] == numbersForY[i]) continue;
                        return numbersForX[i].CompareTo(numbersForY[i]);
                    }
                    if (tryParseForX)
                        return 1;
                    if (tryParseForY)
                        return -1;
                }
                return -1;
            });
        }
    }
}