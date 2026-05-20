# 방치형 카드 수집 RPG 문서

이 폴더는 세로형 자동전투 기반 방치형 게임의 기획, 데이터, 구현 기준을 관리한다.

## 문서 목록

- [MVP_SPEC.md](MVP_SPEC.md): 1차 MVP 게임 기획서
- [DATA_TABLES.md](DATA_TABLES.md): 초기 영웅, 적, 스테이지, 보스, 뽑기, 저장 데이터
- [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md): Unity 구현 순서와 완료 기준
- [TARGET_REFERENCE.md](TARGET_REFERENCE.md): 목표 게임 감각, 성장 능력치, 뽑기/재화 구조

## 현재 구현 기준

- 자동전투는 100마리 처치 진행, 보스 제한 시간, 영웅/몬스터 이동, 대상 락온, 데미지 미터기를 포함한다.
- 성장은 골드 기반 능력 성장 7종, 영웅 레벨/성급, 장비 뽑기/장착/분해/레벨/성급, 초월 옵션, 계정 레벨 특성으로 나뉜다.
- 영웅 탭은 편성, 특성, 기본 정보, 장비, 초월 흐름을 우선 구현한다. 토템, 룬, 성물, 던전 배틀, 상점 결제는 아직 확장 대상이다.
- 레퍼런스 게임의 화면 밀도와 성장 구조는 참고하되, 원본 캐릭터, 아이콘, 명칭, 아트, 픽셀 단위 배치는 복제하지 않는다.

## 레퍼런스 이미지

- [target_reference_hero_assemble_style.jpg](references/target_reference_hero_assemble_style.jpg)

이 이미지는 구조와 감각을 기억하기 위한 내부 레퍼런스다. 원본 캐릭터, 아이콘, UI 아트, 이펙트를 그대로 복제하지 않는다.
