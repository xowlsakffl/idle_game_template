# Unity 구현 계획

## 개발 원칙

- 먼저 숫자 루프를 만든다.
- 그래픽은 임시 도형과 텍스트로 시작한다.
- 모든 주요 수치는 코드에 박아 넣지 않고 데이터로 분리할 수 있게 설계한다.
- 1차 MVP에서는 온라인 기능을 넣지 않는다.

## 권장 Unity 설정

- Unity 버전: LTS 버전 사용
- 템플릿: 2D 또는 Universal 2D
- 화면 방향: Portrait
- 목표 빌드: Android
- 입력: 터치와 마우스 클릭 모두 대응

## 폴더 구조 초안

```text
Assets/
  Scenes/
    Main.unity
  Scripts/
    Battle/
      BattleManager.cs
      HeroUnit.cs
      EnemyUnit.cs
    Data/
      HeroData.cs
      EnemyData.cs
      StageData.cs
    Economy/
      CurrencyManager.cs
      RewardManager.cs
    Gacha/
      GachaManager.cs
    Save/
      SaveManager.cs
    UI/
      MainHUD.cs
      HeroPanel.cs
      GachaPanel.cs
  ScriptableObjects/
    Heroes/
    Enemies/
    Stages/
  Prefabs/
    HeroUnit.prefab
    EnemyUnit.prefab
    UI/
```

## 1단계: 프로젝트 기본 구성

완료 기준:

- Unity 프로젝트 생성
- 세로 화면 Game View 설정
- `Main` 씬 생성
- 기본 UI Canvas 생성
- 상단 재화/스테이지 영역, 중앙 전투 영역, 하단 버튼 영역 배치

## 2단계: 데이터 정의

완료 기준:

- `HeroData` 정의
- `EnemyData` 정의
- `StageData` 정의
- 영웅 3명 데이터 입력
- 적 5종 데이터 입력
- 스테이지 20개 데이터 입력

초기에는 ScriptableObject를 쓰는 것이 좋다. 단, 더 빠르게 만들고 싶다면 코드 배열로 시작한 뒤 나중에 ScriptableObject로 옮겨도 된다.

## 3단계: 자동전투

완료 기준:

- 전투 시작 시 현재 스테이지 적 생성
- 영웅 3명이 각자의 공격 주기마다 적 HP 감소
- 적 HP UI 표시
- 적 사망 시 골드 지급
- 적 사망 후 다음 적 자동 생성
- 처치 수 10 달성 시 다음 스테이지 이동

## 4단계: 골드와 레벨업

완료 기준:

- 골드 보유량 표시
- 영웅별 레벨 표시
- 영웅별 공격력 표시
- 레벨업 버튼 구현
- 골드가 부족하면 레벨업 불가
- 레벨업 후 전투 속도가 체감 가능하게 빨라짐

## 5단계: 뽑기

완료 기준:

- 1회 뽑기 버튼
- 10회 뽑기 버튼
- 확률에 따라 Common, Rare, Epic 결과 출력
- 무작위 영웅 조각 지급
- 영웅별 조각 수 표시

주의:

뽑기를 캐릭터 해금 시스템으로 만들면 예외 처리가 늘어난다. 1차 MVP에서는 모든 영웅을 기본 보유시키고, 뽑기는 조각 획득으로 제한하는 편이 낫다.

## 6단계: 로컬 저장

완료 기준:

- 골드 저장
- 현재 스테이지 저장
- 영웅 레벨 저장
- 영웅 조각 저장
- 앱 재시작 후 데이터 복원

## 7단계: 오프라인 보상

완료 기준:

- 종료 또는 백그라운드 진입 시 마지막 접속 시간 저장
- 재접속 시 시간 차이 계산
- 최대 8시간까지만 보상 계산
- 오프라인 보상 팝업 표시
- 수령 시 골드 증가

## 8단계: MVP QA

확인할 항목:

- 첫 실행에서 오류 없이 전투가 시작되는가
- 5분 플레이 안에 레벨업과 뽑기를 모두 경험할 수 있는가
- 스테이지가 정상적으로 증가하는가
- 골드가 음수가 되지 않는가
- 저장 후 재실행해도 데이터가 유지되는가
- 기기 시간을 조작했을 때 오프라인 보상이 비정상적으로 커지지 않는가

## 가장 먼저 만들 코드 순서

```text
1. HeroData
2. EnemyData
3. StageData
4. CurrencyManager
5. BattleManager
6. HeroUnit
7. EnemyUnit
8. HeroPanel
9. GachaManager
10. SaveManager
```

## 리스크

| 리스크 | 이유 | 대응 |
| --- | --- | --- |
| 밸런스가 너무 빠르게 무너짐 | 방치형은 수치 증가가 핵심이라 작은 공식 차이가 크게 벌어짐 | 20 스테이지까지만 먼저 조정 |
| UI 작업량 증가 | 수집 RPG는 버튼, 팝업, 목록이 많음 | 1차 MVP는 한 화면 중심으로 제한 |
| 뽑기 복잡도 증가 | 천장, 픽업, 중복, 등급 연출이 붙으면 커짐 | 조각 지급만 구현 |
| 저장 구조 재작업 | PlayerPrefs는 확장성이 낮음 | MVP 이후 JSON 저장으로 이전 |
| 재미 검증 실패 | 자동전투만으로는 밋밋할 수 있음 | 성장 속도와 보상 빈도를 먼저 튜닝 |

## 다음 결정 사항

아직 결정해야 할 것은 다음이다.

- 영웅 비주얼 방향: 임시 도형, 픽셀, SD 캐릭터 중 선택
- 게임명
- 세계관 톤: 판타지, 현대, SF 중 선택
- 장기 성장 축: 승급, 장비, 스킬, 유물 중 무엇을 먼저 붙일지

이 결정들은 1차 MVP 구현을 막지는 않는다.
