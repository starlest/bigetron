namespace Bigetron.Core.Domain.Users
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

    /// <summary>
    /// Represents a user
    /// </summary>
    public class User: IdentityUser, IEntity<string>
    {
        private DateTime? _createdDate;

        #region Properties
        object IEntity.Id
        {
            get => Id;
            set => Id = (string)Convert.ChangeType(value, typeof(string));
        }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public DateTime CreatedDate
        {
            get => _createdDate ?? DateTime.Now;
            set => _createdDate = value;
        }

        public DateTime? ModifiedDate { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }
        #endregion

        #region Equality
        public override bool Equals(object obj)
        {
            return Equals(obj as User);
        }

        private static bool IsTransient(User obj)
        {
            return obj != null && Equals(obj.Id, default(string));
        }

        private Type GetUnproxiedType()
        {
            return GetType();
        }

        public virtual bool Equals(User other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (IsTransient(this) || IsTransient(other) || !Equals(Id, other.Id)) return false;
            var otherType = other.GetUnproxiedType();
            var thisType = GetUnproxiedType();
            return thisType.IsAssignableFrom(otherType) ||
                   otherType.IsAssignableFrom(thisType);
        }

        public override int GetHashCode()
        {
            return Equals(Id, default(string)) ? base.GetHashCode() : Id.GetHashCode();
        }

        public static bool operator ==(User x, User y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(User x, User y)
        {
            return !(x == y);
        }
        #endregion
    }
}
