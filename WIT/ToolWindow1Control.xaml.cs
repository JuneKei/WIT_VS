using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Npgsql; // PostgreSQL 연결을 위해 추가
using System; // Exception 클래스를 사용하기 위해 추가
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

// 'Task' 이름 충돌을 피하기 위해 별칭(alias)을 사용합니다.
using Task = System.Threading.Tasks.Task;

namespace WIT
{
    public partial class ToolWindow1Control : UserControl
    {
        // DB를 대신할 더미 데이터 저장소 (Key: 상대 경로, Value: 설명)
        private readonly Dictionary<string, string> _dummyData = new Dictionary<string, string>
        {
            { "WIT", "이것은 WIT 프로젝트 폴더입니다." },
            { @"WIT\ToolWindow1.cs", "도구 창의 메인 클래스 파일입니다." }
        };

        public ToolWindow1Control()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            VS.Events.SelectionEvents.SelectionChanged += SelectionEvents_SelectionChanged;
            await UpdatePathAsync();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            VS.Events.SelectionEvents.SelectionChanged -= SelectionEvents_SelectionChanged;
        }

        private void SelectionEvents_SelectionChanged(object sender, Community.VisualStudio.Toolkit.SelectionChangedEventArgs e)
        {
            _ = UpdatePathAsync();
        }

        // DB 연결 테스트 버튼 클릭 이벤트 핸들러
        private async void TestDbConnection_Click(object sender, RoutedEventArgs e)
        {
            DbStatusTextBlock.Text = "DB 연결 시도 중...";
            DbStatusTextBlock.Foreground = (Brush)FindResource(VsBrushes.WindowTextKey);

            string connString = "Host=192.168.45.67;Port=15432;Username=wit;Password=1234;Database=wit_db";

            try
            {
                // [수정] C# 7.3과 호환되는 전통적인 using 문으로 변경
                using (var conn = new NpgsqlConnection(connString))
                {
                    await conn.OpenAsync();

                    // 연결 성공 시
                    DbStatusTextBlock.Text = $"DB 연결 성공! (PostgreSQL v{conn.PostgreSqlVersion})";
                    DbStatusTextBlock.Foreground = Brushes.Green;

                    await conn.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                // 연결 실패 시
                DbStatusTextBlock.Text = $"DB 연결 실패: {ex.Message}";
                DbStatusTextBlock.Foreground = Brushes.Red;
            }
        }

        private async Task UpdatePathAsync()
        {
            PathTreeView.Items.Clear();

            SolutionItem selectedItem = await VS.Solutions.GetActiveItemAsync();
            if (selectedItem == null)
            {
                PathTreeView.Items.Add(new TreeViewItem { Header = "선택된 항목이 없습니다." });
                return;
            }

            Solution solution = await VS.Solutions.GetCurrentSolutionAsync();
            string rootPath = (solution != null) ? Path.GetDirectoryName(solution.FullPath) : null;

            if (rootPath == null || !selectedItem.FullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            {
                Project containingProject = FindContainingProject(selectedItem);
                rootPath = (containingProject != null) ? Path.GetDirectoryName(containingProject.FullPath) : Path.GetPathRoot(selectedItem.FullPath);
            }

            string fullPath = selectedItem.FullPath;
            string relativePath = fullPath;
            if (fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = fullPath.Substring(rootPath.Length + 1);
            }

            string[] pathParts = relativePath.Split(Path.DirectorySeparatorChar);
            ItemsControl parentNode = PathTreeView;

            for (int i = 0; i < pathParts.Length; i++)
            {
                string part = pathParts[i];
                string currentRelativePath = string.Join(Path.DirectorySeparatorChar.ToString(), pathParts.Take(i + 1));

                var headerPanel = new StackPanel { Orientation = Orientation.Vertical };

                var nameBlock = new TextBlock
                {
                    Text = part,
                    FontWeight = FontWeights.Bold,
                    Foreground = (Brush)FindResource(VsBrushes.WindowTextKey)
                };

                var descriptionBox = new TextBox
                {
                    Text = _dummyData.ContainsKey(currentRelativePath) ? _dummyData[currentRelativePath] : "",
                    Tag = currentRelativePath,
                    Margin = new Thickness(15, 2, 0, 0),
                    MinWidth = 250,
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    BorderBrush = (Brush)FindResource(VsBrushes.ToolWindowBorderKey),
                    Foreground = (Brush)FindResource(VsBrushes.WindowTextKey)
                };

                descriptionBox.LostFocus += DescriptionBox_LostFocus;

                headerPanel.Children.Add(nameBlock);
                headerPanel.Children.Add(descriptionBox);

                var newNode = new TreeViewItem
                {
                    Header = headerPanel,
                    IsExpanded = true
                };

                parentNode.Items.Add(newNode);
                parentNode = newNode;
            }
        }

        private void DescriptionBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string path = textBox.Tag as string;
                string description = textBox.Text;

                if (!string.IsNullOrEmpty(path))
                {
                    _dummyData[path] = description;
                }
            }
        }

        private Project FindContainingProject(SolutionItem item)
        {
            if (item == null) return null;
            if (item.Type == SolutionItemType.Project) return item as Project;
            return FindContainingProject(item.Parent);
        }
    }
}

