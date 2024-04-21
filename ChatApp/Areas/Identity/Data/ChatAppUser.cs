using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ChatAppUser class
public class ChatAppUser : IdentityUser
{
    [Column("full_name")]
    [Required]
    public string FullName { get; set; }
}

