' ======================================================================
'	Vanguard Systems Screwdriver Control Library
'	Public Definitions
' ======================================================================

' ----------------------------------------------------------------------
'	Error Codes
' ----------------------------------------------------------------------

' No error.
#define VGD_PF_NO_ERR 0

' Bad connection id
#define VGD_PF_ERR_BAD_CONNECTION_ID 1

' Failed to open / read / write a file.
#define VGD_PF_ERR_FILE 2

' Failed to open / read / write a network channel.
#define VGD_PF_ERR_NETWORK 3

' Cannot connect any more.
#define VGD_PF_ERR_NO_MORE_CONNECTION 4

' Bad operation.
#define VGD_PF_ERR_BAD_OPERATION 5

' Bad definition file.
#define VGD_PF_ERR_BAD_DEFINITION_FILE 6

' Maximum number of log files is zero
#define VGD_PF_ERR_MAX_IS_ZERO 7

' Unknown tool type.
#define VGD_PF_ERR_UNKNOWN_TOOL_TYPE 8

' ----------------------------------------------------------------------
'	Operation on VGD_PF_ERR_NETWORK
'	This value is recorded in the error log.
' ----------------------------------------------------------------------

' Unknown or common part of operations
#define VGD_PF_OP_UNKNOWN 0

' Connect / Disconnect
#define VGD_PF_OP_CONNECT 1

' Stop
#define VGD_PF_OP_STOP 2

' Tighten
#define VGD_PF_OP_TIGHTEN 3

' Loosen
#define VGD_PF_OP_LOOSEN 4

' Free run
#define VGD_PF_OP_FREERUN 5

' Check status
#define VGD_PF_OP_CHECK_STATUS 6

' Get tightening result
#define VGD_PF_OP_GET_TIGHTENING_RESULT 7

' Get torque data
#define VGD_PF_OP_GET_TORQUE_DATA 8

' Download program
#define VGD_PF_OP_DOWNLOAD_PROGRAM 9

' Upload program
#define VGD_PF_OP_UPLOAD_PROGRAM 10

' Download network configuration
#define VGD_PF_OP_DOWNLOAD_NETCONF 11

' Upload network configuration
#define VGD_PF_OP_UPLOAD_NETCONF 12

' Download system information
#define VGD_PF_OP_DOWNLOAD_SYSINFO 13
