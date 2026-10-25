using System.Runtime.InteropServices;

namespace WpfPlugin;

/// <summary>
/// 엔진 C API (LotCADCore.dll) 로 가는 P/Invoke 래퍼.
///
/// 문자열은 전부 UTF-8 이다. C# 의 기본 마셜링은 ANSI 라 한글이 깨지므로
/// <c>StringMarshalling = StringMarshalling.Utf8</c> 을 명시한다.
///
/// DLL 이름에 확장자를 안 쓰면 OS 마다 알아서 찾는다(Windows .dll / Linux lib*.so).
/// 여기선 Windows 전용이지만 습관을 맞춰 둔다.
/// </summary>
internal static partial class Cad
{
    private const string Dll = "LotCADCore";

    // ── 명령 등록 ──
    // fn 은 C 함수 포인터다. C# 에선 [UnmanagedCallersOnly] 정적 메서드의 주소를 넘긴다.
    // 반환 0 = CAD_OK, 같은 이름·엔진 기본 명령 이름이면 CAD_E_DUPLICATE(음수).
    [LibraryImport(Dll, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int CAD_RegisterCommand(
        string name, string title, nint fn, nint user, uint owner);

    // ── UI 등록 ──
    // kind: 0=메뉴항목 1=구분선 2=툴바버튼 3=리본버튼 4=패널
    // 반환 0 = CAD_OK, outItemId = 새 항목 id(개별 제거용 — 언로드 때는 엔진이 owner 로 지운다).
    [LibraryImport(Dll, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int CAD_AddUiItem(
        int kind, string path, string title, string command, string icon, uint owner, out uint outItemId);

    // ── 엔티티 속성(특성창) ──
    // kind: 0=실수 1=정수 2=참거짓 3=문자(읽기전용). 반환 0 = CAD_OK, 같은 (owner,type,key) CAD_E_DUPLICATE(음수).
    [LibraryImport(Dll, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int CAD_AddEntityProperty(
        string owner, string type, string key, string label,
        int kind, double minV, double maxV, uint pluginId);

    [LibraryImport(Dll)]
    internal static partial int CAD_SetOnPluginEntityChanged(nint cb);   // 0 = CAD_OK

    // ── 형상 ──
    // 점은 CAD_GeVec3(double x,y,z) 배열 — double[] 에 점마다 3개씩 넣으면 같은 배치다. pointCount = 점 개수.
    // normals·opt 가 0 이면 삼각형별 flat 법선·기본 선택. 반환 0 = CAD_OK.
    [LibraryImport(Dll)]
    internal static partial int CAD_CreateMesh(
        [In] double[] points, uint pointCount, [In] uint[] indices, uint indexCount, nint normals, nint opt, out uint outId);

    // 점은 객체 자기 좌표(사용자가 옮긴 자리 위에 얹힘). 반환 0 = CAD_OK.
    [LibraryImport(Dll)]
    internal static partial int CAD_ReplaceMesh(
        uint id, [In] double[] points, uint pointCount, [In] uint[] indices, uint indexCount, nint normals);

    // 반환 0 = CAD_OK(성공이 0 이다 — bool 처럼 쓰지 말 것), 없는 객체 CAD_E_BAD_HANDLE.
    [LibraryImport(Dll, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int CAD_SetPluginTag(uint id, string owner, string type, string data);

    // ── 뷰 ──
    [LibraryImport(Dll)]
    internal static partial int CAD_ZoomExtents();   // 0 = CAD_OK(다음 틱에 적용)
}
