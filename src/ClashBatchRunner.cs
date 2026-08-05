// ClashBatchRunner.cs
//
// Navisworks에 등록된 다수의 Clash Test를 순차적으로 배치 실행하고,
// 결과를 요약 리포트(CSV/JSON)로 저장하는 배치 러너.
//
// 실제 프로젝트에서는 SSHN, Baltic 등 프로젝트별로 수백 개의 Clash Test가
// 등록되어 있어, 담당자가 매번 하나씩 Run 하는 대신 야간/새벽 시간대에
// 전체를 자동 실행하고 아침에 결과 요약만 확인하는 용도로 사용합니다.
//
// 이 코드는 사내 프로젝트 데이터/경로 의존성을 제거한 구조 예시이며,
// Autodesk.Navisworks.Api 참조가 필요해 이 저장소 단독으로는 빌드되지 않습니다.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;

namespace ClashAutomation
{
    /// <summary>
    /// 하나의 Clash Test 실행 결과를 담는 요약 모델.
    /// </summary>
    public class ClashTestResult
    {
        public string TestName { get; set; } = string.Empty;
        public int NewClashCount { get; set; }
        public int ActiveClashCount { get; set; }
        public int ApprovedCount { get; set; }
        public int ResolvedCount { get; set; }
        public DateTime RunAt { get; set; }
    }

    /// <summary>
    /// 등록된 전체 Clash Test를 순회하며 배치 실행하는 러너.
    /// </summary>
    public class ClashBatchRunner
    {
        private readonly Document _doc;

        public ClashBatchRunner(Document doc)
        {
            _doc = doc ?? throw new ArgumentNullException(nameof(doc));
        }

        /// <summary>
        /// DocumentClash 하위에 등록된 모든 Clash Test를 실행하고 결과를 수집한다.
        /// </summary>
        public List<ClashTestResult> RunAll(IEnumerable<string>? onlyTestNames = null)
        {
            var clashData = _doc.GetClash();
            var results = new List<ClashTestResult>();

            var tests = clashData.TestsData.Tests.OfType<ClashTest>();

            if (onlyTestNames != null)
            {
                var filter = new HashSet<string>(onlyTestNames);
                tests = tests.Where(t => filter.Contains(t.DisplayName));
            }

            foreach (var test in tests)
            {
                var result = RunSingleTest(clashData, test);
                results.Add(result);

                Console.WriteLine(
                    $"[{result.RunAt:HH:mm:ss}] {result.TestName} 완료 " +
                    $"(신규 {result.NewClashCount} / 활성 {result.ActiveClashCount})");
            }

            return results;
        }

        private ClashTestResult RunSingleTest(DocumentClash clashData, ClashTest test)
        {
            clashData.TestsData.TestsUpdate(test);

            var results = test.Children.OfType<ClashResult>().ToList();

            return new ClashTestResult
            {
                TestName = test.DisplayName,
                NewClashCount = results.Count(r => r.Status == ClashResultStatus.New),
                ActiveClashCount = results.Count(r => r.Status == ClashResultStatus.Active),
                ApprovedCount = results.Count(r => r.Status == ClashResultStatus.Approved),
                ResolvedCount = results.Count(r => r.Status == ClashResultStatus.Resolved),
                RunAt = DateTime.Now,
            };
        }

        /// <summary>
        /// 배치 실행 결과를 CSV로 저장한다. (일일/야간 배치 후 담당자 공유용)
        /// </summary>
        public void ExportSummary(List<ClashTestResult> results, string outputPath)
        {
            var lines = new List<string>
            {
                "TestName,NewClashCount,ActiveClashCount,ApprovedCount,ResolvedCount,RunAt"
            };

            lines.AddRange(results.Select(r =>
                $"{r.TestName},{r.NewClashCount},{r.ActiveClashCount}," +
                $"{r.ApprovedCount},{r.ResolvedCount},{r.RunAt:yyyy-MM-dd HH:mm:ss}"));

            File.WriteAllLines(outputPath, lines, System.Text.Encoding.UTF8);
        }
    }
}
