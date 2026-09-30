' ======================================================================
'	Vanguard Systems Screwdriver Control Library
'	Internal Definitions
' ======================================================================
'
' ----------------------------------------------------------------------
'	Specifications
' ----------------------------------------------------------------------
' Maximum number of connections
#define MAX_NUM_CONNECTIONS 4
#define Lib2692473628_G3 3	' = MAX_NUM_CONNECTIONS - 1
' Maximum number of "unknown" (means not existing in result log) wave form files
#define MAX_NUM_FILES_UNKNOWN 10
' ----------------------------------------------------------------------
'	File Field Offsets
' ----------------------------------------------------------------------
' Logging info block
#define OFFSET_LOGGING 16
#define OFFSET_LOGGING_STORAGE_TYPE (OFFSET_LOGGING + 1)
#define OFFSET_LOGGING_FOLDER (OFFSET_LOGGING + 2)
#define OFFSET_LOGGING_MAX_NUM_FILES_PC (OFFSET_LOGGING + 203)
#define OFFSET_LOGGING_MAX_NUM_FILES_USB (OFFSET_LOGGING + 207)
#define OFFSET_LOGGING_MAX_NUM_FILES_FL (OFFSET_LOGGING + 211)
' Network info block
#define OFFSET_NETWORK 231
#define OFFSET_NETWORK_TCPPORT (OFFSET_NETWORK + 4)
' Vacuum control info block
#define OFFSET_VACUUM 3465
#define OFFSET_VACUUM_IOLABEL_ON (OFFSET_VACUUM + 0)
#define OFFSET_VACUUM_IOLABEL_BREAK_ON (OFFSET_VACUUM + 33)
#define OFFSET_VACUUM_IOLABEL_SUCTION (OFFSET_VACUUM + 66)
' ----------------------------------------------------------------------
'	Modbus Register Addresses (Input)
' ----------------------------------------------------------------------
#define REG_TOOLTYPE &H04
#define REG_ID &H05
#define REG_CAN_GET_TIGHTENING_RESULT &H30
#define REG_GET_TIGHTENING_RESULT &H31
#define REG_NUM_TORQUES &H3F
#define REG_TORQUES &H40
' ----------------------------------------------------------------------
'	Modbus Register Addresses (Holding)
' ----------------------------------------------------------------------
#define REG_SAMPLING_ANGLE &HB8
#define REG_SELECT_PROGRAM &HC0
#define REG_REQUEST_EXECUTE &HC1
#define REG_DO &HC2
#define REG_DI &HC3
#define REG_ENABLE_REMOTE &HC4
' ----------------------------------------------------------------------
'	Modbus Register Values
' ----------------------------------------------------------------------
' REG_REQUEST_EXECUTE
#define VAL_CLEAR_CAN_GET_TIGHTEN_DATA 1
' REG_DO
#define VAL_DO_END &B0001
#define VAL_DO_BUSY &B0010
#define VAL_DO_ERR &B1000
#define VAL_DO_ERRCODE_MASK &B11110000
#define VAL_DO_ERRCODE_SHIFT 4
' REG_DI
#define VAL_DI_PROGRAM_MASK &B00001111
#define VAL_DI_PROGRAM_SHIFT 0
#define VAL_DI_FSN &B0010000
#define VAL_DI_LSN &B0100000
#define VAL_DI_RTN &B1000000
' REG_ENABLE_REMOTE
#define VAL_DIO_DISABLE 0
#define VAL_ENABLE_DI &B01
#define VAL_ENABLE_DO &B10
