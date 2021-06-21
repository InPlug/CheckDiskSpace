using System;
using NetEti.Globals;
using CheckDiskSpace;
using Vishnu.Interchange;

namespace CheckDiskSpaceDemo
{
    public class Program
    {
        static void Main(string[] args)
        {
            CheckDiskSpace.CheckDiskSpace checkDiskSpace = new CheckDiskSpace.CheckDiskSpace();
            checkDiskSpace.NodeProgressChanged += SubNodeProgressChanged;
            bool? res;
            //res = checkDiskSpace.Run(@"Localhost\d$|1000|100|1", new TreeParameters("MainTree", null), null);
            //res = checkDiskSpace.Run(@"Localhost\d$|1000|100|1|true", new TreeParameters("MainTree", null), null);
            res = checkDiskSpace.Run(@"D|20184|100|3", new TreeParameters("MainTree", null), null);
            //res = checkDiskSpace.Run(@"\\DataServer\c$|100000|100|3", new TreeParameters("MainTree", null), null);
            Console.WriteLine("{0}\\{1}: gesamt: {2} MB, frei: {3} MB, Minimum: {4} MB, Ergebnis: {5}",
              (checkDiskSpace.ReturnObject as ComplexCheckDiskSpaceReturnObject).Server,
              (checkDiskSpace.ReturnObject as ComplexCheckDiskSpaceReturnObject).DriveLetter,
              (checkDiskSpace.ReturnObject as ComplexCheckDiskSpaceReturnObject).TotalNumberOfMBytes,
              (checkDiskSpace.ReturnObject as ComplexCheckDiskSpaceReturnObject).FreeMBytesAvailable,
              (checkDiskSpace.ReturnObject as ComplexCheckDiskSpaceReturnObject).CriticalFreeMBytesAvailable,
              res == null ? "unklar" : res == true ? "ok." : "nicht ok."
            );
            Console.ReadLine();
        }

        static void SubNodeProgressChanged(object sender, CommonProgressChangedEventArgs args)
        {
            Console.WriteLine("{0}: {1} von {2}", args.ItemName, args.CountSucceeded, args.CountAll);
        }
    }
}
