// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       EnumeratedTypes.cs
// ┃  PROJECT:    Utility.WinApi
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 8:02 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

namespace JLR.Utility.WinApi.Error
{
	/// <summary>
	/// Indicates success or failure.
	/// </summary>
	public enum Severity : uint
	{
		/// <summary>Success</summary>
		Success = 0,

		/// <summary>Failure</summary>
		Error = 1
	}

	/// <summary>
	/// Specifies the success codes (SCODEs) for non-error HRESULTs.
	/// </summary>
	public enum SuccessCode : uint
	{
		Ok    = 0,
		False = 1
	}

	/// <summary>
	/// Specifies the software component that defines an error code.
	/// </summary>
	public enum Facility : uint
	{
		NULL                                     = 0,
		RPC                                      = 1,
		DISPATCH                                 = 2,
		STORAGE                                  = 3,
		ITF                                      = 4,
		WIN32                                    = 7,
		WINDOWS                                  = 8,
		SSPI                                     = 9,
		SECURITY                                 = 9,
		CONTROL                                  = 10,
		CERT                                     = 11,
		INTERNET                                 = 12,
		MEDIASERVER                              = 13,
		MSMQ                                     = 14,
		SETUPAPI                                 = 15,
		SCARD                                    = 16,
		COMPLUS                                  = 17,
		AAF                                      = 18,
		URT                                      = 19,
		ACS                                      = 20,
		DPLAY                                    = 21,
		UMI                                      = 22,
		SXS                                      = 23,
		WINDOWS_CE                               = 24,
		HTTP                                     = 25,
		USERMODE_COMMONLOG                       = 26,
		WER                                      = 27,
		USERMODE_FILTER_MANAGER                  = 31,
		BACKGROUNDCOPY                           = 32,
		WIA                                      = 33,
		CONFIGURATION                            = 33,
		STATE_MANAGEMENT                         = 34,
		METADIRECTORY                            = 35,
		WINDOWSUPDATE                            = 36,
		DIRECTORYSERVICE                         = 37,
		GRAPHICS                                 = 38,
		SHELL                                    = 39,
		NAP                                      = 39,
		TPM_SERVICES                             = 40,
		TPM_SOFTWARE                             = 41,
		UI                                       = 42,
		XAML                                     = 43,
		WINDOWS_SETUP                            = 48,
		PLA                                      = 48,
		FVE                                      = 49,
		FWP                                      = 50,
		WINRM                                    = 51,
		NDIS                                     = 52,
		USERMODE_HYPERVISOR                      = 53,
		CMI                                      = 54,
		USERMODE_VIRTUALIZATION                  = 55,
		USERMODE_VOLMGR                          = 56,
		BCD                                      = 57,
		USERMODE_VHD                             = 58,
		SDIAG                                    = 60,
		WINPE                                    = 61,
		WEBSERVICES                              = 61,
		WPN                                      = 62,
		WINDOWS_STORE                            = 63,
		INPUT                                    = 64,
		EAP                                      = 66,
		WINDOWS_DEFENDER                         = 80,
		OPC                                      = 81,
		XPS                                      = 82,
		RAS                                      = 83,
		POWERSHELL                               = 84,
		MBN                                      = 84,
		EAS                                      = 85,
		P2P_INT                                  = 98,
		P2P                                      = 99,
		DAF                                      = 100,
		BLUETOOTH_ATT                            = 101,
		AUDIO                                    = 102,
		VISUALCPP                                = 109,
		SCRIPT                                   = 112,
		PARSE                                    = 113,
		BLB                                      = 120,
		BLB_CLI                                  = 121,
		WSBAPP                                   = 122,
		BLBUI                                    = 128,
		USN                                      = 129,
		USERMODE_VOLSNAP                         = 130,
		WSB_ONLINE                               = 133,
		ONLINE_ID                                = 134,
		DLS                                      = 153,
		SOS                                      = 160,
		DEBUGGERS                                = 176,
		USERMODE_SPACES                          = 231,
		SPP                                      = 256,
		RESTORE                                  = 256,
		DMSERVER                                 = 256,
		DEPLOYMENT_SERVICES_SERVER               = 257,
		DEPLOYMENT_SERVICES_IMAGING              = 258,
		DEPLOYMENT_SERVICES_MANAGEMENT           = 259,
		DEPLOYMENT_SERVICES_UTIL                 = 260,
		DEPLOYMENT_SERVICES_BINLSVC              = 261,
		DEPLOYMENT_SERVICES_PXE                  = 263,
		DEPLOYMENT_SERVICES_TFTP                 = 264,
		DEPLOYMENT_SERVICES_TRANSPORT_MANAGEMENT = 272,
		DEPLOYMENT_SERVICES_DRIVER_PROVISIONING  = 278,
		DEPLOYMENT_SERVICES_MULTICAST_SERVER     = 289,
		DEPLOYMENT_SERVICES_MULTICAST_CLIENT     = 290,
		DEPLOYMENT_SERVICES_CONTENT_PROVIDER     = 293,
		LINGUISTIC_SERVICES                      = 305,
		WEB                                      = 885,
		WEB_SOCKET                               = 886,
		AUDIOSTREAMING                           = 1094,
		ACCELERATOR                              = 1536,
		MOBILE                                   = 1793,
		WMAAECMA                                 = 1996,
		DIRECTMUSIC                              = 2168,
		DIRECT3D10                               = 2169,
		DXGI                                     = 2170,
		DXGI_DDI                                 = 2171,
		DIRECT3D11                               = 2172,
		LEAP                                     = 2184,
		AUDCLNT                                  = 2185,
		WINCODEC_DWRITE_DWM                      = 2200,
		DIRECT2D                                 = 2201,
		DEFRAG                                   = 2304,
		PIDGENX                                  = 2561
	}
}