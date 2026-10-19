namespace MyApi.BSL.Exceptions;

// ข้อมูลซ้ำกับที่มีอยู่แล้วในระบบ -> Controller ตอบ 409 (ข้อความปลอดภัยพอที่จะส่งให้ client)
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
