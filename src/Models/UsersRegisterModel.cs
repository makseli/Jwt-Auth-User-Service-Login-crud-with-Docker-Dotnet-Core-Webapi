    public class UsersRegisterModel
    {
        public string email { get; set; }
        public string password { get; set; }
        public string? work_title { get; set; }
        public int? company_id { get; set; }
        public string name_surname { get; set; } 
        public string? phone { get; set; }
        public int type_id {get; set;}
    }