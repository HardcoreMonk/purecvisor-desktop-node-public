# Hyper-V 브라우저 콘솔 spike

campaign: `scenario-pivot-20261008` (Task 2 조사, Task 3 probe, Task 4 판단)
상태: 조사 완료, probe 대기
일자: 2026-10-08

## 목적

ADR-0017(제안) 시나리오 S1은 "브라우저에서 ISO로 VM을 만들고, 브라우저 콘솔로 OS를 설치한다"이다. 지금 Web Console의
콘솔은 `vmconnect` handoff뿐이고, noVNC bridge(ADR-0010)는 guest 안의 VNC target이 있어야 켜진다. OS가 없는 새 VM의
화면을 브라우저에서 볼 방법이 없다. 이 spike는 Hyper-V만으로 브라우저 콘솔을 만들 경로가 있는지, 있다면 어느 방식이
저장소 경계 안에서 가장 작은지 정한다.

## Hyper-V 콘솔 경로 사실

- `vmconnect`는 호스트의 Virtual Machine Management Service가 듣는 TCP `2179`로 RDP 연결을 연다. 이 호스트에서도
  `0.0.0.0:2179`가 LISTENING이다(2026-10-08 `netstat`).
- 대상 VM은 RDP 연결 전에 보내는 preconnection PDU(MS-RDPEPS `RDP_PRECONNECTION_PDU_V2`)의 `wszPCB`에 VM GUID를 넣어
  고른다. 필드는 `cbSize`(u32), `Flags`(u32, 0), `Version`(u32, 2), `Id`(u32), `cchPCB`(u16), `wszPCB`(UTF-16LE)이고
  `cbSize = 18 + cchPCB * 2`다.
- 이 경로는 guest 네트워크와 무관하게 firmware 화면부터 보인다. OS 설치 화면을 브라우저로 옮기기에 맞는 경로다.
- 접속 계정은 호스트 Administrators와 Hyper-V Administrators 구성원이거나 VM별 접근 권한(`Grant-VMConnectAccess`)이
  있어야 한다고 알려져 있다. 원격 클라이언트에서는 CredSSP 정책 오류(연결 끊김 사유 `3848`) 사례가 있다.
- 서버가 어떤 보안 protocol(TLS, CredSSP)을 요구하는지는 공개 자료로 확인하지 못했다. Task 3 probe가 협상 응답으로 본다.

## 후보 비교

| 후보 | 구성 | 라이선스 | 저장소 경계 | 인증 | 판단 |
| --- | --- | --- | --- | --- | --- |
| A | 브라우저 RDP client(IronRDP web, WASM) + 제품 listener의 WebSocket relay(기존 noVNC bridge 확장, RDCleanPath 방식으로 X.224·TLS를 서버 쪽에서 처리) → `127.0.0.1:2179` + PCB | MIT 또는 Apache-2.0 | 안. 외부 서비스 설치 없음. relay는 C# `SslStream`으로 가능하고 native 호출이 필요 없다 | 서버가 CredSSP를 요구하면 브라우저에 Windows 계정을 받거나 서버 쪽 credential 처리가 필요하다. 새 설계 | Task 3 협상 결과에 달림 |
| B | Myrtille(Windows HTML5 RDP gateway, 2.1.0부터 Hyper-V 콘솔) | Apache-2.0 | 밖. IIS와 .NET Framework 4.5 별도 설치, installer handoff. 마지막 push 2024-03 | 같은 CredSSP 문제 | 제외 |
| C | Apache Guacamole `guacd` + preconnection blob | Apache-2.0 | 밖. `guacd`는 Linux daemon(Docker/WSL)이라 Windows 전용 경계와 Linux stack 금지에 걸림. FreeRDP 2 전환 뒤 PCB 감지 회귀 보고(GUACAMOLE-952) | 같은 CredSSP 문제 | 제외 |
| D | 제품 안에 C# RDP client를 직접 구현해 화면을 VNC/WebSocket으로 다시 냄 | - | 안이지만 RDP client 구현 규모가 큼 | 같은 CredSSP 문제 | 제외 |
| E | `2179`를 쓰지 않는 WMI 경로. 화면은 `Msvm_VirtualSystemManagementService.GetVirtualSystemThumbnailImage`(요청 크기의 RGB565 이미지)를 주기적으로 읽고, 입력은 `Msvm_Keyboard`(`TypeText`, `TypeKey`, `PressKey`, `ReleaseKey`, `TypeCtrlAltDel`)와 마우스 class로 보낸다 | Windows 기본 | 안. 이미 쓰는 WMI adapter와 Local API, Web Console만 쓴다 | 제품 service 권한 그대로. 브라우저에 Windows 계정이 필요 없다 | 화면 갱신률이 낮다(수 fps 예상). OS 설치·부팅 확인에는 충분할 수 있다. Task 3에서 thumbnail 읽기만 확인 |

A와 E만 저장소 경계 안이다. A는 화면 품질이 좋고 E는 인증·설치 부담이 없다. 둘 중 하나는 Task 3 probe로 정한다.

## Task 3 probe 절차

- VM: PCVCLI(설치본 service 경로)로 `pcv-it-console-spike`를 만들고 켠다. OS는 없어도 된다(firmware 화면). VM GUID는
  읽기로 얻는다. 보존 VM `pcv-guest-installed-04253-r1`은 건드리지 않는다.
- 2179 협상 probe(저장소 밖 scratch, 소켓만 사용): `127.0.0.1:2179`에 `RDP_PRECONNECTION_PDU_V2` 뒤 TPKT·X.224
  Connection Request와 `RDP_NEG_REQ`를 보낸다. 사례는 (1) 맞는 GUID와 requestedProtocols `SSL|HYBRID|HYBRID_EX`,
  (2) 맞는 GUID와 `SSL`만, (3) PCB 없음, (4) 틀린 GUID다. 응답의 `RDP_NEG_RSP` selectedProtocol이나
  `RDP_NEG_FAILURE` 코드, 연결 종료 여부를 기록한다. TLS handshake와 CredSSP는 시도하지 않고 계정 정보도 쓰지 않는다.
- WMI 화면 probe(E): 같은 VM에 `GetVirtualSystemThumbnailImage`(640×480)를 읽어 반환 코드와 byte 수를 기록하고 PNG로
  저장한다. 키보드·마우스 입력은 보내지 않는다.
- 정리: VM을 끄고 지운 뒤 `pcv-it-` VM `0`개를 확인한다. raw 결과는 `artifacts/console-spike-20261008/`에 둔다.

## Probe 결과

(Task 3에서 쓴다)

## 결론

(Task 4에서 쓴다)

## 출처

- [MS-RDPEPS: Sending the RDP_PRECONNECTION_PDU_V2](https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-rdpeps/73dca021-74a1-45b3-9805-327090344a1f)
- [mRemoteNG: Connect to virtual machine on Hyper-V](https://mremoteng.readthedocs.io/en/latest/howtos/vmrdp.html)
- [Cloudbase: open source web UI for Hyper-V console(FreeRDP)](https://www.slideshare.net/slideshow/free-rdp-hyperv/16514211)
- [Apache Jira GUACAMOLE-952](https://jira.apache.org/jira/browse/GUACAMOLE-952)
- [Guacamole 1.3.0 manual index(Preconnection PDU, Hyper-V)](https://guacamole.apache.org/doc/1.3.0/gug/book-index.html)
- [IronRDP](https://github.com/Devolutions/IronRDP), [ironrdp-web](https://mintlify.wiki/Devolutions/IronRDP/api/ironrdp-web)
- [Myrtille](https://sourceforge.net/projects/myrtille/), [MachPanel Myrtille Hyper-V 콘솔](https://kb.machsol.com/Print55811.aspx)
- [Microsoft KB 954357(CredSSP)](https://support.microsoft.com/kb/954357)
