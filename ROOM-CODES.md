# Six-digit LAN rooms — source 0.3.7/code37

Host: choose a character and Host Match. The waiting lobby displays a freshly generated six-digit room code. Share that code with the other player.

Join: choose Join Match, enter the host's six digits, then Join Room. The field starts empty every time this screen opens. There is no default code and no IP-address entry. Both devices must use the same Wi-Fi/LAN and identical builds.

Discovery uses bounded IPv4 UDP requests on port 27842, then connects the existing TCP transport on port 27841 automatically. The host stops advertising when a peer connects. Codes are randomly generated, including valid leading zeros; if discovery sees two hosts sharing a code, it rejects the ambiguous room and asks for a new room. Codes are LAN convenience identifiers, not secure authentication or internet relay matchmaking. No external service, account, purchase or publication is involved.

Android acquires a Wi-Fi multicast reception lock only during lobby discovery, releases it on connection/error/destruction, and merges normal Wi-Fi permissions into Unity's generated library manifest without replacing the activity. Guest Wi-Fi/client isolation, firewall restrictions, hotspot behavior and actual Android broadcast reception remain hardware acceptance checks.

Validation: 18 .NET checks passed (actual loopback UDP discovery -> automatic TCP connection and gameplay record, wrong/empty codes, timeouts, port conflicts, code formatting, idempotent manifest permission merge/activity preservation). Existing TCP regression suite passes 27 checks. Full source and UNITY_ANDROID compilation passed against installed Unity assemblies. The Android manifest helper was executed in a .NET test using an Editor-interface stub; no Android build or JNI lock execution is claimed. Logs are work/Room-Code-Tests.log, work/Lan-Transport-Room-Codes.log and work/Room-Codes-Android-Compile.log.

Current downloadable/root APK remains 0.3.5; it does not contain this feature or the preceding multiplayer opening update. Build and install source 0.3.7 on both devices before accepting the room-code flow. Unity batch licensing initialization remains the earlier native validation/build blocker; no new APK or phone testing occurred this turn. Backups: work/Before-room-codes. Source hashes: work/Room-Codes-Source-Manifest.json.
