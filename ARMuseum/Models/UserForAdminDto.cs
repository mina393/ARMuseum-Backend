using System.Collections.Generic;

public class UserForAdminDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsDeleted { get; set; }
    public string PhoneNumber { get; set; }
    public IList<string> Roles { get; set; }
}