using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckDiskSpace
{
    /// <summary>
    /// Klasse zur Demonstration der Auflösung von komplexen
    /// Return-Objects in einem dynamisch geladenen UserNodeControl.
    /// </summary>
    /// <remarks>
    /// File: ComplexServerReturnObject.cs
    /// Autor: Erik Nagel
    ///
    /// 21.12.2014 Erik Nagel: erstellt
    /// </remarks>
    [Serializable()]
    public class ComplexCheckDiskSpaceReturnObject
    {
        /// <summary>
        /// Name des Servers, der angepingt werden soll.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Laufwerksbuchstabe.
        /// </summary>
        public string DriveLetter { get; set; }

        /// <summary>
        /// Noch verfügbarer Plattenplatz in MBytes, ab dem gewarnt wird.
        /// </summary>
        public long CriticalFreeMBytesAvailable { get; set; }

        /// <summary>
        /// Gesamter Plattenplatz in MBytes.
        /// </summary>
        public long TotalNumberOfMBytes { get; set; }

        /// <summary>
        /// Verfügbarer Plattenplatz in MBytes.
        /// </summary>
        public long FreeMBytesAvailable { get; set; }

        /// <summary>
        /// True wenn der Plattenplatz mit Hilfe einer SQL-Server-Instanz ermittelt wurde.
        /// </summary>
        public bool SQLServerAccess { get; set; }

        /// <summary>
        /// Timeout für einen einzelnen Ping.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Anzahl Ping-Versuche, bevor ein Fehler erzeugt wird.
        /// </summary>
        public int Retries { get; set; }

    }
}
