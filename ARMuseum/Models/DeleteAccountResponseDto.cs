using System.ComponentModel.DataAnnotations; // تأكد أن هذا موجود

public class DeleteAccountResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}