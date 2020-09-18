﻿// Enthält unmanaged code (DllImport("kernel32"))!
using System;
using System.Threading;
using System.Net.NetworkInformation;
using NetEti.Globals;
using Vishnu.Interchange;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace CheckDiskSpace
{
    /// <summary>
    /// Prüft, ob noch genug Plattenplatz verfügbar ist.
    /// Ermittelt für das zu prüfende Laufwerk den gesamten Plattenplatz
    /// und den verfügbaren Plattenplatz in MBytes und vergleicht den
    /// verfügbaren Plattenplatz mit einer Mindest-Vorgabe.
    /// </summary>
    /// <remarks>
    /// File: CheckDiskSpace.cs
    /// Autor: Erik Nagel
    ///
    /// 05.04.2014 Erik Nagel: erstellt.
    /// 06.09.2020 Erik Nagel: überarbeitet.
    /// </remarks>
    public class CheckDiskSpace : INodeChecker
    {
        #region public members

        /// <summary>
        /// Kann aufgerufen werden, wenn sich der Verarbeitungs-Fortschritt
        /// des Checkers geändert hat, sollte aber zumindest aber einmal zum
        /// Schluss der Verarbeitung aufgerufen werden.
        /// </summary>
        public event CommonProgressChangedEventHandler NodeProgressChanged;

        /// <summary>
        /// Rückgabe-Objekt des Checkers
        /// </summary>
        public object ReturnObject
        {
            get
            {
                return this._returnObject;
            }
            set
            {
                this._returnObject = value;
            }
        }

        /// <summary>
        /// Haupt-Verarbeitungsroutine - wertet die übergebenen Parameter aus und prüft den verfügbaren Plattenplatz.
        /// </summary>
        /// <param name="checkerParameters">Share oder Laufwerksbuchstabe | minimaler-Plattenplatz(in MByte) [|Timeout [Retries]]</param>
        /// <param name="treeParameters">Für den gesamten Tree gültige Parameter oder null (für zukünftige Versionen vorgesehen).</param>
        /// <param name="source">Auslösendes TreeEvent oder null.</param>
        /// <returns>True, wenn genug Plattenplatz verfügbar ist, ansonsten False oder Exception.</returns>
        public bool? Run(object checkerParameters, TreeParameters treeParameters, TreeEvent source)
        {
            ComplexCheckDiskSpaceReturnObject returnObject = new ComplexCheckDiskSpaceReturnObject();
            this._returnObject = returnObject;
            this.OnNodeProgressChanged(this.GetType().Name, 100, 0, ItemsTypes.items);
            this.EvaluateParametersOrFail(checkerParameters, returnObject);
            this.TryPingOrFail(returnObject);
            this.TryGetDiskSpaceByFileSystem(returnObject);
            if (returnObject.TotalNumberOfMBytes <= 0)
            {
                this.TryGetDiskSpaceBySqlAccess(returnObject);
            }
            if (returnObject.TotalNumberOfMBytes <= 0)
            {
                returnObject.TotalNumberOfMBytes = -1;
                // throw new ApplicationException(String.Format("{0}: Für '{1}' konnte kein Plattenplatz ermittelt werden.", this.GetType().Name, share ?? ""));
            }
            this.OnNodeProgressChanged(this.GetType().Name, 100, 100, ItemsTypes.items);
            return returnObject.FreeMBytesAvailable >= returnObject.CriticalFreeMBytesAvailable;
        }

        #endregion public members

        #region private members

        private object _returnObject = null;

        [DllImport("kernel32")]
        private static extern int GetDiskFreeSpaceEx(
          string lpDirectoryName,
          ref long lpFreeBytesAvailable,
          ref long lpTotalNumberOfBytes,
          ref long lpTotalNumberOfFreeBytes
        );

        private void TryGetDiskSpaceByFileSystem(ComplexCheckDiskSpaceReturnObject returnObject)
        {
            long freeBytesAvailable = 0;
            long totalNumberOfBytes = 0;
            returnObject.SQLServerAccess = false;
            if (returnObject.Server != null)
            {
                if (returnObject.DriveLetter != null)
                {
                    string share = @"\\" + returnObject.Server.TrimStart('\\') + '\\' + returnObject.DriveLetter + '$';
                    GetDiskFreeSpaceBytes(share, out totalNumberOfBytes, out freeBytesAvailable);
                }
            }
            else
            {
                GetDiskFreeSpaceBytes(returnObject.DriveLetter + ":", out totalNumberOfBytes, out freeBytesAvailable);
            }
            returnObject.TotalNumberOfMBytes = totalNumberOfBytes / (1024 * 1000);
            returnObject.FreeMBytesAvailable = freeBytesAvailable / (1024 * 1000);
        }

        private void TryGetDiskSpaceBySqlAccess(ComplexCheckDiskSpaceReturnObject returnObject)
        {
            string sqlServer = returnObject.Server.TrimStart('\\');
            if (sqlServer.ToLower().Equals("localhost"))
            {
                sqlServer = "(local)";
            }
            long freeBytesAvailable;
            long totalNumberOfBytes;
            if (!String.IsNullOrEmpty(returnObject.DriveLetter)
                && this.GetDiskFreeSpaceBytesBySQLServerInstance(sqlServer, returnObject.DriveLetter, out totalNumberOfBytes, out freeBytesAvailable))
            {
                returnObject.SQLServerAccess = true;
                returnObject.TotalNumberOfMBytes = totalNumberOfBytes / (1024 * 1000);
                returnObject.FreeMBytesAvailable = freeBytesAvailable / (1024 * 1000);
            }
        }

        private void TryPingOrFail(ComplexCheckDiskSpaceReturnObject returnObject)
        {
            if (returnObject.Server != null)
            {
                int retry = 0;
                this.OnNodeProgressChanged(this.GetType().Name, returnObject.Retries, retry, ItemsTypes.items);
                while (retry++ < returnObject.Retries && !this.canPing(returnObject.Server.TrimStart('\\'), returnObject.Timeout))
                {
                    this.OnNodeProgressChanged(this.GetType().Name, returnObject.Retries, retry, ItemsTypes.items);
                    Thread.Sleep(10);
                }
                if (retry > returnObject.Retries)
                {
                    throw new ApplicationException(String.Format("{0}: Der Server '{1}' konnte nicht erreicht werden.", this.GetType().Name, returnObject.Server ?? ""));
                }
            }
        }

        private void EvaluateParametersOrFail(object checkerParameters, ComplexCheckDiskSpaceReturnObject returnObject)
        {
            string[] para = checkerParameters.ToString().Split('|');
            string share = para.Length > 0 ? para[0] : "";
            if (!String.IsNullOrEmpty(share))
            {
                string x = share.TrimEnd(':').ToUpper();
                if (x.Length == 1 && Char.IsLetter(x[0]))
                {
                    returnObject.DriveLetter = x;
                }
                if (String.IsNullOrEmpty(returnObject.DriveLetter))
                {
                    string server = null;
                    string tmpShare = @"\\" + share.TrimStart('\\');
                    MatchCollection matches = new Regex(@"\\\\[\w-]+(.*)", RegexOptions.IgnoreCase).Matches(tmpShare);
                    if (matches.Count > 0)
                    {
                        GroupCollection groups = matches[0].Groups;
                        if (groups.Count > 1 && !String.IsNullOrEmpty(groups[1].Value))
                        {
                            server = share.Replace(groups[1].Value, "");
                        }
                        else
                        {
                            server = tmpShare;
                        }
                    }
                    returnObject.Server = server;
                }
            }
            if (returnObject.Server == null && returnObject.DriveLetter == null)
            {
                throw new ArgumentException(String.Format("{0}: Laufwerk oder Server '{1}' wurde nicht gefunden.\n{2}",
                  this.GetType().Name, share ?? "", this.syntax()));
            }
            if (returnObject.DriveLetter == null)
            {
                Match match = Regex.Match(share, @"\\([^\\]+)$");
                string driveLetter = null;
                if (match.Length > 0)
                {
                    GroupCollection groups = match.Groups;
                    if (groups.Count > 1)
                    {
                        driveLetter = groups[1].Value.TrimEnd('$');
                        if (driveLetter != null && driveLetter.Trim() != "" && driveLetter.Length == 1)
                        {
                            returnObject.DriveLetter = driveLetter;
                        }
                    }
                }

            }
            long criticalFreeMBytesAvailable;
            if (para.Length < 2 || !long.TryParse(para[1].Trim(), out criticalFreeMBytesAvailable))
            {
                throw new ArgumentException(String.Format("{0}: Bitte einen Mindest-Platzbedarf in MB angeben.\n{1}",
                  this.GetType().Name, this.syntax()));
            }
            returnObject.CriticalFreeMBytesAvailable = criticalFreeMBytesAvailable;
            int timeout = 2000;
            if (para.Length > 2 && Int32.TryParse(para[2], out timeout)) { }
            returnObject.Timeout = timeout;
            int retries = 1;
            if (para.Length > 3 && !Int32.TryParse(para[3], out retries)) { }
            returnObject.Retries = retries;
        }

        private bool GetDiskFreeSpaceBytesBySQLServerInstance(string sqlServerInstance, string driveLetter, out long totalNumberOfBytes, out long freeBytesAvailable)
        {
            totalNumberOfBytes = 0;
            freeBytesAvailable = 0;
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(
                "data source=" + sqlServerInstance + ";connection timeout=5;trusted_connection=true;"
                + "application name=CheckDiskSpace;Pooling=True;Min Pool Size=3;Max Pool Size=100"
            ))
                {
                    sqlConnection.Open();
                    string cmdText = @"SELECT DISTINCT UPPER(volume_mount_point) volume_mount_point, total_bytes, available_bytes
											FROM sys.master_files AS f
											CROSS APPLY sys.dm_os_volume_stats(f.database_id, f.file_id)";
                    using (SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConnection))
                    {
                        try
                        {
                            using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                            {
                                while (sqlDataReader.Read())
                                {
                                    if (sqlDataReader.GetSqlString(0).ToString().ToUpper().Replace(":\\", "").Equals(driveLetter.ToUpper()))
                                    {
                                        totalNumberOfBytes = Convert.ToInt64(sqlDataReader["total_bytes"]);
                                        freeBytesAvailable = Convert.ToInt64(sqlDataReader["available_bytes"]);
                                        return true;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            sqlCommand.CommandText = @"exec xp_fixeddrives";
                            using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                            {
                                while (sqlDataReader.Read())
                                {
                                    if (sqlDataReader["drive"].Equals(driveLetter.ToUpper()))
                                    {
                                        totalNumberOfBytes = -1;
                                        freeBytesAvailable = Convert.ToInt64(sqlDataReader["MB frei"]) * (1024 * 1000);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return false;
        }


        private object syntax()
        {
            return String.Format("Aufruf: {0} Share oder Laufwerksbuchstabe | minimaler-Plattenplatz(in MByte) [|Timeout [Retries]]", this.GetType().Name);
        }

        private void OnNodeProgressChanged(string itemsName, int countAll, int countSucceeded, ItemsTypes itemsType)
        {
            if (NodeProgressChanged != null)
            {
                NodeProgressChanged(null, new CommonProgressChangedEventArgs(itemsName, countAll, countSucceeded, itemsType, null));
            }
        }

        private bool canPing(string address, int timeout)
        {
            Ping ping = new Ping();

            try
            {
                PingReply reply = ping.Send(address, timeout);
                if (reply == null) return false;

                return (reply.Status == IPStatus.Success);
            }
            catch (PingException)
            {
                return false;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        private static int GetDiskFreeSpaceBytes(string share, out long totalNumberOfBytes, out long freeBytesAvailable)
        {
            int rtn;
            long lpFreeBytesAvailable = 0;
            long lpTotalNumberOfBytes = 0;
            long lpTotalNumberOfFreeBytes = 0;
            rtn = GetDiskFreeSpaceEx(share, ref lpFreeBytesAvailable, ref lpTotalNumberOfBytes, ref lpTotalNumberOfFreeBytes);
            totalNumberOfBytes = lpTotalNumberOfBytes;
            freeBytesAvailable = lpFreeBytesAvailable;
            return rtn;
        }

        #endregion private members
    }
}
