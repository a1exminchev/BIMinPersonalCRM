using System;

namespace BIMinPersonalCRM.Models
{
    /// <summary>
    ///     Represents a client for whom work is performed. Clients can have
    ///     optional contact information and free‑form notes.
    /// </summary>
    public class Client
    {
        /// <summary>
        ///     Primary key used for referencing clients from other entities.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        ///     The display name of the client.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Optional contact information such as email or phone number.
        /// </summary>
        public string? ContactInfo { get; set; }

        /// <summary>
        ///     Free‑form notes about the client. This can include billing details,
        ///     preferred communication channels, or anything else relevant.
        /// </summary>
        public string? Notes { get; set; }

    }
}
