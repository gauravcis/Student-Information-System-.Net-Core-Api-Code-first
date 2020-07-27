using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }
               
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public DateTime DOB { get; set; }


        [Required]
        public string Password { get; set; }

        [Required]
        [Column(TypeName = "bigint")]
        public Int64 Contact { get; set; }

        [Required]
        public string Address { get; set; }

        
    }
}
