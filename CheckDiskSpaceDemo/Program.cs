using System.ComponentModel;
using Vishnu.Interchange;

namespace CheckDiskSpaceDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CheckDiskSpace.CheckDiskSpace checkDiskSpace = new CheckDiskSpace.CheckDiskSpace();
            checkDiskSpace.NodeProgressChanged += SubNodeProgressChanged;
            bool? res;
            //res = checkDiskSpace.Run(@"Localhost\d$|1000|100|1", new TreeParameters("MainTree", null), null);
            //res = checkDiskSpace.Run(@"Localhost\d$|1000|100|1|true", new TreeParameters("MainTree", null), null);
            res = checkDiskSpace.Run(@"D|20184|100|3", new TreeParameters("MainTree", null), TreeEvent.UndefinedTreeEvent);
            //res = checkDiskSpace.Run(@"\\DataServer\c$|100000|100|3", new TreeParameters("MainTree", null), null);
            Console.WriteLine(res.ToString() + " - " + checkDiskSpace.ReturnObject?.ToString() ?? "null");
            Console.ReadLine();
        }

        static void SubNodeProgressChanged(object? sender, ProgressChangedEventArgs args)
        {
            Console.WriteLine(args.ProgressPercentage);
        }
    }
}