using System.Diagnostics.CodeAnalysis;
using Community.VisualStudio.Toolkit;
using EnvDTE; // DTE를 사용하기 위해 필요
using EnvDTE80; // DTE2를 사용하기 위해 필요
using Microsoft.VisualStudio.Shell;
using System.Windows;
using System.Windows.Controls;

namespace WIT
{
    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class ToolWindow1Control : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1Control"/> class.
        /// </summary>
        public ToolWindow1Control()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void GetActiveFilePath_Click(object sender, RoutedEventArgs e)
        {
            // Toolkit을 사용하여 DTE2 서비스를 비동기적으로 가져옵니다.
            // DTE (Development Tools Environment)는 Visual Studio의 핵심 자동화 객체입니다.
            DTE2 dte = await VS.GetServiceAsync<DTE, DTE2>();

            if (dte?.ActiveDocument != null)
            {
                // ActiveDocument 속성에서 FullName (전체 경로)을 가져옵니다.
                string filePath = dte.ActiveDocument.FullName;
                FilePathTextBlock.Text = filePath;
            }
            else
            {
                // 열려있는 파일이 없을 경우 메시지를 표시합니다.
                FilePathTextBlock.Text = "활성화된 문서가 없습니다.";
            }
        }
    }
}