﻿namespace ARMuseum.Models
{
    public class UpdateUserByAdminRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsDeleted { get; set; } 
    }
}
