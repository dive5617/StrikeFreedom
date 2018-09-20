using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pConfig
{
    public class Database : IComparable
    {
        public string DB_Name { get; set; }
        public string DB_Path { get; set; }

        public Database(string DB_Name, string DB_Path)
        {
            this.DB_Name = DB_Name;
            this.DB_Path = DB_Path;
        }

        int IComparable.CompareTo(Object obj)
        {
            Database temp = (Database)obj;
            return this.DB_Name.CompareTo(temp.DB_Name);
        }

        public override bool Equals(object obj)
        {
            var database = obj as Database;
            if (database == null)
                return false;
            if (this.DB_Name == database.DB_Name)
                return true;
            return false;
        }
        public static bool operator ==(Database a, Database b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.DB_Name == b.DB_Name;
        }
        public static bool operator !=(Database a, Database b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return this.DB_Name.GetHashCode();
        }
    }
}
