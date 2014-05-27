using System;
using ProtoBuf;

namespace PhoneBookCommonLib
{
    /// <summary>
    /// Represents a record in phone book.
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class PhoneBookRecord
    {
        /// <summary>
        /// Name of the person.
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// Phone number of the person.
        /// </summary>
        [ProtoMember(2)]
        public string Phone { get; set; }

        /// <summary>
        /// Creation date of this record.
        /// </summary>
        [ProtoMember(3)]
        public DateTime CreationDate { get; set; }


        [ProtoMember(4)]
        public int? Age { get; set; }

        [ProtoMember(5)]
        public DateTime LastestUpdateDate { get; set; }
        /// <summary>
        /// Creates a new PhoneBookRecord object.
        /// </summary>
        public PhoneBookRecord()
        {
            CreationDate = DateTime.Now;
            LastestUpdateDate = DateTime.Now;
        }

        /// <summary>
        /// Generates a string representation of this object.
        /// </summary>
        /// <returns>String representation of this object</returns>
        public override string ToString()
        {
            return string.Format("Name = {0}, Phone = {1}, Age = {2}, Lastest Update at:{3:yyyy/MM/dd HH:mm:ss}", Name, Phone, Age, LastestUpdateDate);
        }
    }
}