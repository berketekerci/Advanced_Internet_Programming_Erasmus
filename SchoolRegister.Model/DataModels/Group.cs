using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolRegister.Model.DataModels
{
    public class Group
    {
          [Key]
          public int Id { get; set; }
          public string Name { get; set; }
          public virtual IList<Student> Students { get; set; }
          public virtual IList<SubjectGroup> SubjectGroups { get; set; }
    }
}