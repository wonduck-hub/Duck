# SARIF 압축 파일을 OneDrive에 저장하기

주간 분석 워크플로는 SARIF 파일이 생성되면 Microsoft 개인 계정의 OneDrive
`SARIF/` 폴더에 ZIP 파일로 업로드합니다. ZIP 안에는 원본 `.sarif` 파일 하나만
들어 있으며 원본 바이트를 그대로 보존합니다. 업로드 전에 SARIF를 UTF-8(BOM
포함 가능) JSON으로 읽어 버전이 `2.1.0`인지 확인합니다. 대상
폴더가 없으면 Microsoft Graph API로 자동 생성합니다. workflow 빌드 단계가
만든 동일한 timestamp를 ZIP 이름과 내부 경로에 사용합니다. 외부 파일명은
`DuckChat_sarif_<yyyyMMdd-HHmmss>.zip` 형식이며, 내부 파일 경로는
`DuckChat_<timestamp>/DuckChat_sarif_<timestamp>.sarif`입니다.
초 단위 timestamp를 파일명에 포함하므로 실행마다 별도 파일로 저장됩니다.
GitHub SARIF 아티팩트 업로드는
유지되며 Code Scanning 업로드는 수행하지 않습니다.

## 사전 준비

1. Microsoft 개인 계정으로 Azure Portal에서 앱 등록을 만듭니다. 개인 계정을
   사용한 앱 등록에는 Microsoft Entra 테넌트 생성 및 앱 등록 권한이 필요할
   수 있습니다. 권한이 없거나 조직 정책으로 제한된 경우 관리자에게
   테넌트와 앱 등록 권한을 요청하세요.
2. 앱의 인증 설정에서 공개 클라이언트/디바이스 코드 흐름을 허용합니다.
3. Microsoft Graph의 위임된 권한으로 `Files.ReadWrite`를 추가하고 동의를
   완료합니다.
4. 앱의 Application (client) ID를 복사합니다.
5. Python 3을 설치합니다. 인증과 업로드 스크립트는 Python 표준 라이브러리만
   사용합니다.

## 최초 디바이스 로그인

PowerShell에서 저장소 루트 기준으로 다음 명령을 실행합니다.

```powershell
$env:MS_CLIENT_ID = "<앱의 Application (client) ID>"
python SarifOneDrive\login.py
```

안내된 URL을 브라우저에서 열고 코드를 입력해 Microsoft 개인 계정 로그인과
권한 동의를 완료하세요. 로그인 토큰은
`SarifOneDrive\initial_tokens.json`에 저장됩니다. 이 파일은 Git에서
제외됩니다. **파일 내용을 출력하거나 커밋하거나 다른 사람과 공유하지
마세요.** 이 파일을 안전하게 보관하고, GitHub Secrets를 설정한 뒤에는
필요하지 않은 로컬 복사본을 안전하게 삭제하세요.

저장된 JSON의 `refresh_token` 값을 로컬에서 안전하게 확인해 아래 GitHub
Actions 시크릿 `ONEDRIVE_REFRESH_TOKEN`에 등록합니다. 토큰 자체를 로그,
명령 출력 또는 저장소 파일에 붙여 넣지 마세요. ZIP에는 SARIF만 들어가며
토큰이나 인증 정보는 포함되지 않습니다.

## GitHub Actions 시크릿

저장소의 **Settings → Secrets and variables → Actions**에서 다음 시크릿을
추가합니다.

| 시크릿 | 값 |
| --- | --- |
| `MS_CLIENT_ID` | Azure 앱 등록의 Application (client) ID |
| `ONEDRIVE_REFRESH_TOKEN` | 최초 디바이스 로그인의 `refresh_token` |
| `GH_SECRETS_PAT` | `ONEDRIVE_REFRESH_TOKEN` 시크릿을 갱신할 수 있는 GitHub PAT |

`GH_SECRETS_PAT`는 대상 저장소의 Actions secrets를 수정할 수 있는 최소
권한만 부여하세요. fine-grained PAT를 사용하는 경우 대상 저장소를 선택하고
Repository permissions의 **Secrets: Read and write** 권한을 설정합니다.
클래식 PAT는 저장소 접근 및 Actions 시크릿 변경에 필요한 권한이 있어야
합니다. PAT는 Actions 로그에 출력하지 말고 저장소에 저장하지 마세요.

## 워크플로 동작 및 토큰 갱신

`.github/workflows/weekly-sarif.yml`은 SARIF 파일이 있을 때 빌드 성공 여부와
관계없이 ZIP을 OneDrive에 업로드합니다. SARIF `2.1.0` 형식만 허용하며,
ZIP 내부에는 검증된 원본 `.sarif` 파일 하나만 저장됩니다. 빌드가
기록한 timestamp를 workflow 환경 변수로 업로더에 전달하여 SARIF 원본 경로,
외부 ZIP 파일명, 내부 디렉터리와 SARIF 파일명이 일치하게 유지됩니다.
빌드는 `CompilerResponseFile`로 Roslyn에 `/errorlog:...,version=2.1`을
전달하고, 진단 로그의 Csc `ResponseFiles` 항목에서 해당 response file이
사용됐는지 검사합니다. SARIF 검사 게이트는 JSON, `runs` 및 정확한 버전
`2.1.0`을 확인하며, 이 검사를 통과하지 않으면 업로드하지 않습니다.
GitHub SARIF 아티팩트 업로드는 유지하며 Code Scanning 업로드는 수행하지
않습니다.

업로드 대상은 OneDrive의 `SARIF/` 폴더입니다. 스크립트는 Microsoft Graph에서
폴더를 확인하고, 없으면 생성한 뒤 파일을 업로드합니다.

Microsoft가 refresh token을 회전해 새 토큰을 반환하면 스크립트는 업로드
성공 후 `GH_SECRETS_PAT`를 사용해 GitHub의 `ONEDRIVE_REFRESH_TOKEN` 시크릿을
갱신합니다. 따라서 PAT가 만료되었거나 권한이 부족하면 워크플로가 실패할
수 있습니다. 해당 PAT를 재발급하고 시크릿을 갱신하세요. 액세스 토큰,
refresh token 및 PAT는 출력되지 않도록 처리됩니다.

## 수동 실행

GitHub 저장소의 **Actions → Weekly DuckChat analysis → Run workflow**에서
수동 실행할 수 있습니다. 로컬에서 ZIP 업로드를 실행하려면 환경 변수
`MS_CLIENT_ID`, `ONEDRIVE_REFRESH_TOKEN`, `GITHUB_REPOSITORY`,
`GITHUB_SHA`를 설정한 다음 다음 명령을 실행하세요.

```powershell
python SarifOneDrive\scripts\upload_sarif_to_onedrive.py --sarif <SARIF 파일 경로>
```

새 refresh token이 반환될 수 있으므로 로컬 실행에도 GitHub 시크릿을
갱신하도록 `GH_SECRETS_PAT`를 설정해야 할 수 있습니다. 토큰 환경 변수와
명령 출력이 로그에 노출되지 않도록 주의하세요.
