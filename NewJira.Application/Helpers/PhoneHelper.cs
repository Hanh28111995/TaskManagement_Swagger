namespace NewJira.Application.Helpers // (Hoặc namespace tương ứng với thư mục bạn chọn)
{
    public static class PhoneHelper
    {
        // Chuyển từ +84 hoặc 84 về dạng 0xxxxxxxxx để query DB
        public static string NormalizeToLocal(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return phone;

            if (phone.StartsWith("+84"))
            {
                return "0" + phone.Substring(3);
            }
            if (phone.StartsWith("84") && phone.Length > 9)
            {
                return "0" + phone.Substring(2);
            }

            return phone.Trim();
        }

        // Chuyển từ 0xxxxxxxxx sang định dạng quốc tế +84 nếu cần gửi đi
        public static string NormalizeToInternational(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return phone;

            if (phone.StartsWith("0"))
            {
                return "+84" + phone.Substring(1);
            }

            return phone.StartsWith("+") ? phone : "+" + phone;
        }
    }
}