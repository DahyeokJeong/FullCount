# 02 · 투구와 타격

## 첫 타격 루프

1. **적용 대상** — `BaseballPrototype` 씬의 투구, 스윙, 리셋.
2. **적용 이유** — 고정 탑뷰에서 타이밍을 바꿔 타구 방향을 반복 비교한다.
3. **구조 · 설계값** — 투구 시작점 (0, 0.181, 4), 공 지름 0.36, 기본 구속 5 Unity 유닛/초. 배트 손잡이 축은 (-1.15, 0.18, -0.55)이며 기본 스윙은 Y축 55°에서 -75°까지 0.26초다. 활성 투구당 투구 재실행과 두 번째 스윙을 차단한다.
4. **장단점** — 공과 배트의 접촉으로 방향과 속도가 결정된다. 체감 타격 구간은 구속과 물리 스텝에 영향을 받는다. 공이 보드를 벗어나도 리셋 버튼으로 반복할 수 있다.
5. **구현 과정** — `Steel Ball`에 동적 Rigidbody와 SphereCollider를 두고 투구 시 홈 방향 선속도와 굴림 각속도를 넣는다. `Bat Pivot`의 키네마틱 Rigidbody를 고정 스텝에서 MoveRotation으로 돌리고 자식 CapsuleCollider가 공을 친다. 리셋은 진행 상태, 공의 선속도·각속도·위치·회전과 배트 각도를 초기화한다. 하단의 한국어 버튼은 기존 Input System의 UI 액션을 사용한다.
6. **개선 사항** — 구속 충전, 좌우 휘어짐, CPU, 카운트, 주루와 룰렛은 후속 단계로 남긴다.

## Hierarchy와 Inspector

- `Baseball Prototype` → Ball은 `Steel Ball` Rigidbody, Ball Collider는 같은 오브젝트의 SphereCollider, Mound는 `Pitch Origin`, Bat은 `Bat Pivot` Rigidbody다.
- Pitch Button, Swing Button, Reset Button은 각각 `Controls/Pitch`, `Controls/Swing`, `Controls/Reset`이다. 버튼 이벤트는 컨트롤러가 Play 시작 시 연결한다. Pitch Speed, Ready Angle, Finish Angle, Swing Duration은 같은 Inspector에서 조절한다.
- `EventSystem`에는 `InputSystemUIInputModule`을 두고 프로젝트의 `InputSystem_Actions` 중 `UI/Point`, `UI/Click`, `UI/Navigate`, `UI/Submit`, `UI/Cancel`을 연결한다.

## Play 모드 재현 순서

1. `Assets/FullCount/Scenes/BaseballPrototype.unity`를 열고 Game 뷰를 16:9로 설정한다. 전체 보드와 세 버튼이 보이고 처음에는 스윙만 비활성인지 확인한다.
2. 투구를 눌러 공이 중심선을 따라 홈 쪽으로 굴러오고 투구 버튼이 비활성되는지 확인한다.
3. 홈 바로 앞에서 스윙하고 타구를 본다. 리셋한 뒤 타이밍을 조금 앞당기거나 늦춰 다른 방향이 나오는지 확인한다. 스윙 버튼은 사용 직후 비활성된다.
4. 투구 직후 스윙해 헛스윙을 만들고, 스윙 도중 또는 공이 홈 뒤 보드 가장자리로 이탈한 뒤 리셋한다. 공과 배트가 시작 자세로 돌아오고 다시 직구가 나오는지 확인한다.
5. 모바일에서는 활성 Build Profile의 씬 목록에 프로토타입을 추가하고 해당 씬을 실행해 같은 순서를 터치로 반복한다.

## 검증 결과

- 프로젝트에 저장된 Unity 6000.3.27f1과 기존 URP·Input System 설정을 사용했다. 원본에서 배치 초기화가 실패해 임시 복사본의 Play 모드에서 버튼 클릭 이벤트를 호출해 검증했다.
- 직구가 중심선을 따라 5 유닛/초로 굴렀다. 스윙 시점을 달리했을 때 타구의 수평 속도 (X, Z)는 약 (-3.83, 9.28), (-1.14, 8.35)로 방향이 약 15° 달랐다.
- 투구와 스윙 버튼의 잠금, 스윙 도중 리셋, 헛스윙 뒤 보드 이탈, 공과 배트의 자세 복구 및 재투구를 확인했다.
- 프로젝트 입력 액션의 마우스와 터치 바인딩을 확인했다. Game 뷰의 실제 마우스 조작과 모바일 실기기 터치는 검증 예정이다.

## 2026-11-12 · Stadium 투구점 적용

- Stadium 프리팹의 PitchAnchor에 맞춰 투구 시작점은 (0,0.181,6)으로 변경했다. 현재 연결과 재검증 결과는 03-Stadium-Integration.md를 따른다.
