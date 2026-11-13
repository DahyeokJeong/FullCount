# 집으로 가져갈 경기장 파일

이 폴더 전체를 `Stadium.zip`으로 압축해 옮긴 뒤, 집에서 먼저 압축을 푼다.

## 파일 구성

- `Stadium.blend`: Blender 원본. 구멍 14개만 아래로 관통하며 나머지 경기장 바닥과 벽은 유지되어 있다. 바닥 텍스처는 파일 안에도 포함했다.
- `Stadium.fbx`: Unity에 넣을 경기장 모델. 벽과 구멍 안쪽 벽 등은 별도 오브젝트다.
- `Textures/Field_BaseColor.png`: 잔디·흙·라인·베이스·KBO Bold 글씨가 포함된 바닥 텍스처.
- `Unity/Editor/StadiumImportTools.cs`: Unity에서 재질과 좌표를 맞춘 프리팹을 만드는 도우미.

## Blender에서 수정

Blender 5.3으로 `Stadium.blend`를 연다. 집의 Blender가 더 오래된 버전이면 5.3 이상으로 맞추는 것이 좋다. 원본을 수정할 때는 다른 이름으로 저장해 배포본을 보관한다.

## 집의 Unity 프로젝트에 적용

1. Unity 프로젝트의 `Assets` 안에 `Stadium` 폴더를 만든다.
2. 그 안에 `Stadium.fbx`, `Textures` 폴더, `Unity` 폴더를 함께 복사한다. `.blend` 원본은 Assets 밖에 보관한다.
3. 임포트와 스크립트 컴파일이 끝나면 상단 메뉴에서 `Tools > Baseball Pinball > Create Stadium Prefab`을 선택한다.
4. Assets에 복사한 `Stadium.fbx`를 선택한다. 재질과 좌표가 맞춰진 `BaseballPinball_Stadium.prefab`이 생성된다. URP Lit 또는 Built-in Standard용이며 HDRP는 별도 변환이 필요하다.
5. 프리팹을 씬에 넣고 Position=(0,0,0), Rotation=(0,0,0), Scale=(1,1,1)에서 시작한다.

도우미는 콜라이더와 플레이 동작을 추가하지 않는다. Field_PlaySurface의 관통 구멍이 메워지지 않도록 바닥 MeshCollider는 Convex를 끈 정적 형상으로 설정한다. 구멍별 판정 트리거와 관중석 벽의 핀볼 반발 기능은 직접 설정한다.

필드 표면 높이가 Y=0이다. 회수용 Unity Plane은 Position=(24,-3,24), 기본 Plane 기준 Scale=(8,1,8)로 두면 80×80 크기가 된다. 일반 콜라이더로 공을 받고, 구멍별 트리거에서 먼저 판정한 뒤 Plane 충돌 이벤트에서 회수한다. 공이 떨어지려면 Rigidbody의 중력을 켜고 Y축 위치 고정을 풀어야 한다.

## 제작 기준

- 외야 반지름 60, 마운드/투구점 평면 좌표 (15,15).
- 모든 구멍은 아래까지 관통. 색상 안쪽 벽 깊이는 2.
- 수비 구멍 7개, 외야 HIT/OUT 구멍 5개, 파울 구멍 2개.
- 관중석 경계 벽 높이 4, 외야펜스 높이 7.
- 금속성 없는 플라스틱 재질. 바닥 Smoothness 0.66, 벽 0.73.
- 의자·관중석 계단·전광판·수비수 모형은 포함하지 않았다.
- Unity 실제 플레이 테스트는 아직 수행하지 않았다.
