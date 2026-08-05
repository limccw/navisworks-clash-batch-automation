// NwdPublisher.cs
//
// NWD 파일을 게시(Publish)할 때, 배포 정책(End Date, 재저장 제한 등)을
// 자동으로 적용하여 Navisworks Freedom 사용자에게 안전하게 배포하기 위한 모듈.
//
// 배경: 협력사/현장에 NWD를 배포할 때 담당자가 수동으로 옵션을 체크하다 보니
// 보안 옵션 누락(End Date 미설정, 재저장 허용 등) 사고가 발생했었음.
// 이를 애드인 레벨에서 강제/자동화하여 휴먼 에러를 줄이는 것이 목적.

using System;
using Autodesk.Navisworks.Api;

namespace ClashAutomation
{
    /// <summary>
    /// NWD 게시 시 적용할 보안/배포 정책.
    /// </summary>
    public class NwdPublishPolicy
    {
        /// <summary>이 날짜 이후 파일을 열 수 없도록 제한.</summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>Navisworks Freedom에서 재저장을 허용할지 여부.</summary>
        public bool AllowResave { get; set; } = false;

        /// <summary>내보내기(Export) 허용 여부.</summary>
        public bool AllowExport { get; set; } = false;

        /// <summary>측정(Measure) 도구 허용 여부.</summary>
        public bool AllowMeasure { get; set; } = true;
    }

    /// <summary>
    /// 정책에 따라 NWD를 게시하는 퍼블리셔.
    /// 실무에서는 두 가지 배포 케이스를 구분해서 운용:
    ///   - Case 1: End Date만 설정, 재저장은 허용 (내부 협업용)
    ///   - Case 2: End Date + 재저장 금지 (외부/협력사 배포용, 통제 강함)
    /// </summary>
    public class NwdPublisher
    {
        private readonly Document _doc;

        public NwdPublisher(Document doc)
        {
            _doc = doc ?? throw new ArgumentNullException(nameof(doc));
        }

        public void Publish(string outputPath, NwdPublishPolicy policy)
        {
            ValidatePolicy(policy);

            // 실제 Navisworks API 호출부는 사내 배포 정책 세부사항을 포함하므로
            // 이 저장소에서는 정책 검증 로직과 구조만 공개합니다.
            //
            // var exporter = _doc.Export(...);
            // exporter.SecuritySettings.ExpirationDate = policy.ExpirationDate;
            // exporter.SecuritySettings.AllowResave = policy.AllowResave;
            // exporter.Save(outputPath);

            Console.WriteLine(
                $"[NwdPublisher] '{outputPath}' 게시 준비 완료 " +
                $"(만료일: {policy.ExpirationDate:yyyy-MM-dd}, " +
                $"재저장: {(policy.AllowResave ? "허용" : "금지")})");
        }

        private void ValidatePolicy(NwdPublishPolicy policy)
        {
            if (policy.ExpirationDate is null)
            {
                throw new InvalidOperationException(
                    "배포 정책에 End Date가 설정되지 않았습니다. " +
                    "만료일 없는 배포는 허용되지 않습니다.");
            }

            if (policy.ExpirationDate < DateTime.Today)
            {
                throw new InvalidOperationException("End Date가 오늘보다 과거일 수 없습니다.");
            }
        }

        /// <summary>외부(협력사) 배포용 기본 정책 — 통제 강함.</summary>
        public static NwdPublishPolicy ExternalDistributionPolicy(DateTime expirationDate) => new()
        {
            ExpirationDate = expirationDate,
            AllowResave = false,
            AllowExport = false,
            AllowMeasure = true,
        };

        /// <summary>내부 협업용 기본 정책 — 재저장 허용.</summary>
        public static NwdPublishPolicy InternalCollaborationPolicy(DateTime expirationDate) => new()
        {
            ExpirationDate = expirationDate,
            AllowResave = true,
            AllowExport = false,
            AllowMeasure = true,
        };
    }
}
