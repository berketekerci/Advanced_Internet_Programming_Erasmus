using System.ComponentModel.DataAnnotations;

namespace SchoolRegister.ViewModels.VM
{
    public class AddOrUpdateGroupVm
    {
       [Required]
       public int? Id { get; set; }
       [Required]
       public string Name { get; set; }
    }
}