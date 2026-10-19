namespace MyApi.BSL.Exceptions;

// ข้อมูลที่ส่งมาไม่ถูกต้อง -> Controller ตอบ 400 (ข้อความปลอดภัยพอที่จะส่งให้ client)
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}
