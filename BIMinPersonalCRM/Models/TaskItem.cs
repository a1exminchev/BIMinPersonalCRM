using System;

namespace BIMinPersonalCRM.Models
{
    /// <summary>
    ///     Represents a discrete unit of work performed for a client. In the
    ///     context of this application a task records the time spent and
    ///     monetary value earned. Additional properties can be added as the
    ///     application evolves.
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        ///     Unique identifier for the task.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        ///     A short description of the work performed.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Optional more detailed description.
        /// </summary>
        public string? Description { get; set; }
;

        /// <summary>
        ///     Foreign key referencing the client associated with this task.
        /// </summary>
        public Guid? ClientId { get; set; }
;

        /// <summary>
        ///     The date the task was started. Stored as <see cref="DateTime"/>.
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.Today;

        /// <summary>
        ///     The date the task was completed. May be <c>null</c> if the task is
        ///     ongoing.
        /// </summary>
        public DateTime? EndDate { get; set; }
;

        /// <summary>
        ///     Total number of hours spent on this task. Represented as a
        ///     <see cref="double"/> to allow fractional hours.
        /// </summary>
        public double HoursSpent { get; set; }
;

        /// <summary>
        ///     The total money earned for this task. If hourly billing is used
        ///     this value is calculated as HoursSpent * HourlyRate; otherwise
        ///     enter the agreed amount manually.
        /// </summary>
        public decimal MoneyEarned { get; set; }
;

        /// <summary>
        ///     Hourly rate used when calculating <see cref="MoneyEarned"/>.
        /// </summary>
        public decimal HourlyRate { get; set; }
;

        /// <summary>
        ///     Computed property used in the UI to display the associated
        ///     client's name. This property is not persisted to the data store and
        ///     is populated at runtime by the view model.
        /// </summary>
        public string? ClientName { get; set; }
;
    }
}
