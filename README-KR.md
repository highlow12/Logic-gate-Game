# Logic Gate Game

이 프로젝트는 불완전한 로직 게이트를 완성하여 주어진 진리표와 일치시키는 것을 목표로 하는 논리 게이트 게임입니다. 여러 스테이지로 구성되어 있으며, 각 스테이지는 특정 진리표와 일부가 비어있는 로직 게이트 회로로 이루어져 있습니다.

## 주요 기능

  * **카르노 맵 기반의 랜덤 로직 게이트 제너레이터**: 카르노 맵을 활용하여 다양한 형태의 랜덤 로직 게이트 스테이지를 생성합니다.
  * **부울 대수 식을 이용한 맵 제너레이터**: 부울 대수 식을 기반으로 게임 맵(스테이지)을 생성하는 기능을 제공합니다.
  * **플레이 가능한 게임**: 사용자가 직접 로직 게이트를 조작하며 스테이지를 해결하는 게임 플레이 기능을 제공합니다.

## 시작하기

프로젝트를 로컬 환경에서 설정하고 실행하는 방법을 안내합니다.

1. **Unity 설치**:
   - 이 프로젝트는 Unity 6000.0.47f1 버전을 사용합니다. Unity Hub를 통해 해당 버전을 설치하세요.

2. **프로젝트 클론**:
   - Git을 사용하여 프로젝트를 클론합니다:
     ```bash
     git clone https://github.com/highlow12/Logic-gate-Game.git
     ```

3. **Unity로 프로젝트 열기**:
   - Unity Hub를 열고 "Open" 버튼을 클릭하여 클론한 프로젝트 폴더를 선택합니다.
   - Unity가 프로젝트를 로드할 때 필요한 종속성을 자동으로 설치합니다.

4. **플레이 모드 실행**:
   - Unity 에디터에서 상단의 "Play" 버튼을 클릭하여 게임을 실행할 수 있습니다.

5. **빌드 및 실행** (선택 사항):
   - 게임을 빌드하려면 "File > Build Settings"로 이동하여 플랫폼을 선택하고 "Build" 버튼을 클릭하세요.

## 기여 방법

프로젝트에 기여하고 싶으신 분들을 위한 가이드라인입니다.

1. **버그 신고**:
   - [GitHub Issues](https://github.com/<repository-owner>/<repository-name>/issues)를 방문하여 새로운 이슈를 생성하세요.
   - 이슈를 생성할 때, 문제를 재현할 수 있는 단계와 관련 스크린샷 또는 로그를 포함해주세요.

2. **기능 제안**:
   - 새로운 기능 아이디어가 있다면, [GitHub Issues](https://github.com/highlow12/Logic-gate-Game/issues)를 통해 제안해주세요.
   - 제안 내용을 명확히 설명하고, 가능하다면 예상되는 결과나 이점도 포함해주세요.

3. **코드 기여**:
   - 프로젝트를 포크(Fork)하고 로컬 환경에 클론(Clone)합니다.
   - 새로운 브랜치를 생성하여 작업을 시작하세요:
     ```bash
     git checkout -b feature/your-feature-name
     ```
   - 작업을 완료한 후, 변경 사항을 커밋하고 원격 저장소에 푸시(Push)합니다:
     ```bash
     git push origin feature/your-feature-name
     ```
   - [Pull Request](https://github.com/highlow12/Logic-gate-Game/pulls)를 생성하여 변경 사항을 제출하세요.

4. **기타 기여 방법**:
   - 문서화 개선, 테스트 작성, 또는 기타 방법으로도 기여할 수 있습니다.

기여해 주셔서 감사합니다!

## 라이선스 (License)

이 프로젝트는 **GNU General Public License v3.0** 하에 배포됩니다. 자세한 내용은 [LICENSE](./LICENSE) 파일을 참고하십시오.

## 문의

프로젝트에 대한 질문이나 피드백은 [GitHub Issues](https://github.com/<repository-owner>/<repository-name>/issues)를 통해 `question` 라벨은 달아서 주세요