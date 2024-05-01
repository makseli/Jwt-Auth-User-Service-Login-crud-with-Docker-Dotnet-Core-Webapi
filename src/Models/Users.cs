using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Src.Models
{
    [Table("users")]
    public class Users
    {
        [Key]
        public int id { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public string? work_title { get; set; }
        public string? name_surname { get; set; } 
        public string? phone { get; set; }
        public int? type_id {get; set;}
        public DateTimeOffset? created_at {get; set;}
        public DateTimeOffset? updated_at {get; set;}
        public DateTimeOffset? deleted_at {get; set;}
        public DateTimeOffset? last_login_time {get; set;}
        public string? last_token  {get; set;}
        public string? last_ip  {get; set;}
        public string? last_user_agent  {get; set;} 
    }

}