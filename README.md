# Navisworks Clash Batch Automation

Navisworks 기반 클래시(간섭) 검토 및 NWD 배포 과정을 자동화하는 애드인 코드 샘플입니다.

플랜트 EPC 프로젝트에서는 프로젝트당 수백 개의 Clash Test가 등록되어 있고, 담당자가 매번 수동으로 하나씩 Run 하고 결과를 확인해야 했습니다. 또한 협력사·현장에 NWD 파일을 배포할 때 End Date, 재저장 제한 같은 보안 옵션을 수동 체크하다 보니 누락 사고가 발생하기도 했습니다. 이 두 가지 반복 작업을 자동화한 애드인의 핵심 로직을 정리했습니다.

> ⚠️ Navisworks 실행 환경(Autodesk.Navisworks.Api)이 필요해 이 저장소만으로는 빌드/실행되지 않습니다. 실제 프로젝트 배포 정책의 세부값은 제거하고, **구조와 로직**을 보여주는 코드 샘플입니다.

## 구성

| 파일 | 역할 |
|---|---|
| `src/ClashBatchRunner.cs` | 등록된 전체(또는 선택) Clash Test를 순차 실행하고 결과를 CSV로 요약 |
| `src/NwdPublisher.cs` | NWD 게시 시 End Date·재저장 제한 등 보안 정책을 강제 적용 |

## 설계 포인트

### 1. 클래시 배치 실행 (`ClashBatchRunner`)
- 야간/새벽 시간대에 전체 Clash Test를 자동 실행 → 담당자는 아침에 요약 리포트만 확인
- `RunAll(onlyTestNames)`로 특정 테스트만 선택 실행도 가능
- 결과를 `New / Active / Approved / Resolved` 상태별로 집계해 CSV로 저장

### 2. NWD 배포 정책 자동화 (`NwdPublisher`)
- 배포 시나리오를 두 가지로 표준화:
  - **내부 협업용**: End Date만 적용, 재저장 허용
  - **외부(협력사) 배포용**: End Date + 재저장 금지, 통제 강함
- `ValidatePolicy()`에서 **End Date 미설정 시 게시 자체를 막아** 휴먼 에러를 원천 차단
- 두 시나리오를 팩토리 메서드(`ExternalDistributionPolicy`, `InternalCollaborationPolicy`)로 제공해 담당자가 옵션을 하나씩 체크할 필요 없이 시나리오만 선택하면 되도록 설계

## Tech Stack

`C#` `Autodesk.Navisworks.Api` `.NET Framework (Add-in)`
