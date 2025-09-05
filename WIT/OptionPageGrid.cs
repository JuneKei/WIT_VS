using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WIT
{
    [ComVisible(true)]
    [Guid("A599E4AD-1DF6-4442-BF19-519615980401")] // 중요: 이 GUID는 직접 생성해야 합니다.
    public class OptionPageGrid : DialogPage
    {
        [Category("Database Settings")]
        [DisplayName("Connection String")]
        [Description("데이터베이스 연결 문자열을 입력하세요.")]
        public string ConnectionString { get; set; } = "Host=localhost;Port=5432;Username=user;Password=password;Database=wit_db";
    }
}