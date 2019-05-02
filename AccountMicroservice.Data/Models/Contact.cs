using System;
using System.Collections.Generic;
using System.Text;

namespace AccountMicroservice.Data.Models
{
    public class Contact
    {
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public string ContactName { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; }
        public int ContactAccountId { get; set; }
        public Account ContactAccount { get; set; }
    }
}
