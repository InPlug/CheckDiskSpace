using System.Runtime.Serialization;
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
    [DataContract()] // [Serializable()]
    public class ComplexCheckDiskSpaceReturnObject
    {
        /// <summary>
        /// Name des Servers, der angepingt werden soll.
        /// </summary>
        [DataMember]
        public string? Server { get; set; }

        /// <summary>
        /// Laufwerksbuchstabe.
        /// </summary>
        [DataMember]
        public string? DriveLetter { get; set; }

        /// <summary>
        /// Noch verfügbarer Plattenplatz in MBytes, ab dem gewarnt wird.
        /// </summary>
        [DataMember]
        public long? CriticalFreeMBytesAvailable { get; set; }

        /// <summary>
        /// Gesamter Plattenplatz in MBytes.
        /// </summary>
        [DataMember]
        public long? TotalNumberOfMBytes { get; set; }

        /// <summary>
        /// Verfügbarer Plattenplatz in MBytes.
        /// </summary>
        [DataMember]
        public long? FreeMBytesAvailable { get; set; }

        /// <summary>
        /// True wenn der Plattenplatz mit Hilfe einer SQL-Server-Instanz ermittelt wurde.
        /// </summary>
        [DataMember]
        public bool? SQLServerAccess { get; set; }

        /// <summary>
        /// Timeout für einen einzelnen Ping.
        /// </summary>
        [DataMember]
        public int? Timeout { get; set; }

        /// <summary>
        /// Anzahl Ping-Versuche, bevor ein Fehler erzeugt wird.
        /// </summary>
        [DataMember]
        public int? Retries { get; set; }

        /// <summary>
        /// Standard Konstruktor.
        /// </summary>
        public ComplexCheckDiskSpaceReturnObject() { }

        /// <summary>
        /// Deserialisierungs-Konstruktor.
        /// </summary>
        /// <param name="info">Property-Container.</param>
        /// <param name="context">Übertragungs-Kontext.</param>
        protected ComplexCheckDiskSpaceReturnObject(SerializationInfo info, StreamingContext context)
        {
            this.Server = info.GetString("Server");
            this.DriveLetter = info.GetString("DriveLetter");
            this.CriticalFreeMBytesAvailable = (long?)info.GetValue("CriticalFreeMBytesAvailable", typeof(long));
            this.TotalNumberOfMBytes = (long?)info.GetValue("TotalNumberOfMBytes", typeof(long));
            this.FreeMBytesAvailable = (long?)info.GetValue("FreeMBytesAvailable", typeof(long));
            this.SQLServerAccess = (bool?)info.GetValue("SQLServerAccess", typeof(bool));
            this.Timeout = (int?)info.GetValue("Timeout", typeof(int));
            this.Retries = (int?)info.GetValue("Retries", typeof(int));
        }

        /// <summary>
        /// Serialisierungs-Hilfsroutine: holt die Objekt-Properties in den Property-Container.
        /// </summary>
        /// <param name="info">Property-Container.</param>
        /// <param name="context">Serialisierungs-Kontext.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Server", this.Server);
            info.AddValue("DriveLetter", this.DriveLetter);
            info.AddValue("CriticalFreeMBytesAvailable", this.CriticalFreeMBytesAvailable);
            info.AddValue("TotalNumberOfMBytes", this.TotalNumberOfMBytes);
            info.AddValue("FreeMBytesAvailable", this.FreeMBytesAvailable);
            info.AddValue("SQLServerAccess", this.SQLServerAccess);
            info.AddValue("Timeout", this.Timeout);
            info.AddValue("Retries", this.Retries);
        }

        /// <summary>
        /// Überschriebene ToString()-Methode - stellt öffentliche Properties
        /// als einen (mehrzeiligen) aufbereiteten String zur Verfügung.
        /// </summary>
        /// <returns>Alle öffentlichen Properties als ein String aufbereitet.</returns>
        public override string ToString()
        {
            if (String.IsNullOrEmpty(Server) && String.IsNullOrEmpty(DriveLetter))
            {
                return String.Empty;
            }
            StringBuilder str = new StringBuilder();
            string serverDrive;
            if (String.IsNullOrEmpty(Server))
            {
                serverDrive = DriveLetter ?? "";
            }
            else
            {
                serverDrive = Server + (String.IsNullOrEmpty(DriveLetter) ? "" : @"//" + DriveLetter);
            }
            str.Append(String.Format("{0}: gesamt: {1} MB, frei: {2} MB, Minimum: {3} MB",
                       serverDrive, TotalNumberOfMBytes, FreeMBytesAvailable, CriticalFreeMBytesAvailable));
            return str.ToString();
        }

        /// <summary>
        /// Vergleicht dieses Objekt mit einem übergebenen Objekt nach Inhalt.
        /// </summary>
        /// <param name="obj">Das zu vergleichende Objekt.</param>
        /// <returns>True, wenn das übergebene Objekt inhaltlich gleich diesem Objekt ist.</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (this.ToString() != obj.ToString())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Erzeugt einen eindeutigen Hashcode für dieses Objekt.
        /// Der Timestamp wird bewusst nicht in den Vergleich einbezogen.
        /// </summary>
        /// <returns>Hashcode (int).</returns>
        public override int GetHashCode()
        {
            return (this.ToString()).GetHashCode();
        }

    }
}
